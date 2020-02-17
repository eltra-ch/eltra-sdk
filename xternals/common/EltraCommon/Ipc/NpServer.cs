using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace EltraCommon.Ipc
{
    public class NpServer
    {
        #region Private fields

        Task _task;

        #endregion

        public NpServer()
        {
            Name = "NpServer";
            ExclusiveMode = true;
        }

        #region Properties

        public string Name { get; set; }

        public bool ExclusiveMode { get; set; }

        #endregion

        #region Events

        public event EventHandler StepRequested;

        protected virtual void OnStopRequested(EventArgs e)
        {
            EventHandler handler = StepRequested;

            handler?.Invoke(this, e);
        }

        #endregion

        #region Methods

        private bool IsServerRunning
        {
            get
            {
                bool result = false;
                var npc = new NpClient() { Name = Name, Timeout = 1000 };

                if (npc.Echo() && ExclusiveMode)
                {
                    result = true;
                }

                return result;
            }            
        }

        public bool Start()
        {
            bool result = false;

            Stop();

            if(!IsServerRunning)
            {
                _task = Task.Run(() => { Execute(); });

                result = true;
            }
            else
            {
                Console.WriteLine("another server already running!");
            }

            return result;
        }

        public bool Stop()
        {
            bool result = false;

            if(_task != null && !_task.IsCompleted)
            {
                var npc = new NpClient() { Name = Name };

                result = npc.Stop();
                
                _task.Wait();
            }

            return result;
        }

        private void Execute()
        {
            using (var ps = new NamedPipeServerStream(Name, PipeDirection.In))
            {
                ps.WaitForConnection();

                try
                {
                    using (StreamReader reader = new StreamReader(ps))
                    {
                        string temp;

                        while ((temp = reader.ReadLine()) != null)
                        {
                            temp = temp.ToLower();

                            if(temp == "stop")
                            {
                                OnStopRequested(new EventArgs());
                                
                                break;
                            }
                            else if (temp == "echo")
                            {
                                //echo
                            }
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("ERROR: {0}", e.Message);
                }
            }            
        }

        #endregion
    }
}
