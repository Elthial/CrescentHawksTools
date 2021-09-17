using System;
using System.IO;

namespace InceptionTools
{
    class InceptionImageFile
    {
        public InceptionImageFile(string FilePath)
        {            
            Path = FilePath;
            Name = System.IO.Path.GetFileNameWithoutExtension(FilePath);
            Extension = System.IO.Path.GetExtension(FilePath);
            Width = Extension.Equals("CMP") ? 320 : 16;
            Contents = File.ReadAllBytes(Path);            
            Size = BitConverter.ToInt16(Contents, 0);
            Format = Contents[2];
            StartPos = 3;

            if (!(Extension.Equals("CMP") || Extension.Equals("ICN")))
            {
                throw new ArgumentException("Image file extensions must be CMP or ICN");
            }
        }
        public string Name { get; }
        public string Path { get; }
        public string Extension { get; }
        public int Width { get; }
        public int Format { get; }
        public int Size { get; }
        public int StartPos { get; }
        public byte[] Contents { get; }

        //Support Palette swapping
        //Change one colour for another
        //Infocom needs 5 (AA00AA) swapped for 41 (5500FF)
        //BTTitle needs 1 (0000AA) swapped for 0 (000000)
    }
}