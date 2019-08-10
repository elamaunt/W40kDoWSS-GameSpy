using System;
using System.Collections.Generic;
using System.Reflection;

namespace Framework
{
    public class ViewModel : Frame
    {
        private static readonly Dictionary<Type, Dictionary<string, Func<object, object>>> _gettersCache = new Dictionary<Type, Dictionary<string, Func<object, object>>>();

        string _category;
        public virtual string Category
        {
            get => _category;
            set
            {
                if (_category == value)
                    return;

                _category = value;
                FirePropertyChanged(nameof(_category));
            }
        }

        public object GetSubFrameByName(string name)
        {
            var type = GetType();
            var dict = _gettersCache.GetOrDefault(type);

            if (dict == null)
            {
                dict = new Dictionary<string, Func<object, object>>();

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                for (int i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    dict[property.Name] = property.GetValue;
                }

                _gettersCache[type] = dict;
            }

            return dict.GetOrDefault(name)?.Invoke(this);
        }
        
        public virtual string GetName()
        {
            return PageHelper.GetViewModelName(GetType());
        }

        public virtual string GetPrefix()
        {
            return "element";
        }

        public virtual string GetViewStyle()
        {
            return "default";
        }
    }
}