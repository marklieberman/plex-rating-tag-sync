using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlexRatingTagSync
{
    internal class SyncOptions
    {
        public string ServerBase { get; private set; }
        public string PlexToken { get; private set; }
        public string LibrarySection { get; private set; }
        public int ModifiedDays { get; private set; }
        public DateTime? SyncThreshold { get; private set; }

        public SyncOptions(string serverBase, string plexToken, string librarySection, int modifiedDays)
        {
            if (!serverBase.ToLower().StartsWith("http"))
            {
                serverBase = "http://" + serverBase;
            }

            ServerBase = serverBase;
            PlexToken = plexToken;
            LibrarySection = librarySection;
            ModifiedDays = modifiedDays;

            // Determine threshold date if only updating modified files.
            if (modifiedDays > 0) 
            {
                SyncThreshold = DateTime.Now.AddDays(-modifiedDays);
            }
        }
    }
}
