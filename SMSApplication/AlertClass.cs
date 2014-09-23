using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SMSApplication
{
    class AlertClass
    {
        private Thread AlertService;
        private Thread TimeService;
        private Mutex mutexSerial;

        public AlertClass(){
           mutexSerial = new Mutex();
           TimeService = new Thread(setTimeThread);
           TimeService.Start();           
        }
        //报警音
        private void setAlert()
        {
            System.IO.Ports.SerialPort serial = new System.IO.Ports.SerialPort();
            serial.BaudRate = 9600;
            serial.DataBits = 8;
            serial.Parity = System.IO.Ports.Parity.None;
            serial.PortName = "COM1";
            serial.StopBits = System.IO.Ports.StopBits.One;
            mutexSerial.WaitOne();
            if (!serial.IsOpen)
            {
                serial.Open();
            }
            serial.DiscardInBuffer();
            Console.WriteLine("Alert!");
            serial.Write("A");
            System.Threading.Thread.Sleep(200);
            serial.Close();
            mutexSerial.ReleaseMutex();
            System.Threading.Thread.Sleep(11000);
        }

        //设置时间
        private void setTime()
        {
            System.IO.Ports.SerialPort serial = new System.IO.Ports.SerialPort();
            serial.BaudRate = 9600;
            serial.DataBits = 8;
            serial.Parity = System.IO.Ports.Parity.None;
            serial.PortName = "COM1";
            serial.StopBits = System.IO.Ports.StopBits.One;
            mutexSerial.WaitOne();
            if (!serial.IsOpen)
            {
                serial.Open();
            }
            serial.DiscardInBuffer();
            //Console.WriteLine("Fix Time.");
            serial.Write("S" + DateTime.Now.ToString("HHmmss"));
            System.Threading.Thread.Sleep(200);
            serial.Close();
            mutexSerial.ReleaseMutex();
        }

        public void alert()
        {
            if ((AlertService == null) || (AlertService.ThreadState == ThreadState.Stopped))
            {
                AlertService = new Thread(setAlertThread);
                AlertService.Start();            
            }
        }

        private void setAlertThread()
        { 
            setAlert();
        }

        private void setTimeThread()
        {
            while (true) {
                setTime();
                System.Threading.Thread.Sleep(60000);
            }
        }
    }
}
