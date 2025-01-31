using System.Net;
using HttpServer;
using System.Text;
using Logging;

namespace Webserver.Content
{
    public class ErrorResult : IActionResult
    {
        public ILogger Logger { get; set; }
        public static string PathToPages { get; set; } = "root/error_pages";

        private static Dictionary<HttpStatusCode, string> paths = new Dictionary<HttpStatusCode, string>()
        {
            {HttpStatusCode.NotFound, $"{PathToPages}/not_found.html" },
            {HttpStatusCode.BadRequest, $"{PathToPages}/bad_request.html" },
            {HttpStatusCode.Unauthorized, $"{PathToPages}/unauthorized.html" },
            {HttpStatusCode.Forbidden, $"{PathToPages}/forbidden.html" }
        };

        internal HttpStatusCode statusCode {  get; set; }

        internal string RefPath {  get; set; }

        internal ErrorResult(HttpStatusCode code)
        {
            if (paths.TryGetValue(code, out string? path))
            {
                statusCode = code;
                RefPath = path;
            }
            else
            {
                statusCode = HttpStatusCode.NotFound;
                RefPath = paths[HttpStatusCode.NotFound];
            }
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.Response;
            response.StatusCode = (int)statusCode;
            await response.OutputStream.FlushAsync();

            Stream input = new FileStream(Path.Combine(context.AbsolutePath, RefPath), FileMode.Open);

            response.ContentLength64 = input.Length;
            response.ContentType = Mime.GetType(RefPath);
            response.ContentEncoding = Encoding.UTF8;

            byte[] buffer = new byte[65535];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                await response.OutputStream.WriteAsync(buffer, 0, bytesRead);
                await response.OutputStream.FlushAsync();
            }

            input.Close();
        }
    }
}
