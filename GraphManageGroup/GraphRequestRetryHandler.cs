using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphManageGroup
{
    class GraphRequestRetryHandler : DelegatingHandler
    {
        private static ILog logger = LogManager.GetLogger(typeof(GraphRequestRetryHandler));

        private const int MaxRetries = 100;

        public GraphRequestRetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }

        /// <summary>
        /// 无法处理BatchRequest，因为BatchRequest始终是200， 429与503是细分在每个Request内的
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int retryTime = 0; retryTime < MaxRetries; retryTime++)
            {
                try
                {
                    response = await base.SendAsync(request, new CancellationToken(false));
                    if (!response.IsSuccessStatusCode
                        && (response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode == (HttpStatusCode)429))
                    {
                        logger.Warn($"An error occurred while send request, will retry the request, retry time: {retryTime.ToString()}, request url: {request.RequestUri}, response message: {response}");
                        Thread.Sleep(10 * 1000);
                        continue;

                    }
                    if (!response.IsSuccessStatusCode
                        && (response.StatusCode == HttpStatusCode.RequestTimeout || response.StatusCode == HttpStatusCode.GatewayTimeout))
                    {
                        logger.Warn($"An error occurred while send request, will retry the request, retry time: {retryTime.ToString()}, request url: {request.RequestUri}, response message: {response}");
                        continue;
                    }
                }
                catch (TaskCanceledException e)
                {
                    logger.Error($"A TaskCanceledException occurred, error:{e.ToString()}");
                    continue;
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while send request, exception:{e.ToString()}");
                    throw;
                }
                return response;
            }

            return response;
        }
    }
}
