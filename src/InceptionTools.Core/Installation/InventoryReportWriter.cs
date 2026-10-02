using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace InceptionTools.Installation;

public static class InventoryReportWriter
{
    public static string WriteText(InstallationInventoryReport report)
    {
        var output = new StringBuilder();
        output.AppendLine("Installation: " + report.InstallationPath);
        output.AppendLine("Located by: " + report.LocatedBy);
        output.AppendLine("Version: " + report.DetectedVersion);
        output.AppendLine("Hashes: " + (report.HashesIncluded ? "included" : "not requested"));
        output.AppendLine();
        output.AppendLine("STATUS\tREQUIRED\tCATEGORY\tLENGTH\tHEADER\tNAME");
        foreach (InstallationInventoryEntry file in report.Files)
        {
            output.Append(file.Validation).Append('\t')
                .Append(file.Required ? "yes" : "no").Append('\t')
                .Append(file.Category).Append('\t')
                .Append(file.Length?.ToString() ?? "-").Append('\t')
                .Append(file.HeaderHex ?? "-").Append('\t')
                .AppendLine(file.Name);
            if (!string.IsNullOrEmpty(file.Sha256))
                output.AppendLine("  sha256=" + file.Sha256);
        }

        int requiredMissing = report.Files.Count(file => file.Required && !file.Present);
        int invalid = report.Files.Count(file => file.Present &&
            (file.Validation.StartsWith("invalid", StringComparison.Ordinal) ||
             file.Validation.StartsWith("length-mismatch", StringComparison.Ordinal)));
        output.AppendLine();
        output.AppendLine("Required missing: " + requiredMissing + "; invalid: " + invalid);
        return output.ToString();
    }

    public static string WriteJson(InstallationInventoryReport report)
    {
        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    public static void SaveIfRequested(string content, string? outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            Console.Write(content);
            return;
        }

        string fullPath = Path.GetFullPath(outputPath);
        string? parent = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(parent))
            Directory.CreateDirectory(parent);
        File.WriteAllText(fullPath, content, new UTF8Encoding(false));
        Console.WriteLine("Inventory written to " + fullPath);
    }
}
