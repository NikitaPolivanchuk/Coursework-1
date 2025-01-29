using System.Text;
using System.Web;

namespace Webserver.Utility
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, string> asQuery(this string str)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (string nameValue in str.Split('&'))
            {
                string[] nameVal = nameValue.Split('=');

                string key = HttpUtility.UrlDecode(nameVal[0]);
                string value = HttpUtility.UrlDecode(nameVal[1]);

                if (dict.ContainsKey(key))
                {
                    dict[key] += $"&{value}";
                }
                else
                {
                    dict[key] = value;
                }
            }
            return dict;
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
