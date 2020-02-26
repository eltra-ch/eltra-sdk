using System;
using System.Collections.Generic;
using System.Linq;
using EltraCloud.Services;

using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

#pragma warning disable CS1591

namespace EltraCloud.Pages
{
    public class IndexModel : PageModel
    {
        #region Private fields

        private readonly ISessionService _sessionService;
        private List<Session> _sessions;
        private List<EltraDevice> _devices;

        #endregion

        #region Constructors

        public IndexModel(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        #endregion

        #region Properties

        [BindProperty]
        public List<Session> Sessions
        {
            get => _sessions ?? (_sessions = new List<Session>());
            set => _sessions = value;
        }

        [BindProperty]
        public Session SelectedSession { get; set; }

        [BindProperty]
        public List<EltraDevice> Devices
        {
            get => _devices ?? (_devices = new List<EltraDevice>());
            set => _devices = value;
        }

        [BindProperty]
        public string SelectedSessionUuid { get; set; }

        #endregion

        #region Methods

        public void ObdClick(object sender, EventArgs e)
        {
            UpdateSessions();
        }

        public void OnGet()
        {
            UpdateSessions();

            UpdateDevices();
        }

        private void UpdateSessions()
        {
            if (_sessionService != null)
            {
                Sessions = _sessionService.GetSessions(SessionStatus.Online, true);
            }
            
            if (Sessions != null)
            {
                SelectedSession = Sessions.FirstOrDefault();
                SelectedSessionUuid = SelectedSession != null ? SelectedSession.Uuid : string.Empty;
            }
            else
            {
                SelectedSessionUuid = string.Empty;
            }
        }

        private void UpdateDevices()
        {
            Devices.Clear();

            if (_sessionService != null)
            {
                foreach (var session in _sessions)
                {
                    var devices = _sessionService.GetSessionDevices(session.Uuid);

                    if (devices != null && devices.Count > 0)
                    {
                        Devices.AddRange(devices);
                    }
                }
            }
        }

        public void SelectSession(string uuid)
        {
            foreach (var session in _sessions)
            {
                if (session.Uuid == uuid)
                {
                    SelectedSession = session;
                    break;
                }
            }
        }

        #endregion
    }
}