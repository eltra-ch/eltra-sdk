using System.Runtime.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
using EltraCommon.Logger;

namespace EltraXamCommon.Plugins
{
    [DataContract]
    internal class PluginStoreItem
    {
        private List<string> _files;

        [DataMember]
        public string PluginId { get; set; }

        [DataMember]
        public List<string> Files => _files ?? (_files = new List<string>());

        internal void Purge()
        {
            foreach(var file in Files)
            {
                if(File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch(Exception e)
                    {
                        MsgLogger.Exception($"{GetType().Name} - Purge", e);
                    }
                }
            }

            Files.Clear();
        }

        public void AddFile(string fileName)
        {
            Files.Add(fileName);
        }
    } 
}
