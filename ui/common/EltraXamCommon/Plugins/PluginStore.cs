﻿using EltraCommon.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace EltraXamCommon.Plugins
{
    [DataContract]
    internal class PluginStore
    {
        #region Constructors

        public PluginStore()
        {
            Header = DefaultHeader;
        }

        #endregion

        #region Properties

        /// <summary>
        /// DefaultHeader
        /// </summary>
        public static string DefaultHeader = "AXR7";

        /// <summary>
        /// Header
        /// </summary>
        [DataMember]
        public string Header { get; set; }

        private List<PluginStoreItem> _items;

        public string LocalPath { get; set; }

        private string PluginStoreFilePath
        {
            get => Path.Combine(LocalPath, ".pluginStore");
        }

        [DataMember]
        public List<PluginStoreItem> Items
        {
            get => _items ?? (_items = new List<PluginStoreItem>());
            set => _items = value;
        }

        #endregion

        #region Methods

        public void Purge()
        {
            foreach(var item in Items)
            {
                item.Purge();
            }
        }

        public PluginStore Load()
        {
            PluginStore result = null;

            try
            {
                if (File.Exists(PluginStoreFilePath))
                {
                    var json = File.ReadAllText(PluginStoreFilePath);

                    var pluginStore = JsonConvert.DeserializeObject<PluginStore>(json);

                    if(pluginStore != null)
                    {
                        result = pluginStore;

                        Items = pluginStore.Items;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Load", e);
            }

            return result;
        }

        public void Serialize()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this);

                File.WriteAllText(PluginStoreFilePath, json);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Serialize", e);
            }
        }

        public void Add(string pluginId, string fileName)
        {
            var item  = GetItem(pluginId);

            if(item != null)
            {
                item.AddFile(fileName);
            }
            else
            {
                item = new PluginStoreItem() { PluginId = pluginId };

                item.AddFile(fileName);

                Items.Add(item);
            }
        }

        private PluginStoreItem GetItem(string pluginId)
        {
            PluginStoreItem result = null;
            
            foreach (var item in Items)
            {
                if (item.PluginId == pluginId)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }

        internal string GetAssemblyFile(string pluginId)
        {
            string result = string.Empty;
            var item = GetItem(pluginId);

            if(item != null)
            {
                var latestFiles = new List<string>(item.Files);
                
                latestFiles.Reverse();

                foreach(var file in latestFiles)
                {
                    if (File.Exists(file))
                    {
                        result = file;
                        break;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
