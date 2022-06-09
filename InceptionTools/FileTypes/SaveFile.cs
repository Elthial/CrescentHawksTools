using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InceptionTools.FileTypes
{
    class SaveFile : AssetFile
    {
        public SaveFile(string FilePath)
        {
            FileLocation = FilePath;
            Name = Path.GetFileNameWithoutExtension(FilePath);
            Extension = Path.GetExtension(FilePath);
            Size = (int)new FileInfo(FilePath).Length;
            Contents = File.ReadAllBytes(FileLocation);
            ContentsIndex = 0;

            if (!Extension.Equals("."))
            {
                throw new ArgumentException("Save file extensions must be blank");
            }
        }
        public override string Name { get; }
        public override string FileLocation { get; }
        public override string Extension { get; }
        public override int Size { get; }
        protected byte[] Contents;
        protected int ContentsIndex;

        protected byte[] ReadContents(int BytesToRead)
        {
            var Output = new byte[BytesToRead];

            for (int i = 0; i < BytesToRead; i++)
            {
                Output[i] = Contents[ContentsIndex];
                ContentsIndex++;
            }

            return Output;
        }
    }
}
