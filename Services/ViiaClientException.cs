using System;
using System.Net;
using System.Net.Http;

namespace ViiaSample.Services
{
    public class ViiaClientException : Exception
    {
        public ViiaClientException(string message) : base(message)
        {
        }

        public ViiaClientException(string url, HttpStatusCode code) : base(FormatMessage(url, code))
        {
        }
        
        public ViiaClientException(string url, HttpMethod method, Exception innerException) : base(FormatMessage(url, method), innerException)
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