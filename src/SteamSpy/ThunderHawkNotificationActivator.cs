using DesktopNotifications;
using System;
using System.Runtime.InteropServices;

namespace ThunderHawk
{
    // The GUID CLSID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("95da8d3b-c508-4ba4-82af-e5174cd85657"), ComVisible(true)]
    public class ThunderHawkNotificationActivator : DesktopNotifications.NotificationActivator
    {
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            // TODO: Handle activation
        }
    }
}
