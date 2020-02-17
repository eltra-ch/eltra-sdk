using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EltraCloudStorage;
using EltraCloudStorage.DataSource.Connection;

namespace EltraCloudStorage.Items
{
    class StorageItem : INotifyPropertyChanged
    {
        #region Private fields

        private DbConnectionWrapper _connection;
        private List<StorageItem> _children;
        private string _engine;

        #endregion

        #region Constructors

        public StorageItem()
        {
            PropertyChanged += DataStoreItem_PropertyChanged;
        }
        
        #endregion

        #region Properties

        public string Engine
        {
            get => _engine;
            set
            {
                _engine = value;
                OnPropertyChanged("Engine");
            }
        }
        
        public DbConnectionWrapper Connection
        {
            get => _connection;
            set
            {
                _connection = value;
                OnPropertyChanged("Connection");
            }
        }

        public List<StorageItem> Children => _children ?? (_children = new List<StorageItem>());

        #endregion

        #region Events

        private void DataStoreItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Connection")
            {
                foreach (var child in Children)
                {
                    child.Connection = Connection;
                }
            }
            if (e.PropertyName == "Engine")
            {
                foreach (var child in Children)
                {
                    child.Engine = Engine;
                }
            }
        }

        #endregion

        #region Methods

        public void AddChild(StorageItem child)
        {
            Children.Add(child);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
