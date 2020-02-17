using EltraCloudStorage.Services;
using EltraCloud.DataSource;

#pragma warning disable CS1591

namespace EltraCloud.Services
{
    public class AuthService : IAuthService
    {
        #region Private fields

        private readonly IStorageService _storageService;

        #endregion

        #region Constructors

        public AuthService(Storage storage)
        {
            _storageService = storage.CreateStorageService();
        }

        #endregion

        #region Methods

        public override AuthState Register(string loginName, string userName, string password)
        {
            AuthState result = Exists(loginName);

            if (result == AuthState.NoUser)
            {
                result = _storageService.RegisterUser(loginName, userName, password) ? AuthState.Success : AuthState.Error;
            }
            else
            {
                result = AuthState.UserAlreadyRegistered;
            }
            
            return result;
        }

        public override AuthState SignIn(string user, string password, out string token)
        {
            token = string.Empty;

            var result = Exists(user);

            if (result == AuthState.Success)
            {
                result = _storageService.SignInUser(user, password, out token) ? AuthState.Success : AuthState.Error;
            }

            return result;
        }

        public override AuthState SignOut(string token)
        {
            AuthState result = AuthState.Undefined;

            if (_storageService.SignOutUser(token))
            {
                result = AuthState.Success;
            }

            return result;
        }

        public override AuthState Exists(string user)
        {
            var result = AuthState.NoUser;

            if (_storageService.UserExists(user))
            {
                result = AuthState.Success;
            }
            
            return result;
        }

        public override AuthState IsValid(string user, string password)
        {
            AuthState result = AuthState.NoAuth;

            if (_storageService.IsUserValid(user, password))
            {
                result = AuthState.Success;
            }

            return result;
        }

        #endregion
    }
}
