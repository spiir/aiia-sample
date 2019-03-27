using System;
using System.Net;
using System.Net.Http;

namespace MyDataSample.Services
{
    public class MyDataClientException : Exception
    {
        public MyDataClientException(string message) : base(message)
        {
        }

        public MyDataClientException(string url, HttpStatusCode code) : base(FormatMessage(url, code))
        {
        }
        
        public MyDataClientException(string url, HttpMethod method, Exception innerException) : base(FormatMessage(url, method), innerException)
        {
        }

        private static string FormatMessage(string url, HttpStatusCode code)
        {
            return $"Request to {url} Failed with code {code}.";
        }
        
        private static string FormatMessage(string url, HttpMethod method)
        {
            return $"Request {method} - {url} Failed.";
        }
    }
}