using EltraCloudStorage.Services;
using Microsoft.Extensions.Configuration;

#pragma warning disable CS1591

namespace EltraCloud.DataSource
{
    public class Storage
    {
        public Storage(IConfiguration configuration)
        {
            Server = configuration.GetValue<string>("Storage:Server");
            DbName = configuration.GetValue<string>("Storage:Database");
            User = configuration.GetValue<string>("Storage:User");
            Password = configuration.GetValue<string>("Storage:Password");
        }
        
        #region Properties

        public string Server { get; set; }

        public string DbName { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        #endregion

        #region Methods

        public IStorageService CreateStorageService()
        {
            var connectionString = $"Server={Server};Database={DbName};Uid={User};Pwd={Password};";

            var storageService = new StorageService { ConnectionString = connectionString };

            return storageService;
        }

        #endregion
    }
}
