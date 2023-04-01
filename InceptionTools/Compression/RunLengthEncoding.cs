using System;

namespace InceptionTools
{
    class RunLengthEncoding
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public byte[] Decompress_Format01(byte[] CompressedFile, int IndexStart)
        {
            //If Index Byte is non-Zero then continue with new Byte each increment until end of index count
            //If Index Byte is Zero then run length until end of index count

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
            //If Index Byte is non-Zero then continue with new Byte each increment until end of index count
            //If Index Byte is Zero then run length until end of index count
            //Same as format01 but extracts as 160 Byte objects. First byte is extracted per object.
            //Two objects per x line, 200 y lines then second byte is extracted per object.
            //This is see in the -31999 and 200 Offsets

            short MaxBufferRemaining = 0x7D00;
            byte[] DecodedBuffer = new byte[MaxBufferRemaining];

            short DecodedBuffer_Index = 0;
            ushort CompressedFile_Index = (ushort)IndexStart;

            log.Info($"CompressedFile Length: {CompressedFile.Length}");
            log.Info($"Max Buffer: {MaxBufferRemaining}");

            const short YAxisSize = 200;
            const short XAxisObjectNextByteOffset = 31999;
            short YAxisRemaining = YAxisSize;
            short XAxisObjectByteSize = 160;
            

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
                DecodedBuffer_Index += XAxisObjectByteSize;
                --YAxisRemaining;

                if (YAxisRemaining == 0)
                {
                    YAxisRemaining = YAxisSize;
                    log.Debug($"Buffer_Index: {DecodedBuffer_Index}");
                    DecodedBuffer_Index -= XAxisObjectNextByteOffset; //Move back to beginning but second byte
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

        public byte[] Decompress_Animation(byte[] CompressedFile, int IndexStart)
        {
            ushort MaxBufferRemaining = 0x0F20;
            byte[] DecodedBuffer = new byte[MaxBufferRemaining];

            ushort DecodedBuffer_Index = 0;
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
            CompressedFile_Index++;
            log.Debug($"Current Index: {CompressedFile_Index}");
            //Next byte will go to buffer
            byte OutputByte = CompressedFile[CompressedFile_Index];

            do
            {
                log.Debug($"OutputByte: {OutputByte.ToString("X")}");
                DecodedBuffer[DecodedBuffer_Index] ^= OutputByte;     
                DecodedBuffer_Index++;
                MaxBufferRemaining--;

                if (MaxBufferRemaining == 0)
                    return DecodedBuffer;
                if (!IsZeroByte)
                {
                    RunLength--;
                    if (RunLength != 0)
                        goto GetNextByte;

                    CompressedFile_Index++;
                    log.Debug($"Current Index: {CompressedFile_Index}");
                    log.Debug($"GetByteToDecompress");
                    goto GetByteToDecompress;
                }
                RunLength--;
            } while (RunLength != 0);
            CompressedFile_Index++;
            log.Debug($"Current Index: {CompressedFile_Index}");
            log.Debug($"GetByteToDecompress");
            goto GetByteToDecompress;
        }
    }
}