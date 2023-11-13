using System;
using System.Collections.Generic;

namespace picdasm
{
    class TableSwitchMetaInstructionProcessor : MetaInstructionProcessorBase2
    {
        private readonly Context c;
        private readonly Writer w;

        public TableSwitchMetaInstructionProcessor(Context c, Writer w)
        {
            this.c = c;
            this.w = w;
        }

        protected override IEnumerable<bool> QQ(CurrentInstructionHandle h)
        {
            int caseCount;
            int defaultJump;
            byte tbll;
            byte tblh;
            int pcStart = h.PC;

            // MOVLW caseCount
            if (h.InstructionType == PicInstrucitonType.MOVLW)
            {
                caseCount = h.Buf.Literal;
                yield return true;
            }
            else
            {
                yield break;
            }

            // CPFSLT INDF0, ACCESS
            yield return h.InstructionType == PicInstrucitonType.CPFSLT && IsINDF0(h.Buf);

            // GOTO defaultJump
            if (h.InstructionType == PicInstrucitonType.GOTO)
            {
                defaultJump = h.Buf.CallGotoAddr * 2;
                yield return true;
            }
            else
            {
                yield break;
            }

            // MOVF INDF0, W, ACCESS
            yield return h.InstructionType == PicInstrucitonType.MOVF && IsINDF0(h.Buf) && h.Buf.Destination == DestinationMode.W;

            // MULLW 0x2
            yield return h.InstructionType == PicInstrucitonType.MULLW && h.Buf.Literal == 0x02;

            // MOVLW tblh
            if (h.InstructionType == PicInstrucitonType.MOVLW)
            {
                tblh = h.Buf.Literal;
                yield return true;
            }
            else
            {
                yield break;
            }

            // MOVWF TBLPTRH, ACCESS
            yield return h.InstructionType == PicInstrucitonType.MOVWF
                && h.Buf.Access == AccessMode.Access
                && h.Buf.FileReg == 0xF7;

            // MOVLW tbll
            if (h.InstructionType == PicInstrucitonType.MOVLW)
            {
                tbll = h.Buf.Literal;
                yield return true;
            }
            else
            {
                yield break;
            }

            // ADDWF PROD, W, ACCESS
            yield return h.InstructionType == PicInstrucitonType.ADDWF
                && h.Buf.Destination == DestinationMode.W
                && h.Buf.Access == AccessMode.Access
                && h.Buf.FileReg == 0xF3;

            // BTFSC STATUS, 0, ACCESS
            yield return h.InstructionType == PicInstrucitonType.BTFSC
                && h.Buf.Access == AccessMode.Access
                && h.Buf.FileReg == 0xD8
                && h.Buf.BitOpBit == 0;

            // INCF PRODH, F, ACCESS
            yield return h.InstructionType == PicInstrucitonType.INCF
                && h.Buf.Destination == DestinationMode.F
                && h.Buf.Access == AccessMode.Access
                && h.Buf.FileReg == 0xF4;

            // MOVWF TBLPTR, ACCESS
            yield return h.InstructionType == PicInstrucitonType.MOVWF
                && h.Buf.Access == AccessMode.Access
                && h.Buf.FileReg == 0xF6;

            // MOVF PRODH, W, ACCESS
            yield return h.InstructionType == PicInstrucitonType.MOVF
                && h.Buf.Destination == DestinationMode.W
                && h.Buf.Access == AccessMode.Access
                && h.Buf.FileReg == 0xF4;

            // ADDWF TBLPTRH, F, ACCESS
            yield return h.InstructionType == PicInstrucitonType.ADDWF
                && h.Buf.Destination == DestinationMode.F
                && h.Buf.Access == AccessMode.Access
                && h.Buf.FileReg == 0xF7;

            // TBLRD*-
            yield return h.InstructionType == PicInstrucitonType.TBLRD
                && h.Buf.TableMode == TableOpMode.PostDecrement;

            // MOVFF TABLAT, PCLATH
            yield return h.InstructionType == PicInstrucitonType.MOVFF
                && h.Buf.MovffSource == 0xff5
                && h.Buf.MovffDest == 0xffa;

            // TBLRD*
            yield return h.InstructionType == PicInstrucitonType.TBLRD
                && h.Buf.TableMode == TableOpMode.None;

            // MOVF TABLAT, W, ACCESS
            yield return h.InstructionType == PicInstrucitonType.MOVF
                && h.Buf.Destination == DestinationMode.W
                && h.Buf.Access == AccessMode.Access
                && h.Buf.FileReg == 0xF5;

            // MOVWF PCL, ACCESS
            yield return h.InstructionType == PicInstrucitonType.MOVWF
                && h.Buf.Access == AccessMode.Access
                && h.Buf.FileReg == 0xF9;

            int pcEnd = h.PC;

            List<string> lines = new List<string>();

            w.RefGoto(pcStart);

            lines.Add("swtich (INDF0)");
            lines.Add("{");

            for (int i = 0; i < caseCount; i++)
            {
                int tbl = (tblh << 8) | tbll;
                int q = tbl + i * 2 - 1;

                int qq = c.ReadU16(q)*2;

                lines.Add(string.Format("case 0x{0:X2}: goto _0x{1:X5};", i, qq));
                w.RefGoto(qq);
            }

            lines.Add(string.Format("default: goto _0x{0:X5};", defaultJump));
            lines.Add("}");

            //w.Rewrite(pcStart, pcEnd, lines.ToArray());
        }

        private static bool IsINDF0(PicInstructionBuf buf)
        {
            return buf.Access == AccessMode.Access && buf.FileReg == 0xEF;
        }
    }
}
