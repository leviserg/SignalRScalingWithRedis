using Microsoft.AspNetCore.SignalR;

namespace Server.Providers
{
    public class CustomUserProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var httpContext = connection.GetHttpContext();
            string userName = String.Empty;
            if (httpContext!= null)
            {
                if (httpContext.Request.Query.ContainsKey("username"))
                {
                    userName += httpContext.Request.Query["username"].First();
                }
                else if (httpContext.Request.Headers.ContainsKey("username"))
                {
                    userName += httpContext.Request.Headers["username"];
                }
                else
                {
                    userName = "Anonymous";
                }
                return userName;
            }
            return "Anonymous";
        }
    }
}
