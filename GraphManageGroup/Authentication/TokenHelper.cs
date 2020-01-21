using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace GraphManageGroup
{
    class TokenHelper
    {
        private readonly string GraphUri = "https://graph.microsoft.com/";
        private TokenProvider tokenProvider;
        public TokenHelper()
        {
        }

        public GraphServiceClient GetGraphServiceClient()
        {
            var graphServiceClient = new GraphServiceClient(string.Format("{0}/v1.0", GraphUri), new DelegateAuthenticationProvider(a =>
             {
                 a.Headers.Add("Authorization", "Bearer " + TokenProvider.GetAccessToken(GraphUri).AccessToken);
                 return Task.FromResult(0);
             }));
            //Set timeout value
            graphServiceClient.HttpProvider.OverallTimeout = TimeSpan.FromMinutes(5);
            return graphServiceClient;
        }

        private TokenProvider TokenProvider
        {
            get
            {
                if (tokenProvider == null)
                {
                    tokenProvider = new TokenProvider();
                }
                return tokenProvider;
            }
        }

        private AuthenticationResult GetToken(string targetUrl)
        {
            var url = new Uri(targetUrl);
            string scope = string.Format("{0}://{1}/", url.Scheme, url.Host);
            return TokenProvider.GetAccessToken(scope);

        }

    }
}
