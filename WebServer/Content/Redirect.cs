namespace Webserver.Content
{
    public class Redirect : IActionResult
    {
        internal string Url { get; private set; }

        internal Redirect(string url)
        {
            Url = url;
        }
    }
}
