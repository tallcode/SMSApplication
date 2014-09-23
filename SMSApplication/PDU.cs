using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSApplication
{
    class PDU
    {
        private String SMST = String.Empty;
        private String SMSC = String.Empty;
        private String SMS = String.Empty;
        private String EncodeType = "08";

        private String PureCode = String.Empty;

        public Boolean setTarget(String Target) {
            SMST = Target;
            return true;
        }

        public Boolean setCenter(String Center)
        {
            SMSC = Center;
            return true;
        }

        public Boolean setMessage(String Message)
        {
            SMS = Message;
            return true;
        }

        //奇数长度补F，奇偶位互换
        private String EncodeNumber(String Number)
        {
            String Result = String.Empty;
            if (String.IsNullOrEmpty(Number))
            {
                return "";
            }
            //补位
            if (( Number.Length % 2) != 0)
            {
                Number = Number + "F";
            }
            //奇偶换位
            for (Int32 i = 0; i < Number.Length; i = i + 2)
            {
                Result = Result + Number.Substring(i + 1, 1) + Number.Substring(i, 1);
            }
            return Result;
        } 

        //获取目标地址编码
        public String getSMSTCode()
        {
            String Number = SMST;
            String Result = String.Empty;
            if (String.IsNullOrEmpty(Number))
            {
                return String.Empty;
            }
            if (Number.Substring(0, 1) == "+")
            {
                Number = Number.Substring(1, Number.Length - 1);
                Result = "91";

            }
            else
            {
                Result = "81";
            }
            Result = Number.Length.ToString("X2") + Result;
            Result += EncodeNumber(Number);
            return Result;
        }
        //获取短线中心地址编码
        public String getSMSCCode()
        {
            String Number = SMSC;
            String Result = String.Empty;
            if (String.IsNullOrEmpty(Number))
            {
                return String.Empty;
            }
            if (Number.Substring(0, 1) == "+")
            {
                Result = "91";
                Number = Number.Substring(1, Number.Length - 1);
            }
            else
            {
                Result = "81";
            }
            Result += EncodeNumber(Number);
            Result = (Result.Length / 2).ToString("X2") + Result;
            return Result;
        }

        private String Encode7Bit(String Text)
        {
            String Result = String.Empty;
            String ByteStr = String.Empty;
            Byte[] bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(Text);
            Int32 i;

            for (i = 0; i < bytes.Length; i++)
            {
                ByteStr = Convert.ToString(bytes[i], 2).PadLeft(7, '0') + ByteStr;
            }
            for (i = ByteStr.Length; i > 0; i -= 8)
            {
                if (i > 8)
                {
                    Result += Convert.ToInt32(ByteStr.Substring(i - 8, 8), 2).ToString("X2");
                }
                else
                {
                    Result += Convert.ToInt32(ByteStr.Substring(0, i), 2).ToString("X2");
                }
            }
            return Result;
        }

        private String EncodeUCS2(String Text) {
            String Result = String.Empty;
            Byte[] bytes = System.Text.UnicodeEncoding.Unicode.GetBytes(Text);
            for (Int32 i = 0; i < bytes.Length; i = i + 2)
            {
                Result = Result + bytes[i + 1].ToString("X2") + bytes[i].ToString("X2");
            }
            return Result;
        }

        public String getSMSCode()
        {
            String Result;
            //System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[\u0000-\u00EF]+$");
            //if (reg.IsMatch(SMS))
            //{
            //    EncodeType = "00";
            //    Result = Encode7Bit(SMS);
            //    Result = SMS.Length.ToString("X2") + Result;
            //}
            //else
            //{
                EncodeType = "08";
                Result = EncodeUCS2(SMS);
                Result = (Result.Length / 2).ToString("X2") + Result;
            //}
            return Result;
        }

        public String getPDUCode(){
            return getSMSCCode() + PureCode;
        }

        public Int32 getPureLength(){
            if (String.IsNullOrEmpty(SMST) || String.IsNullOrEmpty(SMS))
            {
                return 0;
            }
            PureCode = "1100" + getSMSTCode() + "00" + EncodeType + "00" + getSMSCode();
            return PureCode.Length / 2;
        }
    }
}
