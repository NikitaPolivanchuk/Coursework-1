using System.Text;
using System.Web;

namespace Webserver.Utility
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, string> ToDictionary(this string queryString)
        {
            Dictionary<string, string> parameters = [];

            foreach (string parameter in queryString.Split('&'))
            {
                string[] pair = parameter.Split('=');

                if (pair.Length != 2)
                {
                    continue;
                }

                string key = HttpUtility.UrlDecode(pair[0]);
                string value = HttpUtility.UrlDecode(pair[1]);

                parameters[key] = parameters.ContainsKey(key) ? $"&{value}" : value;
            }

            return parameters;
        }

        public static string ToUrlEncodedString (this Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string key in dict.Keys)
            {
                sb.Append('&');
                sb.Append(key);
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode(dict[key]));
            }

            return sb.ToString().TrimStart('&');
        }
    }
}
