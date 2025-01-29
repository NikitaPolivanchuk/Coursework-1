using System.Net;
using System.Text;

namespace HttpServer
{
    public interface IHttpServer
    {
        HttpStatusCode StatusCode { set; }
        long ContentLength { set; }
        string ContentType { set; }
        Encoding Encoding { set; }

        Task SendFileAsync(string path, params string[] args);
        int Run();
        void Shutdown();
    }
}
