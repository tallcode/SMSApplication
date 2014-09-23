using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SMSApplication
{
    class SMS
    {
        public String Number;
        public String Content;
    }

    class SMSClass
    {
        //待发送短信列表
        private List<SMS> SMSList;
        private Thread TraversalMessage;

        public SMSClass() {
            SMSList = new List<SMS>();
        }

        //发送短信
        private void SendMessage(String target, String content)
        {
            System.IO.Ports.SerialPort serial = new System.IO.Ports.SerialPort();
            serial.BaudRate = 9600;
            serial.DataBits = 8;
            serial.Parity = System.IO.Ports.Parity.None;
            serial.PortName = "COM4";
            serial.StopBits = System.IO.Ports.StopBits.One;
            if (!serial.IsOpen)
            {
                serial.Open();
            }
            serial.DiscardInBuffer();

            PDU sms = new PDU();
            sms.setCenter("+8613800571500");
            sms.setTarget(target);
            sms.setMessage(content);

            System.Threading.Thread.Sleep(200);
            serial.Write("AT\r\n");
            System.Threading.Thread.Sleep(200);
            serial.WriteLine("AT+CMGF=0\r\n");
            System.Threading.Thread.Sleep(200);
            serial.WriteLine("AT+CMGS=" + sms.getPureLength().ToString() + "\r\n");
            System.Threading.Thread.Sleep(200);
            serial.WriteLine(sms.getPDUCode() + Encoding.ASCII.GetString(new byte[] { (byte)26 }));
            System.Threading.Thread.Sleep(2000);
            serial.Close();

        }

        //遍历短信列表
        private void TraversalMessageThread()
        {
            while (SMSList.Count > 0)
            {
                Console.WriteLine(SMSList[0].Number + ":" + SMSList[0].Content);
                SendMessage("+86" + SMSList[0].Number, SMSList[0].Content);
                SMSList.RemoveAt(0);
                System.Threading.Thread.Sleep(100);
            }
        }

        public Boolean Add(String number, String content)
        {
            SMS sms = new SMS();
            sms.Number = number;
            sms.Content = content;
            SMSList.Add(sms);

            if ((TraversalMessage == null) || (TraversalMessage.ThreadState == ThreadState.Stopped)) {
                TraversalMessage = new Thread(TraversalMessageThread);
                TraversalMessage.Start();
            }

            return true;
        }
    }
}
