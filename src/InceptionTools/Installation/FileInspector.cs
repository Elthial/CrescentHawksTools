using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InceptionTools.Installation
{
    public sealed class FileInspectionResult
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long FileLength { get; set; }
        public string OffsetDomain { get; set; }
        public long StartOffset { get; set; }
        public byte[] Bytes { get; set; }
        public List<string> Facts { get; set; } = new List<string>();
    }

    public static class FileInspector
    {
        public const int MaximumByteCount = 0x1000;

        public static FileInspectionResult Inspect(
            GameInstallation installation,
            string fileName,
            long offset,
            int count,
            bool decodeBld)
        {
            ValidateRequest(fileName, offset, count);
            string path = installation.ResolveFile(fileName);

            bool isBld = Path.GetExtension(path).Equals(".BLD", StringComparison.OrdinalIgnoreCase);
            if (decodeBld && !isBld)
                throw new ArgumentException("--decode-bld can only be used with a .BLD file.");

            var info = new FileInfo(path);
            long sourceOffset = decodeBld ? offset + 2 : offset;
            long domainLength = decodeBld ? Math.Max(0, info.Length - 2) : info.Length;
            if (offset > domainLength)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset is beyond the selected data domain.");

            int readCount = (int)Math.Min(count, domainLength - offset);
            byte[] bytes = new byte[readCount];
            using (FileStream stream = File.OpenRead(path))
            {
                stream.Seek(sourceOffset, SeekOrigin.Begin);
                int total = 0;
                while (total < bytes.Length)
                {
                    int read = stream.Read(bytes, total, bytes.Length - total);
                    if (read == 0)
                        break;
                    total += read;
                }
            }

            if (decodeBld)
            {
                for (int index = 0; index < bytes.Length; index++)
                    bytes[index] = (byte)(((bytes[index] + 0x29) & 0xFF) ^ 0xE9);
            }

            var result = new FileInspectionResult
            {
                Name = Path.GetFileName(path),
                Path = path,
                FileLength = info.Length,
                OffsetDomain = decodeBld ? "decoded BLD payload (file+0x02)" : "raw file",
                StartOffset = offset,
                Bytes = bytes
            };
            AddFormatFacts(result, path, isBld);
            return result;
        }

        public static string WriteText(FileInspectionResult result)
        {
            var output = new StringBuilder();
            output.AppendLine("File: " + result.Name);
            output.AppendLine("Length: " + result.FileLength + " (0x" + result.FileLength.ToString("X") + ")");
            output.AppendLine("Offset domain: " + result.OffsetDomain);
            foreach (string fact in result.Facts)
                output.AppendLine("- " + fact);
            output.AppendLine();
            output.AppendLine("OFFSET    HEX                                              ASCII");

            for (int row = 0; row < result.Bytes.Length; row += 16)
            {
                int rowLength = Math.Min(16, result.Bytes.Length - row);
                output.Append((result.StartOffset + row).ToString("X8")).Append("  ");
                for (int column = 0; column < 16; column++)
                {
                    if (column < rowLength)
                        output.Append(result.Bytes[row + column].ToString("X2")).Append(' ');
                    else
                        output.Append("   ");
                }
                output.Append(' ');
                for (int column = 0; column < rowLength; column++)
                {
                    byte value = result.Bytes[row + column];
                    output.Append(value >= 0x20 && value <= 0x7E ? (char)value : '.');
                }
                output.AppendLine();
            }
            return output.ToString();
        }

        private static void ValidateRequest(string fileName, long offset, int count)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
            if (count < 1 || count > MaximumByteCount)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 1 and 0x1000 bytes.");
        }

        private static void AddFormatFacts(FileInspectionResult result, string path, bool isBld)
        {
            byte[] header = new byte[3];
            int read;
            using (FileStream stream = File.OpenRead(path))
                read = stream.Read(header, 0, header.Length);

            if (isBld && read >= 2)
            {
                int storedLength = header[0] | (header[1] << 8);
                result.Facts.Add("BLD stored payload length: " + storedLength +
                    (storedLength == result.FileLength - 2 ? " (matches file length - 2)" : " (MISMATCH)"));
            }
            else if ((path.EndsWith(".CMP", StringComparison.OrdinalIgnoreCase) ||
                      path.EndsWith(".ICN", StringComparison.OrdinalIgnoreCase)) && read >= 3)
            {
                int storedLength = header[0] | (header[1] << 8);
                result.Facts.Add("Stored payload length: " + storedLength);
                result.Facts.Add("Compression format byte: " + header[2]);
            }
            else if (Path.GetFileName(path).Equals("BTECH.EXE", StringComparison.OrdinalIgnoreCase))
            {
                result.Facts.Add(read >= 2 && header[0] == 'M' && header[1] == 'Z' ? "DOS MZ executable" : "MZ signature missing");
            }
            else if (Path.GetFileName(path).StartsWith("GAME", StringComparison.OrdinalIgnoreCase))
            {
                result.Facts.Add(result.FileLength == 0x0F49 ? "Recognized 0x0F49-byte save slot" : "Unrecognized save length");
            }
        }
    }
}
