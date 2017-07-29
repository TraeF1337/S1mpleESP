using Ennui.Api.Builder;
using Ennui.Api.Direct.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleESP
{
    public class OnlyThisIds<T> : Filter<T> where T : ISimulationObject
    {
        private HashSet<long> ids;

        public OnlyThisIds(HashSet<long> ids)
        {
            this.ids = ids;
        }

        public bool Ignore(T t)
        {
            var id = t.Id;
            foreach (long cur in ids)
            {
                if (id.Equals(cur))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
