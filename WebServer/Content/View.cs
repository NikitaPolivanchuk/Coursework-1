using HttpServer;

namespace Webserver.Content
{
    public class View : IActionResult
    {
        internal static string AbsolutePath { get; set; } = "";
        public static string[] LayoutArgs { get; set; } = [];

        internal string MimeType { get; }
        private string Title { get; set; }
        private string Path { get; set; }
        internal string[] Args { get; set; }

        internal View(string title, string path, params object[] args)
        {
            Title = title;
            Path = $"{AbsolutePath}Views/{path}";
            MimeType = Mime.GetType(path);
            Args = args.Select(arg => arg.ToString()).ToArray()!;
        }

        internal string Format()
        {
            string layout = File.ReadAllText($"{AbsolutePath}Views/_layout.html");
            string content = File.ReadAllText(Path!);

            if (Args != null && Args.Length > 0)
            {
                content = string.Format(content, Args);
            }

            List<string> args = [Title!];
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
