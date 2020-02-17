namespace EltraCloud.Services
{
    #pragma warning disable CS1591

    public abstract class IAuthService
    {
        public abstract AuthState Register(string loginName, string userName, string password);
        public abstract AuthState SignIn(string user, string password, out string token);
        public abstract AuthState SignOut(string token);
        public abstract AuthState Exists(string user);
        public abstract AuthState IsValid(string user, string password);
    }
}
