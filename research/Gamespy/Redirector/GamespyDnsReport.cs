using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BF2Statistics.Net;

namespace BF2Statistics.Gamespy.Redirector
{
    public class GamespyDnsReport
    {
        /// <summary>
        /// Gets whether there was an error in this report
        /// </summary>
        public bool ErrorFree
        {
            get { return FaultedEntries.Length == 0; }
        }

        /// <summary>
        /// Returns an array of Dns Cache results related to gamespy services only
        /// </summary>
        public Dictionary<string, DnsCacheResult> Entries = new Dictionary<string, DnsCacheResult>();

        /// <summary>
        /// Returns an array of Dns Cache entries that did not contain the expected IPAddress response
        /// </summary>
        public DnsCacheResult[] FaultedEntries
        {
            get 
            { 
                return (from x in Entries.Values where x.GotExpectedResult == false select x).ToArray();
            }
        }

        /// <summary>
        /// Gets or Sets the time this report was generated
        /// </summary>
        public DateTime LastRefresh;

        /// <summary>
        /// Creates a new instance of GamespyDnsReport
        /// </summary>
        public GamespyDnsReport()
        {
            LastRefresh = DateTime.Now;
        }

        /// <summary>
        /// Adds or Updates a Dns Cache Result to the list of entries
        /// </summary>
        /// <param name="Result"></param>
        public void AddOrUpdate(DnsCacheResult Result)
        {
            // Add if the entry already exists
            if (Entries.ContainsKey(Result.HostName))
            {
                Entries[Result.HostName] = Result;
            }
            else // Update entry
            {
                Entries.Add(Result.HostName, Result);
            }
        }
    }
}
