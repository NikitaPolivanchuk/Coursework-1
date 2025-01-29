namespace HttpServer
{
    public class Mime
    {
        private static readonly Dictionary<string, string> mimeTypes = new()
        {
            {".html", "text/html"},
            {".css", "text/css"},
            { ".js", "application/x-javascript" },
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".png", "image/png"},
            {".ico", "image/ico"}
        };

        public static string GetType(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            if (mimeTypes.ContainsKey(extension))
            {
                return mimeTypes[extension];
            }
            return "application/octet-stream";
        }
    }
}
