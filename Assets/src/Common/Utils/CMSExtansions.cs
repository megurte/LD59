using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.Utils
{
    public static class CMSExtansions
    {
        public static CMSEntity GetRandom(this List<CMSEntityPfb> list)
        {
            return list[Random.Range(0, list.Count)].AsEntity();
        }
        
        public static CMSEntity GetRandom(this List<CMSEntity> list)
        {
            return list[Random.Range(0, list.Count)];
        }
        
        public static CMSEntity GetRandom(this List<CMSEntityPfb> list, List<CMSEntityPfb> exclude)
        {
            var excludeIds = exclude.Select(e => e.GetId()).ToHashSet();
            var filtered = list.Where(e => !excludeIds.Contains(e.GetId())).ToList();

            if (filtered.Count == 0) return null;

            return filtered[Random.Range(0, filtered.Count)].AsEntity();
        }
        
        public static CMSEntity GetRandom(this List<CMSEntity> list, List<CMSEntityPfb> exclude)
        {
            var excludeIds = exclude.Select(e => e.GetId()).ToHashSet();
            var filtered = list.Where(e => !excludeIds.Contains(e.id)).ToList();

            if (filtered.Count == 0) return null;

            return filtered[Random.Range(0, filtered.Count)];
        }
    }
}