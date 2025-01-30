using System.Net;
using System.Net.Mime;
using System.Text;

namespace Webserver.Utility;

internal class FormDataParser
{
    public static async Task<Dictionary<string, string>?> ParseAsync(HttpListenerRequest request)
    {
        if (request.InputStream == Stream.Null || string.IsNullOrEmpty(request.ContentType))
        {
            return null;
        }

        return request.ContentType switch
        {
            MediaTypeNames.Application.FormUrlEncoded => await ParseUrlEncodedParams(request),
            _ => null
        };
    }

    private static async Task<Dictionary<string, string>> ParseUrlEncodedParams(HttpListenerRequest request)
    {
        var body = new StringBuilder();
        var buffer = new byte[4096];
        int bytesRead;

        while ((bytesRead = await request.InputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            body.Append(request.ContentEncoding.GetString(buffer, 0, bytesRead));
        }

        return body.ToString().asQuery();
    }
}