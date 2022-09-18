using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.ToolSet;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraCommon.Transport;
using EltraXamCommon.Controls;
using Newtonsoft.Json;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms.Internals;

namespace EltraXamCommon.Plugins
{
    [Preserve(AllMembers = true)]
    public class EltraNavigoPluginManager
    {
        #region Private fields

        private List<EltraPluginCacheItem> _pluginCache;
        private IDialogService _dialogService;

        #endregion

        #region Constructors

        public EltraNavigoPluginManager(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        #endregion

        #region Events

        public event EventHandler<List<ToolViewModel>> PluginAdded;

        private void OnPluginAdded(DeviceToolPayload payload, List<ToolViewModel> viewModels)
        {
            PluginAdded?.Invoke(payload, viewModels);
        }

        #endregion

        #region Properties

        public string Url { get; set; }

        public List<EltraPluginCacheItem> PluginCache => _pluginCache ?? (_pluginCache = new List<EltraPluginCacheItem>());

        public string LocalPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        #endregion

        #region Methods

        public async Task<bool> DownloadTool(EltraDevice device)
        {
            bool result = false;

            var deviceToolSet = device?.ToolSet;

            if (deviceToolSet != null)
            {
                foreach (var tool in deviceToolSet.Tools)
                {
                    if (tool.Status == DeviceToolStatus.Enabled)
                    {
                        result = await DownloadTool(tool);

                        if (!result)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private string GetPluginFilePath(string fileName)
        {
            return Path.Combine(LocalPath, fileName);
        }

        private async Task<bool> DownloadTool(string payloadId, string hashCode, DeviceToolPayloadMode mode)
        {
            bool result = false;

            try
            {
                var fileFullPath = GetPluginFilePath(GetPluginFileName(payloadId));

                var transport = new CloudTransporter();

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uniqueId"] = payloadId;
                query["hashCode"] = hashCode;
                query["mode"] = $"{(int)mode}";

                var url = UrlHelper.BuildUrl(Url, "api/description/payload-download", query);

                var json = await transport.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var payload = JsonConvert.DeserializeObject<DeviceToolPayload>(json);

                    if (payload != null)
                    {
                        var base64EncodedBytes = Convert.FromBase64String(payload.Content);

                        File.WriteAllBytes(fileFullPath, base64EncodedBytes);

                        /*if (FileHelper.ChangeFileNameExtension(fileFullPath, "md5", out string md5FullPath))
                        {
                            File.WriteAllText(md5FullPath, hashCode);

                            result = true;
                        }

                        if (FileHelper.ChangeFileNameExtension(fileFullPath, "id", out string idFullPath))
                        {
                            File.WriteAllText(idFullPath, payloadId);

                            result = true;
                        }*/

                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - DownloadTool", e);
            }

            return result;
        }

        private EltraPluginCacheItem FindPluginInCache(DeviceToolPayload payload)
        {
            EltraPluginCacheItem result = null;

            foreach(var pluginCacheItem in PluginCache)
            {
                if (payload.Mode == DeviceToolPayloadMode.Development)
                {
                    if (pluginCacheItem.PayloadId == payload.Id)
                    {
                        result = pluginCacheItem;
                        break;
                    }
                }
                else
                {
                    if (pluginCacheItem.HashCode == payload.HashCode)
                    {
                        result = pluginCacheItem;
                        break;
                    }
                }
            }

            return result;
        }

        private string GetPluginFileName(string payloadId)
        {
            return $"{payloadId}.pld";
        }

        private bool UpdatePluginCache(DeviceToolPayload payload)
        {
            bool result = false;
            var pluginFilePath = GetPluginFilePath(GetPluginFileName(payload.Id));

            if (File.Exists(pluginFilePath))
            {
                if(UpdatePluginCache(pluginFilePath, payload))
                {
                    var cacheItem = FindPluginInCache(payload);

                    if (cacheItem != null && cacheItem.Plugin != null)
                    {
                        var viewModels = cacheItem.Plugin.GetViewModels();

                        OnPluginAdded(payload, viewModels);

                        result = true;
                    }
                }
            }

            return result;
        }

        private bool GetHashCodeFromFile(string filePath, out string hashCode)
        {
            bool result = false;

            hashCode = string.Empty;

            try
            {
                var bytes = File.ReadAllBytes(filePath);

                if (bytes != null && bytes.Length > 0)
                {
                    var encodedBytes = Convert.ToBase64String(bytes);

                    hashCode = CryptHelpers.ToMD5(encodedBytes);

                    result = true;
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name}", e);
            }
            
            return result;
        }

        private EltraPluginCacheItem FindPluginInFileSystem(DeviceToolPayload payload)
        {
            EltraPluginCacheItem result = null;
            var pluginFilePath = GetPluginFilePath(GetPluginFileName(payload.Id));

            try
            {
                if (File.Exists(pluginFilePath))
                {
                    if (payload.Mode == DeviceToolPayloadMode.Development)
                    {
                        result = UpdateCache(payload, pluginFilePath);
                    }
                    else
                    {
                        if(GetHashCodeFromFile(pluginFilePath, out var hashCode) && hashCode == payload.HashCode)
                        {
                            result = UpdateCache(payload, pluginFilePath);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - FindPluginInFileSystem", e);
            }

            return result;
        }

        private EltraPluginCacheItem UpdateCache(DeviceToolPayload payload, string fullPath)
        {
            EltraPluginCacheItem result = null;

            if (UpdatePluginCache(fullPath, payload))
            {
                var cacheItem = FindPluginInCache(payload);

                if (cacheItem != null && cacheItem.Plugin != null)
                {
                    var viewModels = cacheItem.Plugin.GetViewModels();

                    OnPluginAdded(payload, viewModels);

                    result = cacheItem;
                }
            }

            return result;
        }

        private async Task<bool> DownloadTool(DeviceTool deviceTool)
        {
            bool result = false;

            foreach(var payload in deviceTool.PayloadSet)
            {
                var pluginCacheItem = FindPluginInCache(payload);

                if (pluginCacheItem != null)
                {
                    MsgLogger.WriteFlow($"{GetType().Name} - DownloadTool", $"payload file name {payload.FileName} found in cache");
                    result = true;
                }
                else
                {
                    pluginCacheItem = FindPluginInFileSystem(payload);

                    if (pluginCacheItem != null)
                    {
                        MsgLogger.WriteFlow($"{GetType().Name} - DownloadTool", $"payload file name {payload.FileName} found in file system");
                        result = true;
                    }
                    else
                    {
                        if (await DownloadTool(payload.Id, payload.HashCode, payload.Mode))
                        {
                            result = UpdatePluginCache(payload);
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - DownloadTool", $"payload file name {payload.FileName} download failed!");
                        }
                    }
                }
            }

            return result;
        }

        private bool UpdatePluginCache(string assemblyPath, DeviceToolPayload payload)
        {
            bool result = false;

            try
            {
                MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"load assembly - path = {assemblyPath}");

                var theAssembly = Assembly.LoadFrom(assemblyPath);

                MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", "load assembly - get types");

                Type[] types = theAssembly.GetTypes();

                MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"load assembly - get types - count = {types.Length}");

                foreach (Type t in types)
                {
                    try
                    {
                        var type = t.GetInterface("EltraXamCommon.Plugins.IEltraNavigoPlugin");

                        MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"type full name = {t.FullName}");

                        if (type != null && !string.IsNullOrEmpty(t.FullName))
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"create instance $ {t.FullName}");

                            var assemblyInstace = theAssembly.CreateInstance(t.FullName, false);

                            if (assemblyInstace is IEltraNavigoPlugin pluginInterface)
                            {
                                pluginInterface.DialogService = _dialogService;

                                MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"find payload {payload.FileName}");

                                if (FindPluginInCache(payload) == null)
                                {
                                    var pluginCacheItem = new EltraPluginCacheItem()
                                    { FullPath = assemblyPath, HashCode = payload.HashCode, PayloadId = payload.Id, Plugin = pluginInterface };

                                    PluginCache.Add(pluginCacheItem);

                                    MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"add payload {payload.FileName} cache item");
                                }
                                else
                                {
                                    MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"payload {payload.FileName} already added");
                                }

                                result = true;

                                break;
                            }
                            else
                            {
                                MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"error: create instance $ {t.FullName} failed!");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MsgLogger.Exception($"{GetType().Name} - UpdatePluginCache [1]", e);
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UpdatePluginCache [2]", e);
            }

            return result;
        }

        #endregion
    }
}
