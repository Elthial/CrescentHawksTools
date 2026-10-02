using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using InceptionTools.Binary;
using InceptionTools.Installation;
using InceptionTools.Audio;

namespace InceptionTools.Inspection
{
    public sealed class BldDisassemblyItem
    {
        public int PayloadOffset { get; set; }
        public int FileOffset { get; set; }
        public string Kind { get; set; }
        public int? Opcode { get; set; }
        public string Mnemonic { get; set; }
        public string Operands { get; set; }
        public int[] DecodedBytes { get; set; }
        public int? BranchTarget { get; set; }
        public bool? BranchTargetInRange { get; set; }
        public int[] InlineBranchTargets { get; set; }
        public string Text { get; set; }
    }

    public sealed class BldDisassembly
    {
        public string FileName { get; set; }
        public int FileLength { get; set; }
        public int StoredPayloadLength { get; set; }
        public int StartPayloadOffset { get; set; }
        public int[] StoredPayloadBytes { get; set; }
        public int[] DecodedPayloadBytes { get; set; }
        public bool StoppedAtUnresolvedInlineTable { get; set; }
        public List<BldDisassemblyItem> Items { get; set; } = new List<BldDisassemblyItem>();
    }

    public static class BldDisassembler
    {
        public static BldDisassembly Inspect(GameInstallation installation, string fileName, int startOffset = 0)
        {
            string path = installation.ResolveFile(fileName);
            if (!Path.GetExtension(path).Equals(".BLD", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("BLD disassembly requires a .BLD file.", nameof(fileName));

            byte[] file = File.ReadAllBytes(path);
            var fileReader = new BoundedBinaryReader(file, Path.GetFileName(path));
            fileReader.RequireRange(0, 2, "BLD length prefix");
            int storedLength = fileReader.ReadUInt16LittleEndian(0, "BLD payload length");
            if (storedLength != file.Length - 2)
                throw new InvalidDataException("BLD payload length " + storedLength + " does not match file length minus two (" + (file.Length - 2) + ").");
            if (startOffset < 0 || startOffset > storedLength)
                throw new ArgumentOutOfRangeException(nameof(startOffset), "Start offset is outside the decoded payload.");

            byte[] stored = fileReader.ReadBytes(2, storedLength, "stored BLD payload");
            byte[] decoded = stored.Select(DecodeByte).ToArray();
            var result = new BldDisassembly
            {
                FileName = Path.GetFileName(path),
                FileLength = file.Length,
                StoredPayloadLength = storedLength,
                StartPayloadOffset = startOffset,
                StoredPayloadBytes = stored.Select(value => (int)value).ToArray(),
                DecodedPayloadBytes = decoded.Select(value => (int)value).ToArray()
            };
            Disassemble(decoded, startOffset, result);
            return result;
        }

        public static string WriteJson(BldDisassembly result)
        {
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }

        public static string WriteText(BldDisassembly result)
        {
            var output = new StringBuilder();
            output.AppendLine("BLD: " + result.FileName + " (payload 0x" + result.StoredPayloadLength.ToString("X4") + " bytes)");
            output.AppendLine("Offsets: decoded payload; add 0x02 for file offsets");
            foreach (BldDisassemblyItem item in result.Items)
            {
                output.Append(item.PayloadOffset.ToString("X4")).Append("  ")
                    .Append(FormatBytes(item.DecodedBytes).PadRight(32))
                    .Append(' ').Append(item.Mnemonic);
                if (!string.IsNullOrEmpty(item.Operands))
                    output.Append(' ').Append(item.Operands);
                if (item.Text != null)
                    output.Append(" \"").Append(EscapeText(item.Text)).Append('"');
                if (item.BranchTarget.HasValue)
                    output.Append(item.BranchTargetInRange == true ? "" : " [TARGET OUT OF RANGE]");
                output.AppendLine();
            }
            return output.ToString();
        }

        private static void Disassemble(byte[] data, int offset, BldDisassembly result)
        {
            var reader = new BoundedBinaryReader(data, "decoded BLD payload");
            while (offset < data.Length)
            {
                int start = offset;
                byte opcode = data[offset];
                if (opcode < 0xE4)
                {
                    while (offset < data.Length && data[offset] < 0xE4)
                        offset++;
                    Add(result, data, start, offset - start, "data", null, "DATA", "");
                    continue;
                }

                if (opcode == 0xF3 || opcode == 0xF9)
                {
                    reader.RequireRange(start, 2, "variable-table instruction");
                    string mnemonic = opcode == 0xF3 ? "BRANCH_STATE_TABLE" : "MENU_BRANCH_TABLE";
                    int tableOffset = start + 2;
                    var targets = new List<int>();

                    // The original interpreter has no count operand: F3 indexes this table with a
                    // state byte and F9 indexes it with the menu routine's runtime result. Across
                    // all 36 tables in the local 26-file BLD corpus, every table is a consecutive
                    // run of in-payload absolute offsets and the following word is out of range.
                    // Keep the inference explicit in output so a different executable/data profile
                    // can reject the rule rather than silently treating it as part of the format.
                    while (tableOffset + targets.Count * 2 + 1 < data.Length)
                    {
                        int targetOffset = tableOffset + targets.Count * 2;
                        int target = reader.ReadUInt16LittleEndian(targetOffset, "inline branch target");
                        if (target >= data.Length)
                            break;
                        targets.Add(target);
                    }

                    if (targets.Count == 0)
                    {
                        string unresolvedOperand = (opcode == 0xF3 ? "stateIndex=" : "menuId=") + HexByte(data[start + 1]) +
                            ", inline target-table length unresolved";
                        Add(result, data, start, 2, "instruction", opcode, mnemonic, unresolvedOperand);
                        if (start + 2 < data.Length)
                            Add(result, data, start + 2, data.Length - start - 2, "ambiguous-tail", null,
                                "UNRESOLVED_INLINE_TABLE_AND_FOLLOWING_DATA", "resume with --offset after independently establishing a target");
                        result.StoppedAtUnresolvedInlineTable = true;
                        return;
                    }

                    string operand = (opcode == 0xF3 ? "stateIndex=" : "menuId=") + HexByte(data[start + 1]) +
                        ", targets=" + string.Join(",", targets.Select(value => "0x" + value.ToString("X4"))) +
                        ", count=" + targets.Count + " [boundary inferred: consecutive in-range words]";
                    BldDisassemblyItem tableItem = CreateItem(data, start, 2 + targets.Count * 2,
                        "instruction", opcode, mnemonic, operand);
                    tableItem.InlineBranchTargets = targets.ToArray();
                    result.Items.Add(tableItem);
                    offset += 2 + targets.Count * 2;
                    continue;
                }

                int length = FixedLength(opcode);
                if (opcode == 0xFC)
                {
                    int terminator = reader.IndexOf(0, start + 1, "FC text");
                    if (terminator < 0)
                        throw new InvalidDataException("Unterminated FC text at decoded payload offset 0x" + start.ToString("X4") + ".");
                    length = terminator - start + 1;
                }
                reader.RequireRange(start, length, "opcode 0x" + opcode.ToString("X2"));
                BldDisassemblyItem item = DecodeInstruction(reader, data, start, opcode, length);
                result.Items.Add(item);
                offset += length;
            }
        }

        private static BldDisassemblyItem DecodeInstruction(BoundedBinaryReader reader, byte[] data, int offset, byte opcode, int length)
        {
            string mnemonic;
            string operands = "";
            int? target = null;
            string text = null;
            switch (opcode)
            {
                case 0xE4:
                    mnemonic = "PLAY_SOUND";
                    int soundId = data[offset + 1];
                    operands = "soundId=" + HexByte(data[offset + 1]);
                    if (SoundEffectCatalog.TryGet(soundId, out SoundEffectDefinition effect))
                        operands += " (" + effect.Name + ")";
                    break;
                case 0xE5: mnemonic = "ADD_C_BILLS"; operands = "amount=" + reader.ReadInt16LittleEndian(offset + 1, "C-Bill delta"); break;
                case 0xE6: mnemonic = "SET_COORDINATES"; operands = "x=" + HexWord(reader, offset + 1) + ", y=" + HexWord(reader, offset + 3); break;
                case 0xE7: mnemonic = "BRANCH_IF_X_EQUALS"; target = reader.ReadUInt16LittleEndian(offset + 3); operands = "x=" + HexWord(reader, offset + 1) + ", target=" + HexWord(reader, offset + 3); break;
                case 0xE8: mnemonic = "BRANCH_IF_RANDOM_MASK"; target = reader.ReadUInt16LittleEndian(offset + 2); operands = "mask=" + HexByte(data[offset + 1]) + ", target=" + HexWord(reader, offset + 2); break;
                case 0xE9: mnemonic = "RECRUIT_CRESCENT_HAWK"; operands = "specialtySkill=" + HexByte(data[offset + 1]); break;
                case 0xEA: mnemonic = "CONDITIONAL_SCENE_ACTION"; operands = "scene=" + HexByte(data[offset + 1]) + ", argument=" + HexByte(data[offset + 2]); break;
                case 0xEB: mnemonic = "BRANCH_IF_D451"; target = reader.ReadUInt16LittleEndian(offset + 1); operands = "target=" + HexWord(reader, offset + 1); break;
                case 0xEC: mnemonic = "BRANCH_IF_D450"; target = reader.ReadUInt16LittleEndian(offset + 1); operands = "target=" + HexWord(reader, offset + 1); break;
                case 0xED: mnemonic = "BRANCH_IF_PARTY_SKILL"; target = reader.ReadUInt16LittleEndian(offset + 3); operands = "skill=" + data[offset + 1] + ", minimum=" + data[offset + 2] + ", target=" + HexWord(reader, offset + 3); break;
                case 0xEE: mnemonic = "SUBTRACT_C_BILLS"; operands = "amount=" + reader.ReadUInt16LittleEndian(offset + 1); break;
                case 0xEF: mnemonic = "BRANCH_IF_C_BILLS_AT_LEAST"; target = reader.ReadUInt16LittleEndian(offset + 3); operands = "amount=" + reader.ReadUInt16LittleEndian(offset + 1) + ", target=" + HexWord(reader, offset + 3); break;
                case 0xF0: mnemonic = "SET_TEXT_LAYOUT"; operands = "left=" + data[offset + 1] + ", right=" + data[offset + 2]; break;
                case 0xF1: mnemonic = "ADD_STATE_BYTE"; operands = "index=" + HexByte(data[offset + 1]) + ", value=" + HexByte(data[offset + 2]); break;
                case 0xF2: mnemonic = "TIMED_WAIT_INPUT_CHECK"; break;
                case 0xF4: mnemonic = "SET_STATE_BYTE"; operands = "index=" + HexByte(data[offset + 1]) + ", value=" + HexByte(data[offset + 2]); break;
                case 0xF5:
                    int action = data[offset + 1];
                    mnemonic = action switch
                    {
                        0x1E => "RECRUIT_REX_AND_START_KURITA_AMBUSH",
                        0x23 => "RUN_ARENA_MECH_COMBAT",
                        0x28 => "RUN_JAILBREAK_MISSION",
                        _ => "CALL_ACTION_DISPATCHER"
                    };
                    operands = "action=" + HexByte(data[offset + 1]);
                    break;
                case 0xF6: mnemonic = "PROMPT_YES_NO_BRANCH"; target = reader.ReadUInt16LittleEndian(offset + 1); operands = "target=" + HexWord(reader, offset + 1); break;
                case 0xF7: mnemonic = "BRANCH_IF_STATE_NONZERO"; target = reader.ReadUInt16LittleEndian(offset + 2); operands = "index=" + HexByte(data[offset + 1]) + ", target=" + HexWord(reader, offset + 2); break;
                case 0xF8: mnemonic = "BRANCH"; target = reader.ReadUInt16LittleEndian(offset + 1); operands = "target=" + HexWord(reader, offset + 1); break;
                case 0xFA: mnemonic = "DRAW_MENU_BORDER"; operands = "borderId=" + HexByte(data[offset + 1]); break;
                case 0xFB: mnemonic = "WAIT_FOR_KEY"; break;
                case 0xFC: mnemonic = "DISPLAY_TEXT"; text = Encoding.ASCII.GetString(data, offset + 1, length - 2); break;
                case 0xFD: mnemonic = "REDRAW_SIDEBAR"; break;
                case 0xFE: mnemonic = "APPLY_LAYOUT"; operands = "layoutId=" + HexByte(data[offset + 1]); break;
                case 0xFF: mnemonic = "EXIT"; break;
                default: throw new InvalidDataException("Unknown BLD opcode 0x" + opcode.ToString("X2") + ".");
            }
            var item = CreateItem(data, offset, length, "instruction", opcode, mnemonic, operands);
            item.BranchTarget = target;
            item.BranchTargetInRange = target.HasValue ? target.Value < data.Length : (bool?)null;
            item.Text = text;
            return item;
        }

        private static int FixedLength(byte opcode)
        {
            switch (opcode)
            {
                case 0xE4: case 0xE9: case 0xF5: case 0xFA: case 0xFE: return 2;
                case 0xE5: case 0xEA: case 0xEB: case 0xEC: case 0xEE:
                case 0xF0: case 0xF1: case 0xF4: case 0xF6: case 0xF8: return 3;
                case 0xE8: case 0xF7: return 4;
                case 0xE6: case 0xE7: case 0xED: case 0xEF: return 5;
                case 0xF2: case 0xFB: case 0xFD: case 0xFF: return 1;
                case 0xFC: return 0;
                default: throw new InvalidDataException("Unsupported BLD opcode 0x" + opcode.ToString("X2") + ".");
            }
        }

        private static void Add(BldDisassembly result, byte[] data, int offset, int length, string kind, int? opcode, string mnemonic, string operands)
        {
            result.Items.Add(CreateItem(data, offset, length, kind, opcode, mnemonic, operands));
        }

        private static BldDisassemblyItem CreateItem(byte[] data, int offset, int length, string kind, int? opcode, string mnemonic, string operands)
        {
            return new BldDisassemblyItem
            {
                PayloadOffset = offset,
                FileOffset = offset + 2,
                Kind = kind,
                Opcode = opcode,
                Mnemonic = mnemonic,
                Operands = operands,
                DecodedBytes = data.Skip(offset).Take(length).Select(value => (int)value).ToArray()
            };
        }

        private static byte DecodeByte(byte stored) => (byte)(((stored + 0x29) & 0xFF) ^ 0xE9);
        private static string HexByte(byte value) => "0x" + value.ToString("X2");
        private static string HexWord(BoundedBinaryReader reader, int offset) => "0x" + reader.ReadUInt16LittleEndian(offset).ToString("X4");

        private static string EscapeText(string value)
        {
            var output = new StringBuilder();
            foreach (char character in value)
            {
                if (character == '\r') output.Append("\\r");
                else if (character == '\n') output.Append("\\n");
                else if (character == '\t') output.Append("\\t");
                else if (character == '"') output.Append("\\\"");
                else if (character >= 0x20 && character <= 0x7E) output.Append(character);
                else output.Append("\\x").Append(((int)character).ToString("X2"));
            }
            return output.ToString();
        }

        private static string FormatBytes(int[] bytes)
        {
            const int shown = 8;
            string prefix = string.Join(" ", bytes.Take(shown).Select(value => value.ToString("X2")));
            return bytes.Length <= shown ? prefix : prefix + " ... (" + bytes.Length + " bytes)";
        }
    }
}
