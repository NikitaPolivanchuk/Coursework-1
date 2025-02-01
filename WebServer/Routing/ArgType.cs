namespace Webserver.Routing
{
    internal class ArgType
    {
        private static List<ArgType> types = new List<ArgType>()
        {
            new ArgType(typeof(string), s => s),
            new ArgType(typeof(Guid), s => Guid.Parse(s)),
            new ArgType(typeof(double), s => double.Parse(s), "double"),
            new ArgType(typeof(int), s => int.Parse(s), "int")
        };

        public Type Type { get; private set; }
        public Func<string, object> Parser { get; set; }
        public HashSet<string> AltNames { get; set; }

        private ArgType(Type type, Func<string, object> parser, params string[] altNames)
        {
            Type = type;
            Parser = parser;

            AltNames = new HashSet<string>(altNames);
            AltNames.Add(Type.Name.ToLower());
        }

        public static bool TryParse(string strValue, out object? value, out Type? type)
        {
            for (int i = types.Count - 1; i >= 0; i--)
            {
                ArgType argType = types[i];
                try
                {
                    value = argType.Parser(strValue);
                    type = argType.Type;
                    return true;
                }
                catch
                {
                    continue;
                }
            }
            value = null;
            type = null;
            return false;
        }


        public static ArgType GetArgType(string stringType)
        {
            return types.
                Where(type => type.AltNames.Contains(stringType.ToLower()))
                .FirstOrDefault()!;
        }
    }
}
