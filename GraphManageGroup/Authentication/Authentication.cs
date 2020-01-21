using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphManageGroup
{
    class Authentication
    {
        private static readonly Uri authorityUri = new Uri("https://login.microsoftonline.com/common");
        private static readonly string clientId = "0c217f5b-4f17-49b8-a20a-a4acc319f136";

        public static AuthenticationResult GetAccessToken(Prompt promptType)
        {
            var application = PublicClientApplicationBuilder.Create(clientId)
           .WithAuthority(authorityUri)
           .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
           .Build();
            var builder = application.AcquireTokenInteractive(new List<string>() { "https://graph.microsoft.com/.default" });
           return builder.WithPrompt(promptType).ExecuteAsync().Result;
           //return builder.WithAuthority(authorityUri.ToString()).WithExtraQueryParameters("site_id=501358&amp;display=popup").WithPrompt(promptType).ExecuteAsync().Result;
        }

    }
}
