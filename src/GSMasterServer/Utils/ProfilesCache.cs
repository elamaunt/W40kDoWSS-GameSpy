using GSMasterServer.Data;
using System;
using System.Runtime.Caching;

namespace GSMasterServer.Utils
{
    public static class ProfilesCache
    {
        static readonly MemoryCache ProfilesByIdCache = new MemoryCache("Profiles");

        public static void UpdateProfilesCache(ProfileDBO profile)
        {
            var idString = profile.Id.ToString();

            if (ProfilesByIdCache.Contains(idString))
                ProfilesByIdCache.Remove(idString, CacheEntryRemovedReason.Removed);

            ProfilesByIdCache.Add(new CacheItem(idString, profile), new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromMinutes(25)
            });
        }

        public static ProfileDBO GetProfileByPid(string pid)
        {
            if (ProfilesByIdCache.Contains(pid))
            {
                return (ProfileDBO)ProfilesByIdCache.Get(pid);
            }
            else
            {
                var stats = Database.MainDBInstance.GetProfileById(long.Parse(pid));

                ProfilesByIdCache.Add(new CacheItem(stats.Id.ToString(), stats), new CacheItemPolicy()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(25)
                });

                return stats;
            }
        }

        public static ProfileDBO GetProfileByName(string name)
        {
            var stats = Database.MainDBInstance.GetProfileByName(name);

            ProfilesByIdCache.Add(new CacheItem(stats.Id.ToString(), stats), new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromMinutes(25)
            });

            return stats;
        }
    }
}
