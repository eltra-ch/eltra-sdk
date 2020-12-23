using EltraCommon.Contracts.Users;
using System;
using System.Collections.Generic;
using System.Net;

namespace EltraConnector.Transport.Udp
{
    class EndpointStore
    {
        private Dictionary<string, EndpointStoreEntry> _store;

        public EndpointStore()
        {
            _store = new Dictionary<string, EndpointStoreEntry>();
        }

        public List<IPEndPoint> ActiveEndpoints
        {
            get
            {
                var result = new List<IPEndPoint>();

                foreach (var entry in _store)
                {
                    if (!Exists(result, entry.Value.Endpoint))
                    {
                        result.Add(entry.Value.Endpoint);
                    }
                }

                return result;
            }
        }

        private bool Exists(List<IPEndPoint> endpointList, IPEndPoint endpoint)
        {
            bool result = false;

            foreach(var item in endpointList)
            {
                if(item.ToString() == endpoint.ToString())
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        internal IPEndPoint Lookup(UserIdentity identity)
        {
            IPEndPoint result = null;

            if (_store.ContainsKey(identity.Login))
            {
                result = _store[identity.Login].Endpoint;
            }

            return result;
        }

        internal void Add(UserIdentity identity, IPEndPoint endpoint)
        {
            var entry = new EndpointStoreEntry() { Identity = identity, Endpoint = endpoint };

            if (_store.ContainsKey(identity.Login))
            {
                _store[identity.Login] = entry;
            }
            else
            {
                _store.Add(identity.Login, entry);
            }
        }
    }
}
