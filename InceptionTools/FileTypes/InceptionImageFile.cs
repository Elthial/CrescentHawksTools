using InceptionTools.Graphics;
using System;
using System.Drawing;
using System.IO;

namespace InceptionTools
{
    class InceptionImageFile
    {
        public InceptionImageFile(string FilePath, Palette FilePalette)
        {            
            Path = FilePath;
            Name = System.IO.Path.GetFileNameWithoutExtension(FilePath);
            Extension = System.IO.Path.GetExtension(FilePath);
            Width = Extension.Equals(".CMP") ? 320 : 16;
            CompressedContents = File.ReadAllBytes(Path);            
            Size = BitConverter.ToInt16(CompressedContents, 0);
            CompressionFormat = CompressedContents[2];
            StartPos = 3;
            Palette = FilePalette;

            if (!(Extension.Equals(".CMP") || Extension.Equals(".ICN")))
            {
                throw new ArgumentException("Image file extensions must be .CMP or .ICN");
            }
        }

        public string Name { get; }
        public string Path { get; }
        public string Extension { get; }
        public int Width { get; }
        
        public int Size { get; }
        public int StartPos { get; }
        public int CompressionFormat { get; }
        public byte[] CompressedContents { get; }

        public byte[] DecompressedContents { get; set; }
        public Palette Palette { get; }     
    }
}