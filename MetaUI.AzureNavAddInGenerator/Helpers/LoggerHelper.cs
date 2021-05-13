using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;

namespace MetaUI.AzureNavAddInGenerator.Helpers
{
    public static class LoggerHelper
    {
        public static HttpResponseMessage LogErrorAndReturnHttpResponse(this ILogger logger, string msg, Exception ex)
        {
            logger.LogError(msg + ":");
            logger.LogError(ex.Message);
            return HttpResponseMessageFromExceptiion(ex);
        }

        public static HttpResponseMessage HttpResponseMessageFromExceptiion(Exception ex)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(ex.Message)
            };
        }
    }
}
