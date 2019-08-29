using System;
using System.Net.Http;
using Framework;

namespace ThunderHawk.Core
{
    public class ThunderHawkHttpService : HttpService
    {
        protected override void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri(@"http://127.0.0.1/api/");
        }

        protected override void ConfigureHandler(HttpClientHandler handler)
        {
            handler.UseCookies = false;
        }
    }
}
