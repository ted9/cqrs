using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Cqrs.Infrastructure.Utilities
{
    public static class HttpUtils
    {
        public static IPAddress GetLocalIPV4()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(address => address.AddressFamily == AddressFamily.InterNetwork);
        }

        public static string GetClientIPV4()
        {
            string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrWhiteSpace(ip)) {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (string.IsNullOrWhiteSpace(ip)) {
                ip = HttpContext.Current.Request.UserHostAddress;
            }

            if (string.IsNullOrWhiteSpace(ip)) {
                ip = "0.0.0.0";
            }

            return ip;
        }

        private static string BuildRequestParameterToString(IDictionary<string, string> dicArray, Encoding encoding)
        {
            SortedDictionary<string, string> tempArray = new SortedDictionary<string, string>(dicArray);

            StringBuilder sb = new StringBuilder();
            foreach (var temp in tempArray) {
                sb.AppendFormat("{0}={1}", temp.Key, temp.Value.UrlEncode(encoding)).Append("&");
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        private static TResult WaitResult<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            if (task.Wait(timeout)) {
                return task.Result;
            }
            throw new WebException("the request timeout.", WebExceptionStatus.Timeout);
        }
        
        public static string BuildRequestWithPost(string url, IDictionary<string, string> parameters, int timeout = 0)
        {
            string requestData = BuildRequestParameterToString(parameters, Encoding.UTF8);

            byte[] postBytes = Encoding.UTF8.GetBytes(requestData);


            WebRequest request = HttpWebRequest.Create(url);
            if (timeout > 0)
                request.Timeout = timeout;
            request.Method = "post";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            request.ContentLength = postBytes.Length;

            return Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null)
                .ContinueWith(task => {
                    try {
                        using (var stream = task.Result) {
                            stream.Write(postBytes, 0, postBytes.Length);
                            stream.Close();
                        }
                        return Task.Factory;
                    }
                    catch (Exception) {
                        throw;
                    }                        
                })
                .WaitResult(TimeSpan.FromMinutes(5))
                .FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null)
                .ContinueWith<string>(GetWebResponseResult).WaitResult(TimeSpan.FromMinutes(5));
        }

        private static string GetWebResponseResult(Task<WebResponse> webresponseTask)
        {
            try {
                using (var response = webresponseTask.Result) {
                    StringBuilder responseData = new StringBuilder();
                    using (var stream = response.GetResponseStream()) {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                            string line;
                            while ((line = reader.ReadLine()) != null) {
                                responseData.Append(line);
                            }
                            reader.Close();
                        }
                        stream.Close();
                    }
                    return responseData.ToString();
                }
            }
            catch (Exception) {
                throw;
            }
        }

        public static string BuildRequestWithGet(string url, IDictionary<string, string> parameters, int timeout = 0)
        {
            WebRequest request = HttpWebRequest.Create(string.Concat(url, "?", BuildRequestParameterToString(parameters, Encoding.UTF8)));
            if (timeout > 0)
                request.Timeout = timeout;
            request.Method = "get";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

            return Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null)
                .ContinueWith<string>(GetWebResponseResult).WaitResult(TimeSpan.FromMinutes(5));
        }
    }
}
