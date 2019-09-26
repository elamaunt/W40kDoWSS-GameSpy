using GSMasterServer.Data;
using System;
using System.Runtime.Caching;

namespace GSMasterServer.Utils
{
    public static class ProfilesCache
    {
        static readonly MemoryCache ProfilesByIdCache = new MemoryCache("ProfilesById");
        static readonly MemoryCache ProfilesByNameCache = new MemoryCache("ProfilesByName");

        public static void UpdateProfilesCache(ProfileDBO profile)
        {
            var idString = profile.Id.ToString();

            if (ProfilesByIdCache.Contains(idString))
                ProfilesByIdCache.Remove(idString, CacheEntryRemovedReason.Removed);

            if (ProfilesByNameCache.Contains(profile.Name))
                ProfilesByNameCache.Remove(profile.Name, CacheEntryRemovedReason.Removed);

            ProfilesByIdCache.Add(new CacheItem(idString, profile), new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromMinutes(25)
            });

            ProfilesByNameCache.Add(new CacheItem(profile.Name, profile), new CacheItemPolicy()
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

                if (stats == null)
                    return null;

                ProfilesByIdCache.Add(new CacheItem(stats.Id.ToString(), stats), new CacheItemPolicy()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(25)
                });

                return stats;
            }
        }

        public static ProfileDBO GetProfileByName(string name)
        {
            if (ProfilesByNameCache.Contains(name))
            {
                return (ProfileDBO)ProfilesByNameCache.Get(name);
            }
            else
            {
                var stats = Database.MainDBInstance.GetProfileByName(name);

                if (stats == null)
                    return null;

                ProfilesByNameCache.Add(new CacheItem(name, stats), new CacheItemPolicy()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(25)
                });

                return stats;
            }
        }
    }
}
