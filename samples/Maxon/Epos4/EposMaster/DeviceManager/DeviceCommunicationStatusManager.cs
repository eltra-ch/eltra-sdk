using System;
using System.Threading;
using EltraCommon.Logger;
using EposMaster.DeviceManager.Device;

namespace EposMaster.DeviceManager
{
    class CommunicationStatusManager
    {
        #region Private fields

        private Mutex _lockingObject;
        private readonly EposDevice _device;
        
        #endregion

        #region Constructors

        public CommunicationStatusManager(EposDevice device)
        {
            _device = device;
            
            if(_device!=null)
            {
                LockingObjectName = $"Global\\VcsDevice{_device.Family}{_device.InterfaceName}{_device.PortName}";
            }
        }

        #endregion

        #region Properties

        private string LockingObjectName { get; }

        #endregion

        #region Methods

        public bool IsConnected()
        {
            bool result = false;

            try
            {
                if (Mutex.TryOpenExisting(LockingObjectName, out _))
                {
                    result = true;
                }
            }            
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - method", e);
            }

            MsgLogger.WriteDebug($"{GetType().Name} - method", $"Is device {_device.Family} connected = {result}");

            return result;
        }

        public bool SetConnected()
        {
            bool result = false;

            try
            {
                if (Mutex.TryOpenExisting(LockingObjectName, out var lockingObject))
                {
                    result = lockingObject.WaitOne(0);
                }
                else
                {
                    _lockingObject = new Mutex(true, LockingObjectName);

                    result = _lockingObject != null;
                }
            }
            catch (AbandonedMutexException e)
            {
                MsgLogger.Exception($"{GetType().Name} - method", e);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - method", e);
            }

            MsgLogger.WriteLine($"Set device {_device.Family} connected = {result}");

            return result;
        }

        public void SetDisconnected()
        {
            bool result = false;

            if (Mutex.TryOpenExisting(LockingObjectName, out var lockingObject))
            {
                try
                {
                    if (lockingObject.WaitOne(0))
                    {
                        lockingObject.ReleaseMutex();
                    }

                    lockingObject.Close();

                    _lockingObject.Dispose();
                }
                catch(AbandonedMutexException e)
                {
                    MsgLogger.Exception($"{GetType().Name} - method", e);
                }
                catch(Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - method", e);
                }

                _lockingObject = null;

                result = true;
            }
            
            MsgLogger.WriteLine($"Set device {_device.Family} disconnected = {result}");
        }

        #endregion
    }
}
