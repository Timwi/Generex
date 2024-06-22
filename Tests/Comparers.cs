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

    sealed class CaseInsensitiveCharEqualityComparer : IEqualityComparer<char>
    {
        public bool Equals(char x, char y)
        {
            return char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
        }

        public int GetHashCode(char obj)
        {
            return char.ToUpperInvariant(obj).GetHashCode();
        }
    }
}
