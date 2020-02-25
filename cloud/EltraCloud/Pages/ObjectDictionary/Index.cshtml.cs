using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using EltraCloud.Pages.ObjectDictionary.Models;
using EltraCloud.Services;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

#pragma warning disable CS1591

namespace EltraCloud.Pages.ObjectDictionary
{
    public class ObjectDictionaryModel : PageModel
    {
        private readonly ISessionService _sessionService;
        private readonly object _lock = new Object();

        private List<Session> _sessions;
        private List<ParameterModel> _parameters;
        private List<SelectListItem> _sessionSelection;
        
        [BindProperty]
        public List<SelectListItem> SessionSelection => _sessionSelection ?? (_sessionSelection = new List<SelectListItem>());

        [BindProperty]
        [Display(Name = "Active sessions")]
        public int SelectedSessionId { get; set; }
        
        [BindProperty]
        public List<ParameterModel> Parameters
        {
            get => _parameters ?? (_parameters = new List<ParameterModel>());
        }
        
        public ObjectDictionaryModel(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        
        private void BuildSessionSelection()
        {
            int count = 0;

            SelectedSessionId = 1;

            foreach (var session in _sessions)
            {
                var item = new SelectListItem {Value = $"{count + 1}", Text = session.Uuid};

                SessionSelection?.Add(item);

                count++;
            }
        }

        public IActionResult OnPostAsync(ObjectDictionaryModel _)
        {
            if (!ModelState.IsValid)    
            {
                return Page();
            }
            
            return RedirectToPage();
        }

        private EltraDevice GetActiveDevice()
        {
            EltraDevice result = null;
            var sessionUuid = Request.Cookies["session"];

            if (_sessions != null && _sessions.Count > 0)
            {
                if (!string.IsNullOrEmpty(sessionUuid))
                {
                    foreach (var s in _sessions)
                    {
                        var device = GetActiveDevice(s);

                        if (s.Uuid == sessionUuid && device != null)
                        {
                            result = device;
                            break;
                        }
                    }

                    if(result == null)
                    {
                        result = GetFirstDevice();
                    }
                }
                else
                {
                    result = GetFirstDevice();
                }
            }

            return result;
        }

        private EltraDevice GetFirstDevice()
        {
            EltraDevice result = null;

            foreach (var s in _sessions)
            {
                var device = GetActiveDevice(s);

                if (device != null)
                {
                    result = device;
                    break;
                }
            }

            return result;
        }

        private EltraDevice GetActiveDevice(Session session)
        {
            EltraDevice result = null;
            var serialNumberText = Request.Cookies["serialNumber"];

            var devices  =_sessionService.GetSessionDevices(session.Uuid);

            if (devices.Count > 0)
            {
                if (!string.IsNullOrEmpty(serialNumberText))
                {
                    foreach (var device in devices)
                    {
                        if (ulong.TryParse(serialNumberText, out var serialNumber) && device.Identification.SerialNumber == serialNumber)
                        {
                            result = device;
                            break;
                        }
                    }
                }
                else
                {
                    result = devices.FirstOrDefault();
                }
            }

            return result;
        }
        
        public void OnGet()
        {
            _sessions = _sessionService.GetSessions();

            BuildSessionSelection();

            var device = GetActiveDevice();
            
            if (device != null)
            {
                RegisterEvents(device);

                BuildParameterSelection(device);
            }
        }

        private void RegisterEvents(EltraDevice device)
        {
            var objectDictionary = device?.ObjectDictionary;

            if(objectDictionary != null)
                foreach (var parameter in objectDictionary.Parameters)
                {
                    if (parameter is XddParameter epos4Parameter)
                    {
                        epos4Parameter.ParameterChanged += OnEpos4ParameterChanged;
                    }

                    foreach (var subParameter in parameter.Parameters)
                    {
                        if (subParameter is XddParameter epos4SubParameter)
                        {
                            epos4SubParameter.ParameterChanged += OnEpos4ParameterChanged;
                        }
                    }
                }
        }

        private ParameterModel FindParameterModel(string parameterUniqueId)
        {
            ParameterModel result = null;

            lock (_lock)
            {
                foreach (var parameterModel in Parameters)
                {
                    if (parameterModel.UniqueId == parameterUniqueId)
                    {
                        result = parameterModel;
                        break;
                    }
                }
            }

            return result;
        }

        private ParameterModel FindParameterModel(Parameter parameter)
        {
            var result = FindParameterModel(parameter.UniqueId);

            return result;
        }

        private void OnEpos4ParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            var parameterModel = FindParameterModel(e.Parameter);

            parameterModel?.UpdateValue(e.Parameter);
        }

        private void BuildParameterSelection(EltraDevice device)
        {
            if (device?.ObjectDictionary != null)
            {
                var objectDictionary = device.ObjectDictionary;
                var parameters = objectDictionary.Parameters;

                foreach (var parameter in parameters)
                {
                    var parameterModel = new ParameterModel(parameter);

                    parameterModel.Build();

                    if (parameterModel.IsVisible)
                    {
                        lock (_lock)
                        {
                            Parameters.Add(parameterModel);
                        }

                        var subParameters = new List<ParameterModel>();

                        foreach (var subParameter in parameter.Parameters)
                        {
                            var subParameterModel = new ParameterModel(subParameter);

                            subParameterModel.Build();

                            if (subParameterModel.IsVisible)
                            {
                                subParameters.Add(subParameterModel);
                            }
                        }

                        if (subParameters.Count > 1)
                        {
                            lock (_lock)
                            {
                                Parameters.AddRange(subParameters);
                            }
                        }
                    }
                }
            }
        }
    }
}