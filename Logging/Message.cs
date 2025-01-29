using System.Text;

namespace Logging
{
    public class Message
    {
        public string Title { get; set; }
        private List<KeyValuePair<string, string>> headers;

        public Message(string title)
        {
            Title = title;
            headers = new List<KeyValuePair<string, string>>();
        }

        public Message With(string message)
        {
            return With(null, message);
        }
        public Message With(object value)
        {
            return With(null, value);
        }

        public Message With(Exception exception)
        {
            With("Exception type", exception.GetType())
                .With("Exception message", exception.ToString());

            if (exception.InnerException != null)
            {
                With("Inner exception type", exception.InnerException.GetType())
                    .With("Inner exception message", exception.InnerException.Message);
            }

            return this;
        }

        public Message With(string? header, object? value)
        {
            headers.Add(new KeyValuePair<string, string>(
                header ?? string.Empty,
                (value != null)
                    ? value.ToString() ?? string.Empty
                    : string.Empty));

            return this;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            
            builder.Append(Title);
            builder.Append(Environment.NewLine);

            foreach (KeyValuePair<string, string> pair in headers)
            {
                builder.Append(pair.Key);
                builder.Append(": ");
                builder.Append(pair.Value);
                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
        }
    }
}
