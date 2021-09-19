using System;
using System.Collections.Generic;
using System.IO;

namespace InceptionTools
{
    class STARMAP : MapFile
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public STARMAP(string FilePath, MapFormat MapEncoding, List<byte[]> Tileset): base(FilePath, MapEncoding, Tileset)
        { 
            Contents = File.ReadAllBytes(FileLocation);
            ContentsIndex = 0;

            MapSizeX = 32;
            MapSizeY = 24;           

            log.Debug($"Loading File: {Path.GetFileName(FilePath)}");
            log.Debug($"FileSize {Size}");                      
            log.Debug($"FileSize {ContentsIndex}");
             
            RAWMap = Contents; //246C: 101D, 0x1000               
            MapData = RemapLinearFormat(RAWMap);    

            log.Debug("Map Load Successful");

            if (!Extension.Equals(".MTP"))
            {
                throw new ArgumentException("Image file extensions must be .MTP");
            }
        }   
    }
}