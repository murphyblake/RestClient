using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MurphyBlake.Net.Http
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string uri, T data)
        {
            return await client.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(data)));
        }

        public static async Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string uri, T data)
        {
            return await client.PutAsync(uri, new StringContent(JsonConvert.SerializeObject(data)));
        }
    }
}
