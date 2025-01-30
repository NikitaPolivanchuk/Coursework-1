using HttpServer;

namespace Webserver.Content
{
    public class View : IActionResult
    {
        internal static string AbsolutePath { get; set; } = "";
        public static string[] LayoutArgs { get; set; } = [];

        private readonly string title;
        private readonly string path;
        internal string[] Args { get; set; }

        internal View(string title, string path, params object[] args)
        {
            this.title = title;
            this.path = $"{AbsolutePath}Views/{path}";
            Args = args.Select(arg => arg.ToString()).ToArray()!;
        }

        public async Task ExecuteResultAsync(WebServer server)
        {
            await server.SendStringAsync(Format(), Mime.GetType(path));
        }

        private string Format()
        {
            string layout = File.ReadAllText($"{AbsolutePath}Views/_layout.html");
            string content = File.ReadAllText(path!);

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
