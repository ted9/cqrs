

namespace System.Web
{
    public static class HttpRequestBaseExtensions
    {
        public static bool IsPost(this HttpRequestBase request)
        {
            if (request == null) {
                throw new ArgumentNullException("request");
            }
            return request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsGet(this HttpRequestBase request)
        {
            if (request == null) {
                throw new ArgumentNullException("request");
            }
            return request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetString(this HttpRequestBase request, string name)
        {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (request.Params[name] == null) {
                return string.Empty;
            }
            return request.Params[name];
        }

        public static string GetFormString(this HttpRequestBase request, string name)
        {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (request.Form[name] == null) {
                return string.Empty;
            }
            return request.Form[name];
        }

        public static string GetQueryString(this HttpRequestBase request, string name)
        {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (request.QueryString[name] == null) {
                return string.Empty;
            }
            return request.QueryString[name];
        }

        public static string GetIP(this HttpRequestBase request)
        {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            string result = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(result)) {
                result = request.ServerVariables["REMOTE_ADDR"];
            }

            if (string.IsNullOrEmpty(result)) {
                result = request.UserHostAddress;
            }

            if (string.IsNullOrEmpty(result)) {
                result = "0.0.0.0";
            }

            return result;
        }
    }
}
