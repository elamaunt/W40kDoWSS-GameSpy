using System.Collections.Generic;

namespace SharedServices
{
    public abstract class MessagesServiceBase : ICulturedMessagesService
    {
        readonly Dictionary<string, string> _values;

        public MessagesServiceBase()
        {
            _values = InflateValues();
        }

        protected abstract Dictionary<string, string> InflateValues();

        public string Get(string key)
        {
            if (_values.TryGetValue(key, out string value))
                return value;

            return key;
        }
    }
}
