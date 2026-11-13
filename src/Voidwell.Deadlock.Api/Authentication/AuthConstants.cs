namespace Voidwell.Deadlock.Api.Authentication;

public static class AuthConstants
{
    public static class Roles
    {
        public const string Administrator = "Administrator";
    }

    public static class Claims
    {
        /// <summary>
        /// The user's id. Inbound claim mapping is off, so the token's claim name is used as is.
        /// </summary>
        public const string Subject = "sub";
    }
}
