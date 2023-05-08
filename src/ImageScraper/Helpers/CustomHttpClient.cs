using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ImageScraper.Helpers
{
    public class CustomHttpClient : IDisposable
    {
        private HttpClient _httpClient;

        public CustomHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public CustomHttpClient(string proxyIp, int proxyPort)
        {
            var proxy = new WebProxy(proxyIp, proxyPort);
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
                UseProxy = true
            };
            _httpClient = new HttpClient(httpClientHandler);
        }

        public void SetUserAgent(string userAgent)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        public void SetTimeout(TimeSpan timeout)
        {
            _httpClient.Timeout = timeout;
        }

        public void SetDefaultHeaders(Action<HttpRequestHeaders> headersConfig)
        {
            headersConfig(_httpClient.DefaultRequestHeaders);
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await _httpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            return await _httpClient.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
        {
            return await _httpClient.PutAsync(url, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            return await _httpClient.DeleteAsync(url);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}