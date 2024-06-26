using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.ToolSet;
using EltraCommon.Contracts.Users;
using EltraCommon.Extensions;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraCommon.Transport;
using EltraUiCommon.Controls;
using EltraUiCommon.Helpers;
using EltraMauiCommon.Plugins.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Maui.Controls.Internals;
using EltraUiCommon.Containers;
using EltraUiCommon.Dialogs;
using EltraMauiCommon.Dialogs;
using EltraUiCommon.Framework;

namespace EltraMauiCommon.Plugins
{
    [Preserve(AllMembers = true)]
    public class EltraNavigoPluginManager
    {
        #region Private fields

        private List<EltraPluginCacheItem> _pluginCache;
        private IDialogService _dialogService;
        private PluginStore _pluginStore;
        private IContainerRegistry _containerRegistry;

        #endregion

        #region Constructors

        public EltraNavigoPluginManager(IDialogService dialogService, IContainerRegistry containerRegistry)
        {
            _dialogService = dialogService;
            _containerRegistry = containerRegistry;
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

        public CloudControllerAdapter CloudAdapter { get; set; }

        public List<EltraPluginCacheItem> PluginCache => _pluginCache ?? (_pluginCache = new List<EltraPluginCacheItem>());

        public string LocalPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private PluginStore PluginStore => _pluginStore ?? (_pluginStore = CreatePluginStore());

        public bool Debugging { get; set; }
        public bool PurgeToolStorage { get; set; }

        #endregion

        #region Methods

        private PluginStore CreatePluginStore()
        {
            var pluginStore = new PluginStore() { LocalPath = LocalPath };

            pluginStore.Load();

            return pluginStore;
        }

        public bool PurgeToolFileSystem(EltraDevice device)
        {
            bool result = false;

            var deviceToolSet = device?.ToolSet;

            if (deviceToolSet != null)
            {
                foreach (var tool in deviceToolSet.Tools)
                {
                    result = PurgeToolFileSystem(tool);

                    if (!result)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public async Task<bool> DownloadTool(UserIdentity identity, EltraDevice device)
        {
            bool result = false;

            var deviceToolSet = device?.ToolSet;

            if(PurgeToolStorage)
            {
                if(!PurgeToolFileSystem(device))
                {
                    MsgLogger.WriteError($"{GetType().Name} - DownloadTool", "purge file system failed!");
                }
            }

            if (deviceToolSet != null)
            {
                foreach (var tool in deviceToolSet.Tools)
                {
                    if (tool.Status == DeviceToolStatus.Enabled)
                    {
                        result = await UpdateTool(identity, tool);

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

        private async Task<bool> DownloadTool(UserIdentity identity, string payloadId, string hashCode, DeviceToolPayloadMode mode)
        {
            bool result = false;

            if(identity == null)
            {
                throw new Exception("Identity not defined!");
            }

            try
            {
                var assemblyPath = GetPluginFilePath(GetPluginFileName(Guid.NewGuid().ToString()));

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uniqueId"] = payloadId;
                query["hashCode"] = hashCode;
                query["mode"] = $"{(int)mode}";

                var url = UrlHelper.BuildUrl(CloudAdapter.Url, "api/description/payload-download", query);

                var json = await CloudAdapter.Get(identity, url);

                if (!string.IsNullOrEmpty(json))
                {
                    var payload = json.TryDeserializeObject<DeviceToolPayload>();

                    if (payload != null)
                    {
                        var base64EncodedBytes = Convert.FromBase64String(payload.Content);

                        File.WriteAllBytes(assemblyPath, base64EncodedBytes);

                        PluginStore.Add(payloadId, assemblyPath);

                        PluginStore.Serialize();

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
            var pluginFilePath = PluginStore.GetAssemblyFile(payload.Id);

            if (File.Exists(pluginFilePath))
            {
                if(UpdatePluginCache(pluginFilePath, payload))
                {
                    var cacheItem = FindPluginInCache(payload);

                    if (cacheItem != null && cacheItem.PluginService != null)
                    {
                        var viewModels = cacheItem.PluginService.GetViewModels();

                        OnPluginAdded(payload, viewModels);

                        result = true;
                    }
                }
            }

            return result;
        }

        private EltraPluginCacheItem FindPluginInFileSystem(DeviceToolPayload payload)
        {
            EltraPluginCacheItem result = null;
            
            try
            {
                if (Debugging)
                {
                    var pluginFilePath = GetPluginFilePath(payload.FileName);
                    var currentPathFileName = Path.GetFileName(pluginFilePath);
                    var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var currentPathFullPath = Path.Combine(binFolder, currentPathFileName);

                    if (File.Exists(currentPathFullPath))
                    {
                        if (payload.Mode == DeviceToolPayloadMode.Development)
                        {
                            result = UpdateCache(payload, currentPathFullPath);
                        }
                    }
                    else if (File.Exists(pluginFilePath))
                    {
                        if (payload.Mode == DeviceToolPayloadMode.Development)
                        {
                            result = UpdateCache(payload, pluginFilePath);
                        }
                    }
                    else
                    {
                        pluginFilePath = PluginStore.GetAssemblyFile(payload.Id);

                        if (File.Exists(pluginFilePath))
                        {
                            result = UpdateCache(payload, pluginFilePath);
                        }
                    }
                }
                else
                {
                    var pluginFilePath = PluginStore.GetAssemblyFile(payload.Id);

                    if (File.Exists(pluginFilePath))
                    {
                        result = UpdateCache(payload, pluginFilePath);
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - FindPluginInFileSystem", e);
            }

            return result;
        }

        private bool PurgeToolFileSystem(DeviceToolPayload payload)
        {
            bool result = false;

            try
            {
                var pluginFilePath = GetPluginFilePath(payload.FileName);

                if (File.Exists(pluginFilePath))
                {
                    File.Delete(pluginFilePath);    
                }

                result = true;
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CleanupToolStorage", e);
            }

            return result;
        }

        private EltraPluginCacheItem UpdateCache(DeviceToolPayload payload, string fullPath)
        {
            EltraPluginCacheItem result = null;

            if (UpdatePluginCache(fullPath, payload))
            {
                var cacheItem = FindPluginInCache(payload);

                if (cacheItem != null && cacheItem.PluginService != null)
                {
                    var viewModels = cacheItem.PluginService.GetViewModels();

                    OnPluginAdded(payload, viewModels);

                    result = cacheItem;
                }
            }

            return result;
        }

        private async Task<bool> UpdateTool(UserIdentity identity, DeviceTool deviceTool)
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
                        if (await DownloadTool(identity, payload.Id, payload.HashCode, payload.Mode))
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

        private bool PurgeToolFileSystem(DeviceTool deviceTool)
        {
            bool result = false;

            foreach (var payload in deviceTool.PayloadSet)
            {
                result = PurgeToolFileSystem(payload);

                if (result)
                {
                    MsgLogger.WriteFlow($"{GetType().Name} - CleanupToolStorage", $"payload file name {payload.FileName} deleted from file system");
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

                var payloadPath = PluginStore.GetAssemblyFile(payload.Id);

                if(!string.IsNullOrEmpty(payloadPath))
                {
                    assemblyPath = payloadPath;
                }

                if(!string.IsNullOrEmpty(assemblyPath))
                {
                    var pluginAssembly = Assembly.LoadFrom(assemblyPath);

                    MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", "load assembly - get types");

                    Type[] types = pluginAssembly.GetTypes();

                    MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"load assembly - get types - count = {types.Length}");

                    foreach (Type t in types)
                    {
                        try
                        {
                            var type = t.GetInterface("EltraMauiCommon.Plugins.IEltraNavigoPluginService");

                            MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"type full name = {t.FullName}");

                            if (type != null && !string.IsNullOrEmpty(t.FullName))
                            {
                                MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"create instance $ {t.FullName}");

                                var assemblyInstace = pluginAssembly.CreateInstance(t.FullName, false);
                                string name = pluginAssembly.GetName().Name;
                                string version = pluginAssembly.GetName().Version.ToString();

                                if (assemblyInstace is IEltraNavigoPluginService pluginService)
                                {
                                    pluginService.DialogRequested += OnPluginInterfaceDialogRequested;

                                    MsgLogger.WriteDebug($"{GetType().Name} - UpdatePluginCache", $"find payload {payload.FileName}");

                                    if (FindPluginInCache(payload) == null)
                                    {
                                        var pluginCacheItem = new EltraPluginCacheItem() { Name = name, FullPath = assemblyPath, 
                                                                                           HashCode = payload.HashCode, PayloadId = payload.Id, 
                                                                                           PluginService = pluginService, Version = version };

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
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            MsgLogger.Exception($"{GetType().Name} - UpdatePluginCache [1]", e);
                        }
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - UpdatePluginCache", "assembly store returns empty element");
                }
            }
            catch (Exception e)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                MsgLogger.Exception($"{GetType().Name} - UpdatePluginCache [2]", e);
            }

            return result;
        }

        private void OnPluginInterfaceDialogRequested(object sender, DialogRequestedEventArgs e)
        {
            if(sender is IEltraNavigoPluginService pluginService)
            {
                var dialogView = pluginService.ResolveDialogView(e.ViewModel);

                if (dialogView != null)
                {
                    var viewModel = e.ViewModel;
                    var dialogViewName = dialogView.GetType().Name;

                    if (!_containerRegistry.IsRegistered(dialogView.GetType()))
                    {
                        var viewType = dialogView.GetType().ToString();
                        var viewModelType = e.ViewModel.GetType();

                        ViewModelLocationProvider.Register(viewType, viewModelType);

                        _containerRegistry.Register(typeof(object), dialogView.GetType(), dialogViewName);
                    }

                    ThreadHelper.RunOnMainThread(() =>
                    {
                        Action<IDialogResult> dialogResult = (r) => {
                            e.DialogResult = r;
                        };

                        _dialogService?.ShowDialog(dialogViewName, e.Parameters, dialogResult);
                    });
                }
            }
        }

        #endregion
    }
}
