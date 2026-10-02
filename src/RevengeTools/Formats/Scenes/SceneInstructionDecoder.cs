namespace RevengeTools.Formats.Scenes;

public sealed class SceneInstruction
{
    public required int FileOffset { get; init; }
    public required byte Opcode { get; init; }
    public required string Name { get; init; }
    public required string HandlerAddress { get; init; }
    public required byte[] Operands { get; init; }
    public bool EndsLinearDecode { get; init; }
    public int Length => 1 + Operands.Length;
    public int InstructionOffset => FileOffset - SceneFile.DataOffset - 2;
    public int? BranchTargetInstructionOffset { get; init; }
    public int? SceneEventSlot => Opcode is >= 0xDC and <= 0xE1
        ? 0xE1 - Opcode
        : null;
    public int? SceneEventMessageIndex => SceneEventSlot is int slot
        ? SceneFile.SceneEventMessageBase + slot
        : null;
}

public sealed class SceneInstructionPrefix
{
    public required IReadOnlyList<SceneInstruction> Instructions { get; init; }
    public required int ConsumedByteCount { get; init; }
    public required int RemainingByteCount { get; init; }
    public byte? NextUnknownOpcode { get; init; }
}

public sealed class SceneControlFlowDecode
{
    public required IReadOnlyList<SceneInstruction> Instructions { get; init; }
    public required IReadOnlyDictionary<int, byte> UnknownOpcodes { get; init; }
    public required int CoveredByteCount { get; init; }
}

public static class SceneInstructionDecoder
{
    public static SceneInstructionPrefix DecodeKnownPrefix(byte[] instructionBytes)
    {
        ArgumentNullException.ThrowIfNull(instructionBytes);
        var instructions = new List<SceneInstruction>();
        int cursor = 0;
        byte? unknownOpcode = null;

        while (cursor < instructionBytes.Length)
        {
            SceneInstruction? instruction = DecodeOne(instructionBytes, cursor);
            if (instruction is null)
            {
                unknownOpcode = instructionBytes[cursor];
                break;
            }
            instructions.Add(instruction);
            cursor += instruction.Length;
            if (instruction.EndsLinearDecode)
                break;
        }

        return new SceneInstructionPrefix
        {
            Instructions = instructions.AsReadOnly(),
            ConsumedByteCount = cursor,
            RemainingByteCount = instructionBytes.Length - cursor,
            NextUnknownOpcode = unknownOpcode
        };
    }

    public static SceneControlFlowDecode DecodeKnownControlFlow(byte[] instructionBytes)
    {
        ArgumentNullException.ThrowIfNull(instructionBytes);
        var instructions = new SortedDictionary<int, SceneInstruction>();
        var unknownOpcodes = new SortedDictionary<int, byte>();
        var pending = new Queue<int>();
        pending.Enqueue(0);

        while (pending.Count != 0)
        {
            int cursor = pending.Dequeue();
            while (cursor < instructionBytes.Length && !instructions.ContainsKey(cursor))
            {
                SceneInstruction? instruction = DecodeOne(instructionBytes, cursor);
                if (instruction is null)
                {
                    unknownOpcodes[cursor] = instructionBytes[cursor];
                    break;
                }
                instructions.Add(cursor, instruction);
                int fallthrough = cursor + instruction.Length;

                if (instruction.BranchTargetInstructionOffset is int target)
                {
                    if (target < 0 || target >= instructionBytes.Length)
                        throw new InvalidDataException($"SCENE branch at 0x{cursor:X4} targets 0x{target:X4}, outside the instruction block.");
                    pending.Enqueue(target);
                    if (instruction.Opcode != 0xF7 && fallthrough < instructionBytes.Length)
                        pending.Enqueue(fallthrough);
                    break;
                }
                if (instruction.Opcode == 0xFF)
                    break;
                if (instruction.Opcode < 0xCF || instruction.Opcode is 0xD8 or 0xD9 or 0xF6)
                {
                    if (fallthrough < instructionBytes.Length)
                        pending.Enqueue(fallthrough);
                    break;
                }
                cursor = fallthrough;
            }
        }

        return new SceneControlFlowDecode
        {
            Instructions = instructions.Values.ToArray(),
            UnknownOpcodes = unknownOpcodes,
            // Scripts deliberately branch into bytes that are operands on another path
            // (SCENEK's E2/DE tail is one observed example), so count the union of
            // covered offsets rather than double-counting overlapping instructions.
            CoveredByteCount = instructions
                .SelectMany(item => Enumerable.Range(item.Key, item.Value.Length))
                .Distinct()
                .Count()
        };
    }

    private static SceneInstruction? DecodeOne(byte[] instructionBytes, int cursor)
    {
        byte opcode = instructionBytes[cursor];
        if (opcode < 0xCF)
            return Create(cursor, opcode, "FinishInterpreterPassOnOutOfRangeOpcode", "1A1F:3A6A", [], true);
        if (opcode == 0xFF)
            return Create(cursor, opcode, "EndSceneScript", "terminator", [], true);

        int operandCount = opcode switch
        {
            0xCF => 1,
            0xD0 or 0xD2 or 0xD3 or 0xE3 or 0xE4 or 0xE5 or 0xEB => 2,
            0xD1 => 3,
            0xD4 => 1,
            0xD5 => 2,
            0xD8 or 0xD9 => 0,
            0xDA or 0xDB or 0xE2 => 3,
            0xD6 or 0xD7 or 0xE6 or 0xE7 or 0xF2 or 0xF3 => 4,
            >= 0xDC and <= 0xE1 => 1,
            0xEC or 0xED => 5,
            0xEF => 4,
            0xE8 => 1,
            0xE9 => 5,
            0xEA => 2,
            0xEE => 3,
            0xF0 => 4,
            0xF4 or 0xF5 => 3,
            0xF6 => 0,
            0xF7 => 2,
            0xF1 or 0xF8 => 3,
            0xF9 => 9,
            0xFA or 0xFC or 0xFD or 0xFE => 2,
            0xFB => RequireByte(instructionBytes, cursor + 2, opcode) == 0x18 ? 4 : 2,
            _ => -1
        };
        if (operandCount < 0)
            return null;
        if (cursor + 1 + operandCount > instructionBytes.Length)
            throw new InvalidDataException($"SCENE opcode 0x{opcode:X2} at instruction offset 0x{cursor:X4} is truncated.");

        byte[] operands = instructionBytes[(cursor + 1)..(cursor + 1 + operandCount)];
        bool controlFlow = opcode is 0xD6 or 0xD7 or 0xDA or 0xE6 or 0xE7 or 0xEC or 0xED or 0xEF
            or 0xF0 or 0xF1 or 0xF2 or 0xF3 or 0xF7 or 0xF8;
        int? branchTarget = controlFlow
            ? operands[^2] | operands[^1] << 8
            : null;
        return Create(cursor, opcode, NameFor(opcode), HandlerFor(opcode), operands,
            controlFlow, branchTarget);
    }

    private static byte RequireByte(byte[] data, int offset, byte opcode)
    {
        if (offset >= data.Length)
            throw new InvalidDataException($"SCENE opcode 0x{opcode:X2} is truncated before its target operand.");
        return data[offset];
    }

    private static SceneInstruction Create(int relativeOffset, byte opcode, string name,
        string handlerAddress, byte[] operands, bool endsLinearDecode,
        int? branchTargetInstructionOffset = null) => new()
    {
        FileOffset = SceneFile.DataOffset + 2 + relativeOffset,
        Opcode = opcode,
        Name = name,
        HandlerAddress = handlerAddress,
        Operands = operands,
        EndsLinearDecode = endsLinearDecode,
        BranchTargetInstructionOffset = branchTargetInstructionOffset
    };

    private static string NameFor(byte opcode) => opcode switch
    {
        0xCF => "SetSceneGlobalFlagBySelector",
        0xD0 => "InitializeSceneRuntimeSlot",
        0xD1 => "InvokeSceneEffect19F5",
        0xD2 => "FillRuntimeBytesAE9D",
        0xD3 => "InitializeSpecialMapEvent",
        0xD4 => "SetCampaignMilestoneFlag",
        0xD5 => "GrantUnitOrCampaignUpgrade",
        0xD6 => "JumpIfUnitYNotEqual",
        0xD7 => "JumpIfUnitXNotEqual",
        0xD8 => "FinishInterpreterPassWithMode3",
        0xD9 => "FinishInterpreterPassWithMode2",
        0xDA => "JumpIfUnitAbsentOrWithdrawn",
        0xDB => "SetUnitSceneDestination",
        >= 0xDC and <= 0xE1 => $"ActivateSceneEventSlot{0xE1 - opcode}",
        0xE2 => "InvokeConditionalSceneEffect4923",
        0xE3 => "SetUnitGunneryTargetNumber",
        0xE4 => "SetUnitPilotId",
        0xE5 => "SetUnitPilotExperience",
        0xE6 => "JumpIfMapCellValueBelow3",
        0xE7 => "JumpIfMapCellValueNonzero",
        0xE8 => "AdvanceUnitSceneRoute",
        0xE9 => "SpawnScenarioUnit",
        0xEA => "AddUnitAmmunitionFamily",
        0xEB => "DamageUnitAmmunitionBin",
        0xEC => "JumpIfCampaignClockNotEqual",
        0xED => "JumpIfCampaignClockBefore",
        0xEE => "SetCampaignClock",
        0xEF => "JumpIfUnitLacksEquipmentType",
        0xF0 => "JumpIfUnitAmmunitionNonzero",
        0xF1 => "JumpIfUnitAbsentOrImmobile",
        0xF2 => "JumpIfUnitInternalPercentBelow",
        0xF3 => "JumpIfUnitDurabilityBelowPercent",
        0xF4 => "RepairUnitArmor",
        0xF5 => "DamageUnitArmor",
        0xF6 => "FinishInterpreterPass",
        0xF7 => "JumpRelative",
        0xF8 => "JumpIfUnitHasNoActiveOrder",
        0xF9 => "InitializeUnitMovementRoute",
        0xFA => "SetUnitMovementHeatExponent",
        0xFB => "SetUnitAttackTarget",
        0xFC => "SetUnitTacticalOrderMode",
        0xFD => "InstallUnitEquipment",
        0xFE => "RemoveUnitEquipment",
        _ => throw new ArgumentOutOfRangeException(nameof(opcode))
    };

    private static string HandlerFor(byte opcode) => opcode switch
    {
        0xCF => "1A1F:3A46",
        0xD0 => "1A1F:3A12",
        0xD1 => "1A1F:39D5",
        0xD2 => "1A1F:3997",
        0xD3 => "1A1F:393C",
        0xD4 => "1A1F:3889",
        0xD5 => "1A1F:37C6",
        0xD6 => "1A1F:3779",
        0xD7 => "1A1F:372C",
        0xD8 => "1A1F:371E",
        0xD9 => "1A1F:3710",
        0xDA => "1A1F:36C2",
        0xDB => "1A1F:35C4",
        >= 0xDC and <= 0xE1 => "1A1F:3579",
        0xE2 => "1A1F:352A",
        0xE3 => "1A1F:34F7",
        0xE4 => "1A1F:34C4",
        0xE5 => "1A1F:3491",
        0xE6 => "1A1F:343D",
        0xE7 => "1A1F:33EA",
        0xE8 => "1A1F:33D1",
        0xE9 => "1A1F:32B4",
        0xEA => "1A1F:3270",
        0xEB => "1A1F:31F5",
        0xEC => "1A1F:3184",
        0xED => "1A1F:30FA",
        0xEE => "1A1F:30C8",
        0xEF => "1A1F:3044",
        0xF0 => "1A1F:2FE7",
        0xF1 => "1A1F:2F8A",
        0xF2 => "1A1F:2ED4",
        0xF3 => "1A1F:2E66",
        0xF4 => "1A1F:2E23",
        0xF5 => "1A1F:2DB2",
        0xF6 => "1A1F:3A6A",
        0xF7 => "1A1F:2D96",
        0xF8 => "1A1F:2D5D",
        0xF9 => "1A1F:2C04",
        0xFA => "1A1F:2BE3",
        0xFB => "1A1F:2B9D",
        0xFC => "1A1F:2B7C",
        0xFD => "1A1F:2B4C",
        0xFE => "1A1F:2AE4",
        _ => throw new ArgumentOutOfRangeException(nameof(opcode))
    };
}
