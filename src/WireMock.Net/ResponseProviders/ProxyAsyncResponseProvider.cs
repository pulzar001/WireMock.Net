using System;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.Settings;

namespace WireMock.ResponseProviders
{
    internal class ProxyAsyncResponseProvider : IResponseProvider
    {
        private readonly Func<RequestMessage, IWireMockServerSettings, Task<ResponseMessage>> _responseMessageFunc;
        private readonly Func<RequestMessage, IWireMockServerSettings, HttpClient, Task<ResponseMessage>> _responseMessageFuncWithHttpClient;
        private readonly IWireMockServerSettings _settings;
        private readonly HttpClient _httpClientForProxy;

        public ProxyAsyncResponseProvider(Func<RequestMessage, IWireMockServerSettings, Task<ResponseMessage>> responseMessageFunc, IWireMockServerSettings settings)
        {
            _responseMessageFunc = responseMessageFunc;
            _settings = settings;
        }

        public ProxyAsyncResponseProvider(Func<RequestMessage, IWireMockServerSettings, HttpClient, Task<ResponseMessage>> responseMessageFunc, IWireMockServerSettings settings, HttpClient p_httpClientForProxy)
        {
            _responseMessageFuncWithHttpClient = responseMessageFunc;
            _settings = settings;
            _httpClientForProxy = p_httpClientForProxy;
        }

        public Task<ResponseMessage> ProvideResponseAsync(RequestMessage requestMessage, IWireMockServerSettings settings)
        {
            if (_httpClientForProxy != null)
            {
                return _responseMessageFuncWithHttpClient(requestMessage, _settings, _httpClientForProxy);
            }
            return _responseMessageFunc(requestMessage, _settings);
        }
    }
}