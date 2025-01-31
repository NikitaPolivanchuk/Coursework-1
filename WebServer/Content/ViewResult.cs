using System.Text;
using HttpServer;
using Logging;

namespace Webserver.Content
{
    public class ViewResult : IActionResult
    {
        public static string[] LayoutArgs { get; set; } = [];

        private readonly string title;
        internal string[] Args { get; set; }
        internal string RefPath { get; set; }
        public ILogger Logger { get; set; }

        internal ViewResult(string title, string path, params object[] args)
        {
            this.title = title;
            RefPath = path;
            Args = args.Select(arg => arg.ToString()).ToArray()!;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.Response;
            var content = string.Empty;
            await response.OutputStream.FlushAsync();

            try
            {
                content = string.Format(BuildView(context.AbsolutePath), Args);
            }
            catch
            {
            }

            response.ContentLength64 = content.Length;
            response.ContentType = Mime.GetType(RefPath);
            response.ContentEncoding = Encoding.UTF8;

            byte[] buffer = Encoding.UTF8.GetBytes(content);

            context.Response.SendChunked = true;

            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            await context.Response.OutputStream.FlushAsync();
        }

        private string BuildView(string absolutePath)
        {
            string layout = File.ReadAllText($"{absolutePath}Views/_layout.html");
            string content = File.ReadAllText($"{absolutePath}Views/{RefPath}");

            if (Args != null && Args.Length > 0)
            {
                content = string.Format(content, Args);
            }

            List<string> args = [title!];
            foreach (string arg in LayoutArgs)
            {
                args.Add(arg);
            }
            args.Add(content);

            string data = layout;
                
            try
            {
                data = string.Format(layout, args.ToArray());
            }
            catch { }               

            return data;
        }
    }
}
