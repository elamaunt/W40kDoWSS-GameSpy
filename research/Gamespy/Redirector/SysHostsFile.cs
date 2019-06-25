using PRMasterServer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace BF2Statistics.Gamespy.Redirector
{
    public class SysHostsFile : HostsFile
    {
        /// <summary>
        /// Direct Filepath to the hosts file
        /// </summary>
        public static string FilePath = Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");

        /// <summary>
        /// Specifies whether the HOSTS file is locked
        /// </summary>
        public static bool IsLocked { get; protected set; }

        /// <summary>
        /// Creates a new instance of SysHostsFile
        /// </summary>
        public SysHostsFile() : base()
        {
            // We dont know?
            IsLocked = false;
            try
            {
                // Get the Hosts file object
                HostFile = new FileInfo(FilePath);

                // If HOSTS file is readonly, remove that attribute!
                if (HostFile.IsReadOnly)
                {
                    try
                    {
                        HostFile.IsReadOnly = false;
                    }
                    catch (Exception e)
                    {
                        L.LogError("HOSTS file is READONLY, Attribute cannot be removed: " + e.Message);
                        LastException = e;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                L.LogError("Program cannot access HOSTS file in any way: " + e.Message);
                LastException = e;
                return;
            }

            // Try to get the Access Control for the hosts file
            try
            {
                Security = HostFile.GetAccessControl();
            }
            catch (Exception e)
            {
                L.LogError("Unable to get HOSTS file Access Control: " + e.Message);
                LastException = e;
                return;
            }

            // Make sure we can read the file amd write to it!
            try
            {
                // Unlock hosts file
                if (!UnLock())
                    return;

                // Get the hosts file contents
                using (StreamReader Rdr = new StreamReader(HostFile.OpenRead()))
                {
                    while (!Rdr.EndOfStream)
                        OrigContents.Add(Rdr.ReadLine());
                }

                CanRead = true;
            }
            catch (Exception e)
            {
                L.LogError("Unable to READ the HOSTS file: " + e.Message);
                LastException = e;
                return;
            }

            // Check that we can write to the hosts file
            try
            {
                using (FileStream Stream = HostFile.OpenWrite())
                    CanWrite = true;
            }
            catch (Exception e)
            {
                L.LogError("Unable to WRITE to the HOSTS file: " + e.Message);
                LastException = e;
                return;
            }

            // Parse file entries
            base.ParseEntries();
        }

        /// <summary>
        /// Removes the "Deny" read permissions, and adds the "Allow" read permission
        /// on the HOSTS file. If the Hosts file cannot be unlocked, the exception error
        /// will be logged in the App error log
        /// </summary>
        public bool UnLock()
        {
            // Make sure we have a security object
            if (Security == null)
                return false;

            // Allow ReadData
            Security.RemoveAccessRule(new FileSystemAccessRule(WorldSid, FileSystemRights.ReadData, AccessControlType.Deny));
            Security.AddAccessRule(new FileSystemAccessRule(WorldSid, FileSystemRights.ReadData, AccessControlType.Allow));

            // Try and set the new access control
            try
            {
                HostFile.SetAccessControl(Security);
                IsLocked = false;
                return true;
            }
            catch (Exception e)
            {
                L.LogError("Unable to REMOVE the Readonly rule on Hosts File: " + e.Message);
                LastException = e;
                return false;
            }
        }

        /// <summary>
        /// Removes the "Allow" read permissions, and adds the "Deny" read permission
        /// on the HOSTS file. If the Hosts file cannot be locked, the exception error
        /// will be logged in the App error log
        /// </summary>
        public bool Lock()
        {
            // Make sure we have a security object
            if (Security == null)
                return false;

            // Donot allow Read for the Everyone Sid. This prevents the BF2 client from reading the hosts file
            Security.RemoveAccessRule(new FileSystemAccessRule(WorldSid, FileSystemRights.ReadData, AccessControlType.Allow));
            Security.AddAccessRule(new FileSystemAccessRule(WorldSid, FileSystemRights.ReadData, AccessControlType.Deny));

            // Try and set the new access control
            try
            {
                HostFile.SetAccessControl(Security);
                IsLocked = true;
                return true;
            }
            catch (Exception e)
            {
                L.LogError("Unable to REMOVE the Readonly rule on Hosts File: " + e.Message);
                LastException = e;
                return false;
            }
        }
    }
}
