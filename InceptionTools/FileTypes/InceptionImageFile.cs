using InceptionTools.Graphics;
using System;
using System.IO;

namespace InceptionTools
{
    class InceptionImageFile : AssetFile
    {
        public InceptionImageFile(string FilePath, ImagePurpose FilePurpose, Palette FilePalette) 
        {            
            FileLocation = FilePath;
            Name = Path.GetFileNameWithoutExtension(FilePath);
            Extension = Path.GetExtension(FilePath);
            Purpose = FilePurpose;
            Width = Extension.Equals(".CMP") ? 320 : 16;
            CompressedContents = File.ReadAllBytes(FileLocation);            
            Size = BitConverter.ToInt16(CompressedContents, 0);
            CompressionFormat = CompressedContents[2];
            StartPos = 3;
            Palette = FilePalette;

            if (!(Extension.Equals(".CMP") || Extension.Equals(".ICN")))
            {
                throw new ArgumentException("Image file extensions must be .CMP or .ICN");
            }
        }

        public override string Name { get; }
        public override string FileLocation { get; }
        public override string Extension { get; }
        public ImagePurpose Purpose { get; }
        public int Width { get; }        
        public override int Size { get; }
        public int StartPos { get; }
        public int CompressionFormat { get; }
        public byte[] CompressedContents { get; }
        public byte[] DecompressedContents { get; set; }
        public Palette Palette { get; }     
    }
}