using System.Net;

namespace Webserver.Content
{
    public class Error : IActionResult
    {
        public static string PathToPages { get; set; } = "root/error_pages";

        private static Dictionary<HttpStatusCode, string> paths = new Dictionary<HttpStatusCode, string>()
        {
            {HttpStatusCode.NotFound, $"{PathToPages}/not_found.html" },
            {HttpStatusCode.BadRequest, $"{PathToPages}/bad_request.html" },
            {HttpStatusCode.Unauthorized, $"{PathToPages}/unauthorized.html" },
            {HttpStatusCode.Forbidden, $"{PathToPages}/forbidden.html" }
        };

        internal HttpStatusCode statusCode {  get; set; }

        internal string Path {  get; set; }

        internal Error(HttpStatusCode code)
        {
            if (paths.TryGetValue(code, out string? path))
            {
                statusCode = code;
                Path = path;
            }
            else
            {
                statusCode = HttpStatusCode.NotFound;
                Path = paths[HttpStatusCode.NotFound];
            }
        }

        public async Task ExecuteResultAsync(WebServer server)
        {
            server.StatusCode = statusCode;
            await server.SendFileAsync(Path);
        }
    }
}
