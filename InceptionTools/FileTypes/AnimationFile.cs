using InceptionTools.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InceptionTools.FileTypes
{
    internal class AnimationFile : AssetFile
    {
        public AnimationFile(string FilePath, Palette FilePalette)
        {
            //File Metadata
            FileLocation = FilePath;
            Name = Path.GetFileNameWithoutExtension(FilePath);
            Extension = Path.GetExtension(FilePath);            
            CompressedContents = File.ReadAllBytes(FileLocation);
            Width = 88;
            Size = BitConverter.ToInt16(CompressedContents, 0);
            StartPos = 0x33;
            Palette = FilePalette;
        }

        public override string Name { get; }
        public override string FileLocation { get; }
        public override string Extension { get; }
        public override int Size { get; }
        public int Width { get; }
        public int StartPos { get; }
        public byte[] CompressedContents { get; }
        public byte[] DecompressedContents { get; set; }
        public Palette Palette { get; }
    }
}
