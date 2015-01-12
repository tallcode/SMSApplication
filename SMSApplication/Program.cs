using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using NetFwTypeLib;

namespace SMSApplication
{


    class Program
    {
        private static AlertClass AlertService;
        private static SMSClass SMSService;

        public static void NetFwAddPorts(string name, int port, string protocol)
        {
            //创建firewall管理类的实例
            INetFwMgr netFwMgr = (INetFwMgr)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr"));
            INetFwOpenPort objPort = (INetFwOpenPort)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwOpenPort"));
            objPort.Name = name;
            objPort.Port = port;
            if (protocol.ToUpper() == "TCP")
            {
                objPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
            }
            else
            {
                objPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP;
            }
            objPort.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;
            objPort.Enabled = true;
            bool exist = false;
            //加入到防火墙的管理策略
            foreach (INetFwOpenPort mPort in netFwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts)
            {
                //已经存在策略不重复添加
                if (objPort == mPort)
                {
                    exist = true;
                    break;
                }
            }
            if (!exist) netFwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts.Add(objPort);
        }

        public static void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            String responseString = "";
            String pathname = request.RawUrl.Split('?')[0];
            if (pathname == "" || pathname == "/") {
                responseString = "<HTML><BODY><p>It works</p></BODY></HTML>";
            }
            else if (pathname == "/alert.html") {
                AlertService.alert();
            }
            else if (pathname == "/sms.html")
            {
                String number = request.QueryString["number"];
                String content = request.QueryString["content"];
                Regex RegNumber = new Regex(@"^(?:\+86)?(1[358]\d{9})$");
                if (!String.IsNullOrEmpty(number) && !String.IsNullOrEmpty(content)) {
                    Match MatchNumber = RegNumber.Match(number);
                    if (MatchNumber.Success)
                    {
                        number = MatchNumber.Groups[1].Captures[0].ToString();
                        SMSService.Add(number, content);
                        responseString = "<HTML><BODY><p>Success</p></BODY></HTML>";
                    }
                    else
                    {
                        responseString = "<HTML><BODY><p>Fail</p></BODY></HTML>";
                    }                    
                }                
            }

            if (String.IsNullOrEmpty(responseString))
            {
                response.StatusCode = 404;
                response.Close();
            }
            else
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
                response.Close();
            }
        }

        public static void NonblockingListener()
        {
            HttpListener listener = new HttpListener();
            //需要管理员权限
            listener.Prefixes.Add("http://127.0.0.1:9090/");
            listener.Start();
            while (true)
            {
                IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                // Applications can do some work here while waiting for the 
                // request. If no work can be done until you have processed a request,
                // use a wait handle to prevent this thread from terminating
                // while the asynchronous operation completes.
                result.AsyncWaitHandle.WaitOne();
            }
        }

        static void Main(string[] args)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            //添加防火墙例外
            NetFwAddPorts("SMS Service",9090,"TCP");
            Thread httpServer = new Thread(NonblockingListener);
            httpServer.Start();
            Console.WriteLine("HTTP Server Running......");
            AlertService = new AlertClass();
            SMSService = new SMSClass();
        }
    }
}
