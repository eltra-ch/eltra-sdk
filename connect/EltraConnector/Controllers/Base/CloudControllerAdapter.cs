using EltraCommon.Transport;
using System;
using System.Net.Sockets;
using EltraConnector.Controllers.Base.Events;
using System.Collections.Generic;
using EltraCommon.Transport.Events;

namespace EltraConnector.Controllers.Base
{
    internal class CloudControllerAdapter
    {
        #region Private fields

        private bool _good;
        private List<CloudControllerAdapter> _children;

        #endregion

        #region Constructors

        public CloudControllerAdapter(string url)
        {
            Url = url;

            _good = true;
            _children = new List<CloudControllerAdapter>();

            Transporter = new CloudTransporter();

            RegisterEvents();
        }

        #endregion

        #region Properties

        public string Url { get; }

        protected CloudTransporter Transporter { get; }

        public bool Good 
        {
            get => _good;
            set
            {
                if(_good != value)
                {
                    _good = value;

                    OnGoodChanged();
                }                
            }
        }

        #endregion

        #region Events

        public event EventHandler<GoodChangedEventArgs> GoodChanged;

        #endregion

        #region Events handling

        private void OnSocketErrorChanged(object sender, SocketErrorChangedEventAgs e)
        {
            Good = e.SocketError == SocketError.Success;
        }

        private void OnGoodChanged()
        {
            GoodChanged?.Invoke(this, new GoodChangedEventArgs() { Good = Good });
        }

        private void OnChildGoodChanged(object sender, GoodChangedEventArgs e)
        {
            if (Good)
            {
                foreach (var child in _children)
                {
                    if(!child.Good)
                    {
                        Good = false;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            Transporter.SocketErrorChanged += OnSocketErrorChanged;
        }

        public void AddChild(CloudControllerAdapter child)
        {
            if (child != null)
            {
                child.GoodChanged += OnChildGoodChanged;

                _children.Add(child);
            }
        }

        public virtual bool Start()
        {
            return true;
        }

        public virtual bool Stop()
        {
            return true;
        }

        #endregion
    }
}
