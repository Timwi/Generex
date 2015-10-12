using System.Collections.Generic;

namespace RT.Generexes.Tests
{
    sealed class Mod7IntEqualityComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y)
        {
            return (x % 7) == (y % 7);
        }

        public int GetHashCode(int obj)
        {
            return obj % 7;
        }
    }
}
