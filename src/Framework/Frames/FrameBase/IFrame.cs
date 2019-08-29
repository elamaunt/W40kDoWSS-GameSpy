using System.ComponentModel;

namespace Framework
{
    public interface IFrame : INotifyPropertyChanged
    {
        void FirePropertyChanged(string propertyName);

        void SetExtension<ExtensionType>(ExtensionType extension)
            where ExtensionType : class;

        ExtensionType GetExtension<ExtensionType>()
            where ExtensionType : class;

        void RequestViewExtention<ExtensionType>()
            where ExtensionType : class;

        void Cleanup();
    }
}