using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Framework
{
    public class Frame : IFrame, INotifyPropertyChanged
    {
        Dictionary<TypeInfo, object> _extensions;
        HashSet<TypeInfo> _extentionRequests;

        public event PropertyChangedEventHandler PropertyChanged;
        
        public void FirePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ExtensionType GetExtension<ExtensionType>()
            where ExtensionType : class
        {
            if (_extensions.IsNullOrEmpty())
                 return default(ExtensionType);

            var type = typeof(ExtensionType).GetTypeInfo();

            if (_extensions.TryGetValue(type, out object extension))
                return (ExtensionType)extension;

            return default(ExtensionType);
        }

        public void SetExtension<ExtensionType>(ExtensionType extension)
            where ExtensionType : class
        {
            if (_extensions == null)
                _extensions = new Dictionary<TypeInfo, object>();

            _extensions[typeof(ExtensionType).GetTypeInfo()] = extension;
        }

        public bool IsExtentionRequested<ExtensionType>()
            where ExtensionType : class
        {
            if (_extentionRequests.IsNullOrEmpty())
                return false;

            return _extentionRequests.Contains(typeof(ExtensionType).GetTypeInfo());
        }

        public void RequestViewExtention<ExtensionType>() 
            where ExtensionType : class
        {
            if (IsExtentionRequested<ExtensionType>())
                return;

            if (_extentionRequests == null)
                _extentionRequests = new HashSet<TypeInfo>();


            _extentionRequests.Add(typeof(ExtensionType).GetTypeInfo());
        }

        public virtual void Cleanup()
        {
            // Nothing
        }
    }
}