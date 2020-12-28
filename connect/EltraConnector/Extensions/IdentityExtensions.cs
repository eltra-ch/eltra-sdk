using EltraCommon.Contracts.Users;
using EltraCommon.Helpers;

namespace EltraConnector.Extensions
{
    internal static class IdentityExtensions
    {
        public static UserIdentity HashPassword(this UserIdentity identity)
        {
            var result = identity.Clone();

            result.Password = CryptHelpers.ToSha256(identity.Password);

            return result;
        }

        public static UserIdentity Clone(this UserIdentity identity)
        {
            var result = new UserIdentity();

            result.Login = identity.Login;
            result.Name = identity.Name;
            result.Password = identity.Password;
            result.Role = identity.Role;

            return result;
        }
    }
}
