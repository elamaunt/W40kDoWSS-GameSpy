using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF2Statistics.Gamespy.Redirector
{
    public class HostsFileIcs : HostsFile
    {
        /// <summary>
        /// Direct Filepath to the hosts file
        /// </summary>
        public static string FilePath = Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts.ics");

        /// <summary>
        /// Creates a new instance of HostsFileIcs
        /// </summary>
        public HostsFileIcs() : base()
        {
            // We dont know?
            try
            {
                // Get the Hosts file object
                HostFile = new FileInfo(FilePath);

                // Make sure file exists
                if (!HostFile.Exists)
                    HostFile.Open(FileMode.Create).Close();

                // If HOSTS file is readonly, remove that attribute!
                if (HostFile.IsReadOnly)
                {
                    try
                    {
                        HostFile.IsReadOnly = false;
                    }
                    catch (Exception e)
                    {
                        L.LogError("HOSTS.ics file is READONLY, Attribute cannot be removed: " + e.Message);
                        LastException = e;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                L.LogError("Program cannot access HOSTS.ics file in any way: " + e.Message);
                LastException = e;
                return;
            }

            // Make sure we can read the file amd write to it!
            try
            {
                // Get the hosts file contents
                using (StreamReader Rdr = new StreamReader(HostFile.OpenRead()))
                {
                    CanRead = true;
                    while (!Rdr.EndOfStream)
                        OrigContents.Add(Rdr.ReadLine());
                }
            }
            catch (Exception e)
            {
                L.LogError("Unable to READ the HOSTS.ics file: " + e.Message);
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
                L.LogError("Unable to WRITE to the HOSTS.ics file: " + e.Message);
                LastException = e;
                return;
            }

            // Parse file entries
            base.ParseEntries();
        }
    }
}
