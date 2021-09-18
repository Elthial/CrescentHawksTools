using System;
using System.IO;
using System.Linq;

namespace InceptionTools
{
    class MAP
    {
        public MAP(string FilePath)
        {            
            Path = FilePath;
            Name = System.IO.Path.GetFileNameWithoutExtension(FilePath);
            Extension = System.IO.Path.GetExtension(FilePath);
            Size = (int)new FileInfo(FilePath).Length;
            Contents = File.ReadAllBytes(Path);
            ContentsIndex = 0;

            Console.WriteLine($"Loading File: {System.IO.Path.GetFileName(FilePath)}");
            Console.WriteLine($"FileSize {Size}");
               
            Variable1 = ReadContents(1).First(); //3092:5F9C, 0x01
            Variable2 = ReadContents(1).First(); //3092:5F98, 0x01
            Variable3 = ReadContents(1).First(); //3092:5F94, 0x01
            MapSizeX = ReadContents(1).First(); //3092:5F9E, 0x01
            MapSizeY = ReadContents(1).First(); //3092:5F96, 0x01
            Variable6 = ReadContents(0x80);   // 246C: A461, 0x80
            Variable7 = ReadContents(0x100); //246C: A561, 0x100
            Variable8 = ReadContents(0x20);    // 3092:4564, 0x20
            Variable9 = ReadContents(0x20);    //3092:4596, 0x20
            Variable10 = ReadContents(0x20);   // 3092:39B4, 0x20
            Variable11 = ReadContents(0x20);   //3092:39D4, 0x20
            Variable12 = ReadContents(0x10);   //0x3092:4602, 0x10
            Variable13 = ReadContents(0x08);   //3092:3768, 0x08
            MapData = ReadContents(0x1000); //246C: 101D, 0x1000

            int i = 0;
            for(int y = 0; y < 64; y++) //MapSizeY?
            {
                for(int x = 0; x< 64; x++) //MapSizeX?
                {
                    Console.Write($"{MapData[i]:x} ");
                    i++;
                }
                Console.WriteLine("");
            }

            var distinct = MapData.Distinct().OrderByDescending(num => num).ToList();

            Console.WriteLine("FileLoad Successful");

            if (!Extension.Equals(".MTP"))
            {
                throw new ArgumentException("Image file extensions must be .MTP");
            }
        }

        private byte[] ReadContents(int BytesToRead)
        {
            var Output = new byte[BytesToRead];

            for(int i = 0; i < BytesToRead; i++)
            {
                Output[i] = Contents[ContentsIndex];
                ContentsIndex++;
            }

            return Output;
        }

        public string Name { get; }
        public string Path { get; }
        public string Extension { get; }

        public int Size { get; }
        private byte[] Contents;
        private int ContentsIndex;

        public byte Variable1 { get; }
        public byte Variable2 { get; }
        public byte Variable3 { get; }
        public int MapSizeX { get; }
        public int MapSizeY { get; }
        public byte[] Variable6 { get; }
        public byte[] Variable7 { get; }
        public byte[] Variable8 { get; }
        public byte[] Variable9 { get; }
        public byte[] Variable10 { get; }
        public byte[] Variable11 { get; }
        public byte[] Variable12 { get; }
        public byte[] Variable13 { get; }
        public byte[] MapData { get; }     
    }
}