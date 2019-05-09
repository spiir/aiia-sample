using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
        
        public ViiaClientException(string url, HttpMethod method, HttpResponseMessage message, Exception innerException) : base(FormatMessage(url, method, message), innerException)
        {
        }

        private static string FormatMessage(string url, HttpStatusCode code)
        {
            return $"Request to {url} Failed with code {code}.";
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