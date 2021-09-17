using System;

namespace InceptionTools
{
    class RunLengthEncoding
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public byte[] Decompress_Format01(byte[] CompressedFile, int IndexStart)
        {
            short MaxBufferRemaining = 0x7D00;
            byte[] DecodedBuffer = new byte[MaxBufferRemaining]; 

            short DecodedBuffer_Index = 0;
            ushort CompressedFile_Index = (ushort)IndexStart;

            log.Info($"CompressedFile Length: {CompressedFile.Length}");
            log.Info($"Max Buffer: {MaxBufferRemaining}");

            GetByteToDecompress:

            log.Debug("------------------------------");
            log.Debug($"Current Index: {CompressedFile_Index} of {CompressedFile.Length}");
            log.Debug($"Remaining: {CompressedFile.Length - CompressedFile_Index}");
            log.Debug("------------------------------");
            ushort ByteValue;
            bool IsZeroByte = false;
            sbyte CurrentCompressedByte = (sbyte)CompressedFile[CompressedFile_Index];
            log.Debug($"Byte: {CurrentCompressedByte.ToString("X")}");

            if (CurrentCompressedByte != 0x00)
            {
                ByteValue = (ushort)CurrentCompressedByte;
                if (CurrentCompressedByte >= 0x00)
                    goto SetRepeatValue;
                //if byte negative flip to positive. 
                ByteValue = (ushort)-CurrentCompressedByte; 
                log.Debug($"Negate to: {ByteValue.ToString("X")}");                
            }
            else
            {
                CompressedFile_Index++;
                log.Debug($"Current Index: {CompressedFile_Index}");
                ByteValue = BitConverter.ToUInt16(CompressedFile, CompressedFile_Index);
                CompressedFile_Index++;
                log.Debug($"Current Index: {CompressedFile_Index}");
            }
            //Flag as zero seen
            IsZeroByte = true;
            log.Debug($"Zero Flag");

            SetRepeatValue:
            //Byte value becomes repeat value
            ushort RunLength = ByteValue; 
            log.Debug($"RunLength: {RunLength.ToString("X")}");

            GetNextByte:
            //Increment to next byte
            ++CompressedFile_Index;
            log.Debug($"Current Index: {CompressedFile_Index}");
            //Next byte will go to buffer
            byte OutputByte = CompressedFile[CompressedFile_Index];         

            do
            {
                log.Debug($"OutputByte: {OutputByte.ToString("X")}");
                DecodedBuffer[DecodedBuffer_Index] = OutputByte;
                ++DecodedBuffer_Index;
                --MaxBufferRemaining;

                if (MaxBufferRemaining == 0)
                    return DecodedBuffer;


                if (!IsZeroByte)
                {
                    --RunLength;
                    if (RunLength != 0)
                        goto GetNextByte;

                    ++CompressedFile_Index;
                    log.Debug($"Current Index: {CompressedFile_Index}");
                    log.Debug($"GetByteToDecompress");
                    goto GetByteToDecompress;
                }
                --RunLength;
            } while (RunLength != 0);
            ++CompressedFile_Index;
            log.Debug($"Current Index: {CompressedFile_Index}");
            log.Debug($"GetByteToDecompress");
            goto GetByteToDecompress;
        }

        public byte[] Decompress_Format02(byte[] CompressedFile, int IndexStart)
        {
            short MaxBufferRemaining = 0x7D00;
            byte[] DecodedBuffer = new byte[MaxBufferRemaining];

            short DecodedBuffer_Index = 0;
            ushort CompressedFile_Index = (ushort)IndexStart;

            log.Info($"CompressedFile Length: {CompressedFile.Length}");
            log.Info($"Max Buffer: {MaxBufferRemaining}");

            short MaxRunLengthRemaining = 200;

            GetByteToDecompress:

            log.Debug("------------------------------");
            log.Debug($"Current Index: {CompressedFile_Index} of {CompressedFile.Length}");
            log.Debug($"Remaining: {CompressedFile.Length - CompressedFile_Index}");
            log.Debug("------------------------------");
            ushort ByteValue;
            bool IsZeroByte = false;
            sbyte CurrentCompressedByte = (sbyte)CompressedFile[CompressedFile_Index];
            log.Debug($"Byte: {CurrentCompressedByte.ToString("X")}");

            if (CurrentCompressedByte != 0x00)
            {
                ByteValue = (ushort)CurrentCompressedByte;
                if (CurrentCompressedByte >= 0x00)
                    goto SetRepeatValue;

                ByteValue = (ushort)-CurrentCompressedByte;
                log.Debug($"Negate to: {ByteValue.ToString("X")}");
            }
            else
            {
                CompressedFile_Index++;
                //log.Debug($"Current Index: {CompressedFile_Index}");
                ByteValue = BitConverter.ToUInt16(CompressedFile, CompressedFile_Index);
                CompressedFile_Index++;
                //log.Debug($"Current Index: {CompressedFile_Index}");
            }
            IsZeroByte = true;
            log.Debug($"Zero Flag");

            SetRepeatValue:
            ushort RunLength = ByteValue;
            log.Debug($"RunLength: {RunLength.ToString("X")}");

            GetNextByte:
            ++CompressedFile_Index;
            //log.Debug($"Current Index: {CompressedFile_Index}");
            byte OutputByte = CompressedFile[CompressedFile_Index];

            do
            {
                log.Debug($"OutputByte: {OutputByte.ToString("X")}");
                DecodedBuffer[DecodedBuffer_Index] = OutputByte;
                DecodedBuffer_Index += 160;
                --MaxRunLengthRemaining;

                if (MaxRunLengthRemaining == 0)
                {
                    MaxRunLengthRemaining = 200;
                    log.Debug($"Buffer_Index: {DecodedBuffer_Index}");
                    DecodedBuffer_Index -= 31999; //How to this map outside of pointers sub di,7CFFh
                    log.Debug($"After RunLength Reset Buffer_Index: {DecodedBuffer_Index}");
                }
                --MaxBufferRemaining;

                if (MaxBufferRemaining == 0)
                    return DecodedBuffer;

                if (!IsZeroByte)
                {
                    --RunLength;
                    if (RunLength != 0x00)
                        goto GetNextByte;

                    ++CompressedFile_Index;
                    //log.Debug($"Current Index: {CompressedFile_Index}");
                    log.Debug($"GetByteToDecompress");
                    goto GetByteToDecompress;
                }
                --RunLength;

            } while (RunLength != 0x00);

            ++CompressedFile_Index;
            //log.Debug($"Current Index: {CompressedFile_Index}");
            log.Debug($"GetByteToDecompress");
            goto GetByteToDecompress;
        }
    }
}