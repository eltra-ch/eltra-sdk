using System.Runtime.Serialization;
using System.Collections.Generic;
using System;
using EltraCommon.Logger;

namespace EltraXamCommon.Plugins
{
    [DataContract]
    internal class PluginStoreItem
    {
        private List<PluginStoreItemFile> _files;

        [DataMember]
        public string PluginId { get; set; }

        [DataMember]
        public List<PluginStoreItemFile> Files => _files ?? (_files = new List<PluginStoreItemFile>());

        internal void Purge()
        {
            foreach(var file in Files)
            {
                file.Purge();                
            }

            Files.Clear();
        }

        public void AddFile(string fileName)
        {
            var item = new PluginStoreItemFile(true) { FileName = fileName };

            Files.Add(item);
        }

        internal bool Validate(out int failed)
        {
            var toRemove = new List<PluginStoreItemFile>();
            bool result = false;

            failed = 0;
            
            try
            {
                foreach (var file in Files)
                {
                    if (!file.Validate())
                    {
                        file.Purge();

                        if (file.Purged)
                        {
                            toRemove.Add(file);
                            failed++;
                        }
                    }
                }

                foreach (var ri in toRemove)
                {
                    Files.Remove(ri);
                }

                result = true;
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Validate", e);
            }

            return result;
        }
    } 
}
