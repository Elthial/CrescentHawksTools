namespace InceptionTools
{
    abstract class AssetFile
    {       
        public abstract string Name { get; }
        public abstract string FileLocation { get; }
        public abstract string Extension { get; }           
        public abstract int Size { get; }      
    }
}