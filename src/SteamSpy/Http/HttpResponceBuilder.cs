using System;
using System.IO;
using System.Text;

namespace GSMasterServer.Http
{
    internal class HttpResponceBuilder
    {
        public static HttpResponse FileUTF8(string path)
        {
            try
            {
                var resp = new HttpResponse()
                {
                    ReasonPhrase = "Ok",
                    StatusCode = "200",
                    ContentAsUTF8 = System.IO.File.ReadAllText(path)
                };
                
                return resp;
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        public static HttpResponse File(string path, Encoding encoding = null)
        {
            try
            {
                byte[] bytes;

                if (encoding == null)
                   bytes = System.IO.File.ReadAllBytes(path);
                else
                    bytes = Encoding.Convert(Encoding.UTF8, encoding, Encoding.UTF8.GetBytes(System.IO.File.ReadAllText(path)));

                var resp = new HttpResponse()
                {
                    ReasonPhrase = "Ok",
                    StatusCode = "200",
                    Content = bytes
                };

                resp.Headers.Add("Content-Type", QuickMimeTypeMapper.GetMimeType(Path.GetExtension(path)));
                //resp.Headers.Add("Content-Length", bytes.Length.ToString());

                return resp;

            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        public static HttpResponse InternalServerError()
        {
            string content = System.IO.File.ReadAllText("Resources/Pages/500.html");

            return new HttpResponse()
            {
                ReasonPhrase = "InternalServerError",
                StatusCode = "500",
                ContentAsUTF8 = content
            };
        }
        
        public static HttpResponse Success()
        {
            return new HttpResponse()
            {
                ReasonPhrase = "Ok",
                StatusCode = "200"
            };
        }

        public static HttpResponse NotFound()
        {
            string content = System.IO.File.ReadAllText("Resources/Pages/404.html");

            return new HttpResponse()
            {
                ReasonPhrase = "NotFound",
                StatusCode = "404",
                ContentAsUTF8 = content
            };
        }

        public static HttpResponse Text(string text)
        {
            return new HttpResponse()
            {
                ReasonPhrase = "Ok",
                StatusCode = "200",
                ContentAsUTF8 = text
            };
        }
    }
}
