using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework
{
    public static class PageHelper
    {
        static volatile bool s_isTypesExtracted;
        static List<Type> s_pageViewModelTypes;
        static List<Type> s_pageViewTypes;
        static List<Type> s_itemViewModelTypes;
        static List<Type> s_itemViewTypes;
        static List<Type> s_abstractViewTypes;

        public static string GetPageViewModelName(Type type)
        {
            var name = type.Name;

            name = name.CutWhenEndsWith("WindowViewModel");
            name = name.CutWhenEndsWith("PageViewModel");
            name = name.CutWhenEndsWith("ViewModel");

            return name;
        }

        public static string GetItemViewModelName(Type type)
        {
            var name = type.Name;
            
            name = name.CutWhenEndsWith("ItemViewModel");
            name = name.CutWhenEndsWith("ViewModel");

            return name;
        }

        public static string GetPageViewModelName(this PageViewModel viewModel)
        {
            return GetPageViewModelName(viewModel.GetType());
        }

        public static string GetItemViewModelName(this ItemViewModel viewModel)
        {
            return GetItemViewModelName(viewModel.GetType());
        }

        public static Type FindPageViewType<PageViewModelType>(string removeNamePostfix = null)
            where PageViewModelType : PageViewModel
        {
            return FindPageViewType(GetPageViewModelName<PageViewModelType>(), removeNamePostfix);
        }

        public static string GetPageViewModelName<PageViewModelType>()
            where PageViewModelType : PageViewModel
        {
            return GetPageViewModelName(typeof(PageViewModelType));
        }

        public static Type FindPageViewType(string typeName, string removeNamePostfix = null)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            if (!s_isTypesExtracted)
                ExtractTypes();

            if (removeNamePostfix.IsNullOrWhiteSpace())
            {
                for (int k = 0; k < s_pageViewTypes.Count; k++)
                {
                    type = s_pageViewTypes[k];

                    if (StringComparer.OrdinalIgnoreCase.Compare(type.Name, typeName) == 0)
                        return type;
                }
            }
            else
            {
                for (int k = 0; k < s_pageViewTypes.Count; k++)
                {
                    type = s_pageViewTypes[k];

                    if (StringComparer.OrdinalIgnoreCase.Compare(type.Name.CutWhenEndsWith(removeNamePostfix), typeName) == 0)
                        return type;
                }
            }

            return null;
        }
        
        public static Type FindPageViewModelType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            if (!s_isTypesExtracted)
                ExtractTypes();

            for (int k = 0; k < s_pageViewModelTypes.Count; k++)
            {
                type = s_pageViewModelTypes[k];
                if (StringComparer.OrdinalIgnoreCase.Compare(type.Name, typeName) == 0)
                    return type;
            }

            return null;
        }

        public static string GetItemViewModelName<ItemViewModelType>()
           where ItemViewModelType : ItemViewModel
        {
            return GetItemViewModelName(typeof(ItemViewModelType));
        }


        public static Type FindItemViewType<ItemViewModelType>(string removeNamePostfix = null)
            where ItemViewModelType : ItemViewModel
        {
            return FindItemViewType(GetItemViewModelName<ItemViewModelType>(), removeNamePostfix);
        }

        public static Type FindItemViewType(string typeName, string removeNamePostfix = null)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            if (!s_isTypesExtracted)
                ExtractTypes();

            if (removeNamePostfix.IsNullOrWhiteSpace())
            {
                for (int k = 0; k < s_itemViewTypes.Count; k++)
                {
                    type = s_itemViewTypes[k];

                    if (StringComparer.OrdinalIgnoreCase.Compare(type.Name, typeName) == 0)
                        return type;
                }
            }
            else
            {
                for (int k = 0; k < s_itemViewTypes.Count; k++)
                {
                    type = s_itemViewTypes[k];

                    if (StringComparer.OrdinalIgnoreCase.Compare(type.Name.CutWhenEndsWith(removeNamePostfix), typeName) == 0)
                        return type;
                }
            }

            return null;
        }

        private static void ExtractTypes()
        {
            var pageViewModelTypes = new LinkedList<Type>();
            var pageViewTypes = new LinkedList<Type>();
            var itemViewModelTypes = new LinkedList<Type>();
            var itemViewTypes = new LinkedList<Type>();
            var abstractViewTypes = new LinkedList<Type>();


            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                var ass = assemblies[i];

                if (!ass.IsDefined(typeof(ExtractTypesAttribute)))
                    continue;

                if (ass.IsDynamic)
                    continue;

                var types = ass.GetExportedTypes();

                for (int k = 0; k < types.Length; k++)
                {
                    var type = types[k];

                    if (type.IsAbstract || type.IsGenericTypeDefinition || !type.IsClass)
                        continue;

                    if (type.IsDefined(typeof(PageViewModelAttribute), true))
                    {
                        pageViewModelTypes.AddLast(type);
                    }

                    if (type.IsDefined(typeof(PageViewAttribute), true))
                    {
                        pageViewTypes.AddLast(type);
                        abstractViewTypes.AddLast(type);
                    }

                    if (type.IsDefined(typeof(ItemViewModelAttribute), true))
                    {
                        itemViewModelTypes.AddLast(type);
                    }

                    if (type.IsDefined(typeof(ItemViewAttribute), true))
                    {
                        itemViewTypes.AddLast(type);
                        abstractViewTypes.AddLast(type);
                    }
                }
            }

            s_pageViewModelTypes = pageViewModelTypes.ToList();
            s_pageViewTypes = pageViewTypes.ToList();
            s_itemViewModelTypes = itemViewModelTypes.ToList();
            s_itemViewTypes = itemViewTypes.ToList();
            s_abstractViewTypes = abstractViewTypes.ToList();

            s_isTypesExtracted = true;
        }
        public static IDataBundle Wrap(IDictionary<string, object> dict)
        {
            return new DictionaryWrapper(dict);
        }

        public static IDataBundle CreateBundle()
        {
            return Wrap(new Dictionary<string, object>());
        }

        private class DictionaryWrapper : IDataBundle
        {
            readonly IDictionary<string, object> m_dict;

            public DictionaryWrapper(IDictionary<string, object> dict)
            {
                m_dict = dict;
            }

            public string GetString(string key, string defaultValue = null)
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                if (!m_dict.ContainsKey(key))
                    return defaultValue;
                return (string)m_dict[key];
            }

            public void SetString(string key, string value)
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                m_dict[key] = value;
            }
        }
    }
}
