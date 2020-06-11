using System;
using System.Net;
using System.Net.Http;

namespace ViiaSample.Services
{
    public class ViiaClientException : Exception
    {
        public HttpMethod Method { get; }
        public HttpStatusCode StatusCode { get; }

        public ViiaClientException(string url, HttpStatusCode statusCode) : base(FormatMessage(url, statusCode))
        {
            StatusCode = statusCode;
        }

        public ViiaClientException(string url, HttpStatusCode statusCode, string response) : base(FormatMessage(url, statusCode, response))
        {
            StatusCode = statusCode;
        }

        public ViiaClientException(string url, HttpMethod method, HttpResponseMessage message, Exception innerException) :
            base(FormatMessage(url, method, message), innerException)
        {
            Method = method;
        }

        public ViiaClientException(string url, HttpMethod method, string response, Exception innerException) : base(
                                                                                                                    FormatMessage(url, method, response))
        {
            Method = method;
        }

        private static string FormatMessage(string url, HttpStatusCode code)
        {
            return $"Request to {url} Failed with code {code}.";
        }

        private static string FormatMessage(string url, HttpStatusCode code, string response)
        {
            return $"Request to {url} Failed with code {code}. Response Body:\n{response}";
        }

        private static string FormatMessage(string url, HttpMethod method, string response)
        {
            return $"Request {method} - {url} Failed. Response Body:\n{response}\n. End";
        }

        private static string FormatMessage(string url, HttpMethod method, HttpResponseMessage response)
        {
            if (response == null)
            {
                return $"Request {method} - {url} Failed.";
            }

            try
            {
                var contentTask = response.Content.ReadAsStringAsync();
                contentTask.Wait();
                var content = contentTask.Result;

                return $"Request {method} - {url} Failed.\nResponse:\n{content}";
            }
            catch
            {
                return $"Request {method} - {url} Failed.";
            }
        }
    }
}
