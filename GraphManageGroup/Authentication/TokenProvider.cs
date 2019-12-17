using Microsoft.Identity.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphManageGroup
{
    class TokenProvider
    {
        private ConcurrentDictionary<string, AuthenticationResult> tokenCache;

        public TokenProvider()
        {
            tokenCache = new ConcurrentDictionary<string, AuthenticationResult>();
        }

        public AuthenticationResult GetAccessToken(string resource)
        {
            lock (tokenCache)
            {
                if (tokenCache.ContainsKey(resource) && !IsExpire(tokenCache[resource]))
                {
                    return tokenCache[resource];
                }
                if (tokenCache.Count == 0)
                {
                    tokenCache[resource] = Authentication.GetAccessToken(Prompt.SelectAccount);
                }
                else
                {
                    tokenCache[resource] = Authentication.GetAccessToken(Prompt.Never);
                }
            }
            return tokenCache[resource];
        }

        private bool IsExpire(AuthenticationResult token)
        {
            if ((token.ExpiresOn.UtcDateTime - DateTime.UtcNow).Minutes <= 5)
            {
                return true;
            }
            return false;
        }
    }
}
