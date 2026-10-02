using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CrescentHawksTools.Cli;
using RevengeTools.Formats.Maps;
using RevengeTools.Installation;

namespace RevengeTools.Formats.Scenes;

public sealed class SceneSummary
{
    public required string SourceFile { get; init; }
    public required int StoredLength { get; init; }
    public required int ScriptLength { get; init; }
    public required int InstructionLength { get; init; }
    public required int ScriptReservedWord { get; init; }
    public required int MessageCount { get; init; }
    public required int NonEmptyMessageCount { get; init; }
    public required int MapId { get; init; }
    public required string MapFile { get; init; }
    public required int IconSetId { get; init; }
    public required string IconSetFile { get; init; }
    public required int TrailerByte { get; init; }
    public required int FinalPaddingLength { get; init; }
    public required string MetadataHex { get; init; }
    public required string ScriptSha256 { get; init; }
    public required int ResourceTableIndex { get; init; }
}

public sealed class SceneEvidenceExportResult
{
    public required string OutputDirectory { get; init; }
    public required int SceneCount { get; init; }
    public required int MetadataColumnCount { get; init; }
}

public static class SceneReports
{
    private static readonly string[] GenericMessageCategoryLabels =
    [
        "unit_damage_warning",
        "unit_shutdown",
        "cancel_overburn",
        "target_spotted_engaging",
        "target_spotted_holding",
        "unknown_unused",
        "target_spotted_holding_fire",
        "arrived_at_objective",
        "target_status_change",
        "target_destroyed",
        "unsafe_engagement_warning"
    ];

    public static SceneSummary Summarize(string sourceFile, SceneFile scene) => new()
    {
        SourceFile = Path.GetFileName(sourceFile),
        StoredLength = scene.StoredLength,
        ScriptLength = scene.ScriptBytes.Length,
        InstructionLength = scene.InstructionBytes.Length,
        ScriptReservedWord = scene.ScriptReservedWord,
        MessageCount = scene.Messages.Count,
        NonEmptyMessageCount = scene.Messages.Count(message => message.Text.Length != 0),
        MapId = scene.MapId,
        MapFile = scene.MapFileName,
        IconSetId = scene.IconSetId,
        IconSetFile = scene.IconSetFileName,
        TrailerByte = scene.TrailerByte,
        FinalPaddingLength = scene.FinalPaddingLength,
        MetadataHex = Convert.ToHexString(scene.Metadata),
        ScriptSha256 = Convert.ToHexString(SHA256.HashData(scene.ScriptBytes)).ToLowerInvariant(),
        ResourceTableIndex = SceneFile.ResourceTableIndex
    };

    public static IReadOnlyList<SceneSummary> ExportAll(GameInstallation installation,
        string outputFile, bool overwrite)
    {
        string[] files = Directory.EnumerateFiles(installation.DirectoryPath, "SCENE*.DAT")
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase).ToArray();
        var summaries = files.Select(path =>
        {
            string name = Path.GetFileName(path);
            return Summarize(name, SceneFile.Parse(File.ReadAllBytes(path), name));
        }).ToList();
        OutputFile.WriteText(outputFile,
            JsonSerializer.Serialize(summaries, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            overwrite);
        return summaries.AsReadOnly();
    }

    public static SceneEvidenceExportResult ExportEvidence(GameInstallation installation,
        string outputDirectory, bool overwrite)
    {
        string[] files = Directory.EnumerateFiles(installation.DirectoryPath, "SCENE*.DAT")
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase).ToArray();
        var parsed = files.Select(path => new
        {
            Path = path,
            Name = Path.GetFileName(path),
            Scene = SceneFile.Parse(File.ReadAllBytes(path), Path.GetFileName(path))
        }).ToArray();

        foreach (var item in parsed)
        {
            string sceneDirectory = Path.Combine(outputDirectory,
                Path.GetFileNameWithoutExtension(item.Name));
            OutputFile.WriteBytes(Path.Combine(sceneDirectory, "metadata.bin"), item.Scene.Metadata, overwrite);
            OutputFile.WriteBytes(Path.Combine(sceneDirectory, "script.bin"), item.Scene.ScriptBytes, overwrite);
            OutputFile.WriteText(Path.Combine(sceneDirectory, "metadata.hex.txt"),
                FormatHex(item.Scene.Metadata, SceneFile.MetadataOffset), overwrite);
            OutputFile.WriteText(Path.Combine(sceneDirectory, "script.hex.txt"),
                FormatHex(item.Scene.ScriptBytes, SceneFile.DataOffset), overwrite);
            OutputFile.WriteText(Path.Combine(sceneDirectory, "script-prefix.txt"),
                FormatKnownInstructionPrefix(item.Scene), overwrite);
            OutputFile.WriteText(Path.Combine(sceneDirectory, "script-control-flow.txt"),
                FormatKnownControlFlow(item.Scene), overwrite);
            OutputFile.WriteText(Path.Combine(sceneDirectory, "offsets.csv"),
                FormatOffsets(item.Scene), overwrite);
            OutputFile.WriteText(Path.Combine(sceneDirectory, "messages.txt"),
                FormatMessages(item.Scene), overwrite);
        }

        IReadOnlyList<SceneSummary> summaries = parsed
            .Select(item => Summarize(item.Name, item.Scene)).ToArray();
        OutputFile.WriteText(Path.Combine(outputDirectory, "manifest.json"),
            JsonSerializer.Serialize(summaries, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            overwrite);
        OutputFile.WriteText(Path.Combine(outputDirectory, "metadata-columns.csv"),
            FormatMetadataColumns(parsed.Select(item => (item.Name, item.Scene)).ToArray()), overwrite);

        return new SceneEvidenceExportResult
        {
            OutputDirectory = Path.GetFullPath(outputDirectory),
            SceneCount = parsed.Length,
            MetadataColumnCount = SceneFile.MetadataLength
        };
    }

    private static string FormatOffsets(SceneFile scene)
    {
        var output = new StringBuilder("index,file_offset,span_length,region\n");
        for (int index = 0; index < scene.Offsets.Count; index++)
        {
            int end = index + 1 < scene.Offsets.Count ? scene.Offsets[index + 1] : scene.StoredLength + 1;
            output.Append(index).Append(",0x").Append(scene.Offsets[index].ToString("X4"))
                .Append(',').Append(end - scene.Offsets[index]).Append(',')
                .AppendLine(index == 0 ? "instruction_block" : $"message_{index - 1:D2}");
        }
        return output.ToString();
    }

    private static string FormatMessages(SceneFile scene)
    {
        var output = new StringBuilder();
        foreach (SceneMessage message in scene.Messages)
            output.Append('[').Append(message.Index.ToString("D2")).Append("] 0x")
                .Append(message.Offset.ToString("X4")).Append(' ')
                .Append(MessageBankLabel(message.Index)).Append(' ')
                .AppendLine(message.Text.Replace("\r", "\\r").Replace("\n", "\\n"));
        return output.ToString();
    }

    private static string FormatKnownInstructionPrefix(SceneFile scene)
    {
        SceneInstructionPrefix prefix = SceneInstructionDecoder.DecodeKnownPrefix(scene.InstructionBytes);
        var output = new StringBuilder();
        foreach (SceneInstruction instruction in prefix.Instructions)
        {
            output.Append("0x").Append(instruction.FileOffset.ToString("X4")).Append("  ")
                .Append(instruction.Opcode.ToString("X2"));
            foreach (byte operand in instruction.Operands)
                output.Append(' ').Append(operand.ToString("X2"));
            output.Append("  ").Append(instruction.Name).Append("  [")
                .Append(instruction.HandlerAddress).Append(']');
            AppendSceneEventMessage(output, scene, instruction);
            output.AppendLine();
        }
        output.Append("Consumed ").Append(prefix.ConsumedByteCount).Append(" of ")
            .Append(scene.InstructionBytes.Length).Append(" bytes; ")
            .Append(prefix.RemainingByteCount).Append(" remain");
        if (prefix.NextUnknownOpcode is byte unknown)
            output.Append("; next unknown opcode 0x").Append(unknown.ToString("X2"));
        output.AppendLine(".");
        return output.ToString();
    }

    private static string FormatKnownControlFlow(SceneFile scene)
    {
        SceneControlFlowDecode decode = SceneInstructionDecoder.DecodeKnownControlFlow(scene.InstructionBytes);
        var output = new StringBuilder();
        foreach (SceneInstruction instruction in decode.Instructions)
        {
            output.Append("0x").Append(instruction.FileOffset.ToString("X4")).Append("  ")
                .Append(instruction.Opcode.ToString("X2"));
            foreach (byte operand in instruction.Operands)
                output.Append(' ').Append(operand.ToString("X2"));
            output.Append("  ").Append(instruction.Name);
            if (instruction.BranchTargetInstructionOffset is int target)
                output.Append(" -> 0x").Append((SceneFile.DataOffset + 2 + target).ToString("X4"));
            output.Append("  [").Append(instruction.HandlerAddress).Append(']');
            AppendSceneEventMessage(output, scene, instruction);
            output.AppendLine();
        }
        foreach ((int offset, byte opcode) in decode.UnknownOpcodes)
            output.Append("0x").Append((SceneFile.DataOffset + 2 + offset).ToString("X4"))
                .Append("  ").Append(opcode.ToString("X2")).AppendLine("  UNKNOWN");
        output.Append("Covered ").Append(decode.CoveredByteCount).Append(" of ")
            .Append(scene.InstructionBytes.Length).Append(" bytes across reachable known paths; ")
            .Append(decode.UnknownOpcodes.Count).AppendLine(" unknown entry point(s).");
        return output.ToString();
    }

    private static string MessageBankLabel(int messageIndex)
    {
        if (messageIndex < SceneFile.SceneEventMessageBase)
        {
            int category = messageIndex % SceneFile.GenericMessageCategoryCount;
            return $"speaker_group_{messageIndex / SceneFile.GenericMessageCategoryCount} " +
                $"category_{category:D2}_{GenericMessageCategoryLabels[category]}";
        }
        return $"scene_event_slot_{messageIndex - SceneFile.SceneEventMessageBase}";
    }

    private static void AppendSceneEventMessage(StringBuilder output, SceneFile scene,
        SceneInstruction instruction)
    {
        if (instruction.SceneEventMessageIndex is not int messageIndex ||
            messageIndex >= scene.Messages.Count)
            return;
        string text = scene.Messages[messageIndex].Text
            .Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"");
        output.Append(" ; message[").Append(messageIndex).Append("] \"")
            .Append(text).Append('"');
    }

    private static string FormatMetadataColumns(IReadOnlyList<(string Name, SceneFile Scene)> scenes)
    {
        var output = new StringBuilder("metadata_offset,file_offset,distinct_count,minimum,maximum,values_hex,known_meaning\n");
        for (int offset = 0; offset < SceneFile.MetadataLength; offset++)
        {
            byte[] values = scenes.Select(item => item.Scene.Metadata[offset]).Distinct().Order().ToArray();
            string meaning = offset switch
            {
                SceneFile.MapIdMetadataOffset => "map selector",
                SceneFile.IconSetIdMetadataOffset => "icon-set selector",
                _ => string.Empty
            };
            output.Append("0x").Append(offset.ToString("X2")).Append(",0x")
                .Append((SceneFile.MetadataOffset + offset).ToString("X4")).Append(',')
                .Append(values.Length).Append(",0x").Append(values[0].ToString("X2"))
                .Append(",0x").Append(values[^1].ToString("X2")).Append(',')
                .Append(string.Join('|', values.Select(value => value.ToString("X2"))))
                .Append(',').AppendLine(meaning);
        }
        return output.ToString();
    }

    private static string FormatHex(byte[] data, int baseOffset)
    {
        var output = new StringBuilder();
        for (int row = 0; row < data.Length; row += 16)
        {
            int length = Math.Min(16, data.Length - row);
            output.Append((baseOffset + row).ToString("X4")).Append("  ");
            for (int column = 0; column < 16; column++)
                output.Append(column < length ? data[row + column].ToString("X2") + ' ' : "   ");
            output.Append(' ');
            for (int column = 0; column < length; column++)
            {
                byte value = data[row + column];
                output.Append(value is >= 0x20 and <= 0x7E ? (char)value : '.');
            }
            output.AppendLine();
        }
        return output.ToString();
    }

    public static MapExportResult ExportSceneMap(GameInstallation installation, string sceneFile,
        string? paletteFile, string outputFile, bool overwrite)
    {
        SceneFile scene = SceneFile.Parse(File.ReadAllBytes(installation.ResolveFile(sceneFile)), sceneFile);
        return MapExporter.Export(installation, scene.MapFileName, scene.IconSetFileName,
            paletteFile, outputFile, overwrite);
    }
}
