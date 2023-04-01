using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Graphics
{
    internal class EGA_Animations
    {
        //void VGA_Inline_ASM_Loop(int PtrSource, int PtrDestination, int BytesToRead)
        //{
        //    ushort SourceOffset = (word16)PtrSource;
        //    ushort SourceSeg = SLICE(PtrSource, word16, 16);
        //    ushort DestinationOffset = (word16)PtrDestination;
        //    ushort seg2446 = SLICE(PtrDestination, word16, 16);
        //    // Segment [246C]
        //    // Switch DS to [246C]
        //    ushort DestinationIndex = DestinationOffset;

        //    for(int TimesToLoop = BytesToRead / 2; TimesToLoop != 0x00; TimesToLoop--)
        //    {
        //        EGA_Animation_Bitshift();
        //        EGA_Animation_Bitshift();
        //        EGA_Animation_Bitshift();
        //        EGA_Animation_Bitshift();
        //        EGA_Animation_Bitshift();
        //        EGA_Animation_Bitshift();
        //        EGA_Animation_Bitshift();
        //        EGA_Animation_Bitshift();
        //        seg2446->*DestinationIndex = SourceSeg;
        //        DestinationIndex++;
        //        seg2446->* DestinationIndex = SourceOffset;
        //        DestinationIndex++;          
        //    } 
        //}

        static void EGA_Animation_Bitshift(ref byte al, ref byte bl, ref byte bh, ref byte dl, ref byte dh)
        {
            byte carry;

            // Shift AL left by 1 bit
            carry = (byte)(al >> 7);
            al <<= 1;

            // Rotate DH left by 1 bit, with carry from previous operation
            dh <<= 1;
            dh |= carry;

            // Shift AL left by 1 bit
            carry = (byte)(al >> 7);
            al <<= 1;

            // Rotate DL left by 1 bit, with carry from previous operation
            dl <<= 1;
            dl |= carry;

            // Shift AL left by 1 bit
            carry = (byte)(al >> 7);
            al <<= 1;

            // Rotate BH left by 1 bit, with carry from previous operation
            bh <<= 1;
            bh |= carry;

            // Shift AL left by 1 bit
            carry = (byte)(al >> 7);
            al <<= 1;

            // Rotate BL left by 1 bit, with carry from previous operation
            bl <<= 1;
            bl |= carry;
        }

        // 207F:1E37: void DrawCall_EGA_Animations()
        // Called from:
        //      Draw_Animation_Scene
        private static byte[] DrawCall_EGA_Animations(byte[] InputArray)
        {
            //A000 memory consists of 64 KB of memory
            byte[] OutputArray = new byte[InputArray.Length];

            //ushort DestinationIndex_143 = 0x0141; //321 - first x of second y row?
            //ushort SourceOffset = 0x336B; //This is the pointer to the AnimationFinalMemory

            int outputIndex = 0;
            int InputIndex = 0;

            for (int bx_36 = 0x58; bx_36 != 0x00; bx_36--)
            {

                for (int cx_38 = 0x0B; cx_38 != 0x00; cx_38--) //Per pixel of the source
                {
                    // Apply bit plane 0 mask to the current byte
                    if ((InputArray[InputIndex++] & 0x01) != 0)
                        OutputArray[outputIndex] |= 0x01;

                    // Apply bit plane 1 mask to the current byte
                    if ((InputArray[InputIndex++] & 0x02) != 0)
                        OutputArray[outputIndex] |= 0x02;

                    // Apply bit plane 2 mask to the current byte
                    if ((InputArray[InputIndex++] & 0x04) != 0)
                        OutputArray[outputIndex] |= 0x04;

                    // Apply bit plane 3 mask to the current byte
                    if ((InputArray[InputIndex++] & 0x08) != 0)
                        OutputArray[outputIndex] |= 0x08;

                    outputIndex++;
                }
                outputIndex += 0x1D;
            }

            return OutputArray;
        }

    }
}
