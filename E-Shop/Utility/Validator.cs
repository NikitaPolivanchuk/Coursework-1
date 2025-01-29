using System.Text.RegularExpressions;

namespace E_Shop.Utility
{
    internal class Validator
    {
        private class ValueError
        {
            public string message;
            public string value;
            public bool isValid;

            public ValueError()
            {
                message = string.Empty;
                value = string.Empty;
                isValid = true;
            }
        }

        private Dictionary<string, ValueError> _data;

        public string BasePattern;

        public int minLength;
        public int maxLength;

        public bool IsValid { get; private set; }

        public Validator(Dictionary<string, string> data)
        {
            _data = new Dictionary<string, ValueError>();

            foreach (var pair in data)
            {
                ValueError valueError = new ValueError();
                valueError.value = pair.Value;

                _data[pair.Key] = valueError;
            }
            BasePattern = @"^[0-9A-Za-z_: ]*$";

            minLength = 3;
            maxLength = 50;

            IsValid = true;
        }

        public bool ValidateEmail(string field, bool required = true)
        {
            return Validate(field, null, 320, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", required);
        }

        public bool ValidateDouble(string field, double? min, double? max, bool required = true)
        {
            bool valid = (required) ? _Required(field) : true;

            if (valid && (valid = double.TryParse(_data[field].value, out double value)))
            {
                if (min != null && max != null
                    && (value < min || value > max))
                {
                    _data[field].message = $"Value length must be in range between {min} and {max}.";
                    _data[field].isValid = false;
                }
                else if (min != null && value < min)
                {
                    _data[field].message = $"Value must be greater than {min}.";
                    _data[field].isValid = false;
                }
                else if (max != null && value > max)
                {
                    _data[field].message = $"Value must be lower than {max}.";
                    _data[field].isValid = false;
                }
                valid = _data[field].isValid;
            }

            if (!valid)
            {
                IsValid = false;
            }
            return valid;
        }

        public bool Validate(string field, bool required = true)
        {
            return Validate(field, minLength, maxLength, BasePattern, required);
        }

        public bool Validate(string field, int? min, int? max, bool required = true)
        {
            return Validate(field, min, max, BasePattern, required);
        }

        public bool Validate(string field, int? min, int? max, string pattern, bool required = true)
        {
            bool valid = (required) ? _Required(field) : true;

            valid = valid
                    && _CheckLength(field, min, max)
                    && _MatchPattern(field, pattern);

            if (!valid)
            {
                IsValid = false;
            }
            return valid;
        }

        public void AddInfoField(string filed)
        {
            _data[filed] = new ValueError();
        }

        private bool _Required(string field)
        {
            if (string.IsNullOrEmpty(_data[field].value))
            {
                _data[field].message = "This field is required.";
                _data[field].isValid = false;
            }
            return _data[field].isValid;
        }

        private bool _CheckLength(string field , int? min, int? max)
        {
            if (_data[field].isValid)
            {
                string value = _data[field].value;

                if (min != null && max != null 
                    && (value.Length < min || value.Length > max))
                {
                    _data[field].message = $"Value length must be between {min} and {max} chars.";
                    _data[field].isValid = false;
                }
                else if (min != null && value.Length < min)
                {
                    _data[field].message = $"Value length must be greater than {min} chars.";
                    _data[field].isValid = false;
                }
                else if (max != null && value.Length > max)
                {
                    _data[field].message = $"Value length must be lower than {max} chars.";
                    _data[field].isValid = false;
                }
            }
            return _data[field].isValid;
        }

        private bool _MatchPattern(string field, string pattern)
        {
            if (_data[field].isValid
                && !Regex.Match(_data[field].value, pattern).Success)
            {
                _data[field].message = "Value is not valid.";
                _data[field].isValid = false;
            }
            return _data[field].isValid;
        }

        public void SetCustomError(string field, string error)
        {
            if (_data[field].isValid)
            {
                _data[field].message = error;
                _data[field].isValid = false;
                IsValid = false;
            }
        }

        public string[] ToValueError()
        {
            List<string> list = new List<string>();

            foreach (var pair in _data.Values)
            {
                list.Add(pair.value);
                list.Add($"<span class=\"text-danger\">{pair.message}</span>");
            }

            return list.ToArray();
        }

        public static string[] Fill(int fieldsNum)
        {
            string[] fields = new string[fieldsNum];

            for (int i = 0; i < fieldsNum; i++)
            {
                fields[i] = "";
            }
            return fields;
        }
    }
}
