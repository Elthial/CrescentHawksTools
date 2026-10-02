using System;
using System.IO;
using InceptionTools.Records;

namespace InceptionTools.FileTypes
{
    /// <summary>
    /// Path-based compatibility wrapper over the verified, lossless save model.
    /// New code should use SaveGameRecord.Parse when it already owns the bytes.
    /// </summary>
    [Obsolete("Use SaveGameRecord.Parse or SaveGameInspector. This wrapper remains for legacy callers.")]
    class SaveFile : AssetFile
    {
        public SaveFile(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            FileLocation = Path.GetFullPath(filePath);
            Name = Path.GetFileNameWithoutExtension(FileLocation);
            Extension = Path.GetExtension(FileLocation);
            if (!string.IsNullOrEmpty(Extension))
                throw new ArgumentException("Save file extensions must be blank.", nameof(filePath));

            byte[] contents = File.ReadAllBytes(FileLocation);
            Size = contents.Length;
            Record = SaveGameRecord.Parse(contents, Path.GetFileName(FileLocation));
        }

        public override string Name { get; }
        public override string FileLocation { get; }
        public override string Extension { get; }
        public override int Size { get; }
        public SaveGameRecord Record { get; }
    }
}
