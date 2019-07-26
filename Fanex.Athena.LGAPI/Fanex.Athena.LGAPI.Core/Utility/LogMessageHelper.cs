using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Fanex.Athena.LGAPI.Core.Utility
{
    public static class LogMessageHelper
    {
        public static string GenerateMessage(Exception exception)
        {
            var messageBuilder = new StringBuilder();

            messageBuilder.AppendLine($"ErrMsg: {exception.Message}");

            AddRequestInfo(messageBuilder);

            AddExceptionInfo(messageBuilder, exception);

            return messageBuilder.ToString();
        }

        private static void AddRequestInfo(StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine("REQUEST INFO");

            var webInfo = GetRequestInfo(HttpContext.Current?.Request);

            foreach (var item in webInfo)
            {
                builder.AppendLine($"{item.Key}: {item.Value}");
            }
        }


        private static void AddExceptionInfo(StringBuilder builder, Exception exception)
        {
            builder.AppendLine();
            builder.AppendLine("EXCEPTION INFO");

            builder.AppendLine($"Source: {exception.Source}");
            builder.AppendLine($"TargetSite: {exception.TargetSite}");
            builder.AppendLine($"StackTrace: {exception.StackTrace}");
        }

        private static IDictionary<string, object> GetRequestInfo(
            HttpRequest request)
            => request == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>
                {
                    [nameof(request.Url)] = request.Url,
                    [nameof(request.UrlReferrer)] = request.UrlReferrer,
                    [nameof(request.Form)] = request.Form,
                    [nameof(request.UserHostAddress)] = request.UserHostAddress,
                    [nameof(request.UserAgent)] = GetUserAgent(request.ServerVariables),
                    ["Client IP"] = GetClientIP(request.ServerVariables),
                };

        private static string GetClientIP(NameValueCollection serverVariables)
        {
            var clientIP = serverVariables["HTTP_X_FORWARDED_FOR"]
                ?? serverVariables["REMOTE_ADDR"]
                ?? string.Empty;

            return string.IsNullOrEmpty(clientIP) || clientIP.Equals("::1")
                ? "127.0.0.1"
                : clientIP;
        }

        private static string GetUserAgent(NameValueCollection serverVariables)
            => serverVariables["HTTP_USER_AGENT"];
    }
}