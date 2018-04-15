using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MurphyBlake.Net.Http
{
    public class RestClient
    {
        private HttpClient _httpClient;

        public string ClientName { get; set; }
        public string BaseAddress { get; set; }
        public HttpClient Client => _httpClient ?? (_httpClient = new HttpClient());

        public Func<HttpRequestHeaders, HttpRequestHeaders> SetHeaders;

        public RestClient()
        {
            //
        }

        public RestClient(HttpClient Client) : this()
        {
            _httpClient = Client ?? throw new NullReferenceException("HttpClient cannot be set to null");
        }

        public RestClient(HttpClient Client, string BaseAddress) : this(Client)
        {
            this.BaseAddress = BaseAddress ?? throw new NullReferenceException("BaseAddress cannot be set to null");
        }

        public RestClient(HttpClient Client, string BaseAddress, string ClientName) : this(Client, BaseAddress)
        {
            this.ClientName = ClientName ?? throw new NullReferenceException("ClientName cannot be set to null");
        }

        public Task<TResult> GetAsync<TResult>(string uri)
        {
            return Execute<TResult>(
               async (client) => { return await client.GetAsync(uri); },
               async (response) => { return await response.Content.ReadAsAsync<TResult>(); });
        }

        public Task PostAsJsonAsync<T>(T data, string uri)
        {
            return Execute<string>(
               async (client) => { return await client.PostAsJsonAsync(uri, data); });
        }

        public Task<TResult> PostAsJsonAsync<TResult, T>(T data, string uri)
        {
            return Execute<TResult>(
               async (client) => { return await client.PostAsJsonAsync<T>(uri, data); },
               async (response) => { return await response.Content.ReadAsAsync<TResult>(); });
        }

        public Task PutAsJsonAsync<T>(T data, string uri)
        {
            return Execute<string>(
               async (client) => { return await client.PutAsJsonAsync(uri, data); });
        }

        public Task<TResult> PutAsJsonAsync<TResult, T>(T data, string uri)
        {
            return Execute<TResult>(
               async (client) =>
               {
                   return await client.PutAsJsonAsync<T>(uri, data);
               },
               async (response) =>
               {
                   return await response.Content.ReadAsAsync<TResult>();
               });
        }

        private async Task<TResult> Execute<TResult>(
            Func<HttpClient, Task<HttpResponseMessage>> operation,
            Func<HttpResponseMessage, Task<TResult>> actionOnResponse = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            Configure(Client);

            using (HttpResponseMessage response = await operation(_httpClient).ConfigureAwait(true))
            {

                if (!response.IsSuccessStatusCode)
                {
                    var exception = new Exception($"Resource returned an exception. StatusCode : {response.StatusCode}");
                    exception.Data.Add("StatusCode", response.StatusCode);
                    throw exception;
                }

                if (actionOnResponse != null)
                {
                    return await actionOnResponse(response).ConfigureAwait(false);
                }
                else
                {
                    return default(TResult);
                }
            }
        }

        private void Configure(HttpClient client)
        {
            if (string.IsNullOrEmpty(BaseAddress))
            {
                throw new Exception("BaseAddress is required!");
            }

            if (!string.IsNullOrEmpty(ClientName))
            {
                client.DefaultRequestHeaders.Add("CN", ClientName);
            }

            client.BaseAddress = new Uri(BaseAddress);
            client.Timeout = new TimeSpan(0, 0, 0, 10);

            SetHeaders?.Invoke(client.DefaultRequestHeaders);
        }
    }
}
