using System;
using System.IO;
using System.IO.Pipes;

namespace EltraCommon.Ipc
{
    public class NpClient
    {
        public NpClient()
        {
            Name = "NpServer";
            Timeout = 30000;
        }

        public string Name { get; set; }

        public int Timeout { get; set; }
        
        public bool Echo()
        {
            return SendMessage("echo");
        }

        public bool Stop()
        {
            return SendMessage("Stop");
        }

        private bool SendMessage(string msg)
        {
           bool result = false;

            try
            {
                using (var pc = new NamedPipeClientStream(".", Name, PipeDirection.Out))
                {
                    pc.Connect(Timeout);

                    using (var writer = new StreamWriter(pc))
                    {
                        writer.AutoFlush = true;

                        writer.WriteLine(msg);

                        result = true;
                    }
                }
            }
            catch (TimeoutException)
            {
                result = false;
            }
            catch (Exception)
            {
                result = false;
            }
           
            return result;
        }
    }
}
