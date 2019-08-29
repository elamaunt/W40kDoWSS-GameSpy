using ApiDomain;
using Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

using static ThunderHawk.Core.JsonHelper;

namespace ThunderHawk.Core
{
    public class ServerNewsProvider : INewsProvider
    {
        static RequestBuilder Build(string path, UriKind kind = UriKind.Relative) => CoreContext.HttpService.Build(path, kind);

        public Task<NewsItemDTO[]> LoadLastNews(CancellationToken token)
        {
            return Build("lastnews")
                .Get()
                .Send(token)
                .ValidateSuccessStatusCode()
                .Json<NewsItemDTO[]>(Serializer);
        }
    }
}
