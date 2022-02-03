using System;
using System.Collections.Generic;

namespace MTGOReplayToolWpf
{
    class MyComparer<T> : IEqualityComparer<T> where T : NewMatch
    {
        public Boolean Equals(T x, T y)
        {
            return x.Id.Equals(y.Id);
        }

        public Int32 GetHashCode(T obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
