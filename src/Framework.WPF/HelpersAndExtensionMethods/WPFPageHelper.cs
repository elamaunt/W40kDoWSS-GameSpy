﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace Framework.WPF
{
    public static class WPFPageHelper
    {
        static readonly string AssemblyName = Application.Current.GetType().Assembly.GetName().Name;
        static readonly Dictionary<string, string> XamlResources = new Dictionary<string, string>();
        static readonly Dictionary<string, string> ImageResources = new Dictionary<string, string>();

        static WPFPageHelper()
        {
            PreloadXamlPaths();
        }

        public static bool IsImageExists(string name)
        {
            return ImageResources.ContainsKey(name?.ToLowerInvariant());
        }

        public static bool TryGetImagePath(string name, out string path)
        {
            return ImageResources.TryGetValue(name?.ToLowerInvariant(), out path);
        }

        public static bool TryGetXamlPath(string prefix, string style, out string path)
        {
            return TryGetXamlPath(prefix + "_" + style, out path);
        }

        public static bool TryGetXamlPath(string name, out string path)
        {
            return XamlResources.TryGetValue(name?.ToLowerInvariant(), out path);
        }

        public static IBindableView InstantiateView(string prefix, string name)
        {
            var contentElementType = FindElementViewType(name);

            if (contentElementType != null)
                return (IBindableView)Activator.CreateInstance(contentElementType);

            name = (prefix.ToLowerInvariant()) + "_" + (name.ToLowerInvariant());

            if (XamlResources.TryGetValue(name, out string path))
            {
                var resourceLocater = new System.Uri($@"/{AssemblyName};component/{path}.xaml", System.UriKind.Relative);
                return (IBindableView)Application.LoadComponent(resourceLocater);
            }

            throw new Exception($"Xaml resource was not found with name: {name}.xaml");
        }

        public static BindableControl InstantiateControl(string name)
        {
            var contentElementType = FindElementViewType(name);

            if (contentElementType != null)
                return (BindableControl)Activator.CreateInstance(contentElementType);

            name = "element_" + (name.ToLowerInvariant());

            if (XamlResources.TryGetValue(name, out string path))
            {
                var resourceLocater = new System.Uri($@"/{AssemblyName};component/{path}.xaml", System.UriKind.Relative);
                return (BindableControl)Application.LoadComponent(resourceLocater);
            }

            throw new Exception($"Xaml resource was not found with name: {name}.xaml");
        }

        public static BindablePage InstantiatePage<T>()
            where T : PageViewModel, new()
        {
            var page = InstantiatePage(PageHelper.GetPageViewModelName<T>());
            page.ViewModel = new T();
            return page;
        }

        public static BindableWindow InstantiateWindow(WindowViewModel viewModel)
        {
            var page = InstantiateWindow(PageHelper.GetPageViewModelName(viewModel));
            page.ViewModel = viewModel;
            return page;
        }

        public static BindableWindow InstantiateWindow<T>()
            where T : WindowViewModel, new()
        {
            var page = InstantiateWindow(PageHelper.GetPageViewModelName<T>());
            page.ViewModel = new T();
            return page;
        }

        public static BindableWindow InstantiateWindow(string name)
        {
            var contentPageType = FindPageViewType(name);

            if (contentPageType != null)
                return (BindableWindow)Activator.CreateInstance(contentPageType);

            name = "window_" + (name.ToLowerInvariant());

            if (XamlResources.TryGetValue(name, out string path))
            {
                var resourceLocater = new System.Uri($@"/{AssemblyName};component/{path}.xaml", System.UriKind.Relative);
                return (BindableWindow)Application.LoadComponent(resourceLocater);
            }

            throw new Exception($"Xaml resource was not found with name: {name}.xaml");
        }

        public static BindablePage InstantiatePage(string name)
        {
            var contentPageType = FindPageViewType(name);

            if (contentPageType != null)
                return (BindablePage)Activator.CreateInstance(contentPageType);

            name = "page_" + (name.ToLowerInvariant());

            if (XamlResources.TryGetValue(name, out string path))
            {
                var resourceLocater = new System.Uri($@"/{AssemblyName};component/{path}.xaml", System.UriKind.Relative);
                return (BindablePage)Application.LoadComponent(resourceLocater);
            }

            throw new Exception($"Xaml resource was not found with name: {name}.xaml");
        }

        public static Type FindWindowViewType(string typeName)
        {
            return PageHelper.FindPageViewType(typeName, "window");
        }

        public static Type FindPageViewType(string typeName)
        {
            return PageHelper.FindPageViewType(typeName, "page");
        }

        public static Type FindElementViewType(string typeName)
        {
            return PageHelper.FindItemViewType(typeName, "element");
        }

        public static void PreloadXamlPaths()
        {
            var assembly = Application.Current.GetType().GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".g.resources"))
            {
                using (var reader = new ResourceReader(stream))
                {
                    foreach (DictionaryEntry entry in reader)
                    {
                        string str = (string)entry.Key;

                        if (str.EndsWith(".baml", StringComparison.OrdinalIgnoreCase))
                        {
                            XamlResources[Path.GetFileNameWithoutExtension(str)] = str.Substring(0, str.Length - 5);
                            continue;
                        }

                        if (str.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || str.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        {
                            ImageResources[Path.GetFileNameWithoutExtension(str)] = "/" + str;
                            continue;
                        }
                    }
                }
            }
        }
    }
}
