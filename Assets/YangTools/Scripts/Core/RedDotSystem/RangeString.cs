using System;

namespace YangTools.Scripts.Core.RedDotSystem
{
    public struct RangeString : IEquatable<RangeString>
    {
        private readonly string source;
        private readonly int startIndex;
        private readonly int endIndex;
        private readonly int length;
        private readonly bool isSourceNullOrEmpty;
        private int hashCode;

        public RangeString(string source, int startIndex, int endIndex)
        {
            this.source = source;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            length = endIndex - startIndex + 1;
            isSourceNullOrEmpty = string.IsNullOrEmpty(source);
            hashCode = 0;
        }

        public bool Equals(RangeString other)
        {
            var isOtherIsNullOrEmpty = string.IsNullOrEmpty(other.source);

            if (isSourceNullOrEmpty && isOtherIsNullOrEmpty)
            {
                return true;
            }

            if (isSourceNullOrEmpty || isOtherIsNullOrEmpty)
            {
                return false;
            }

            if (length != other.length)
            {
                return false;
            }

            for (int i = startIndex, j = other.startIndex; i < endIndex; i++, j++)
            {
                if (source[i] != other.source[j])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            if (hashCode == 0 && !isSourceNullOrEmpty)
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    hashCode = 31 * hashCode + source[i];
                }
            }

            return hashCode;
        }

        public override string ToString()
        {
            RedDotMgr.Instance.CachedStrBuilder.Clear();
            for (int i = startIndex; i <= endIndex; i++)
            {
                RedDotMgr.Instance.CachedStrBuilder.Append(source[i]);
            }

            var str = RedDotMgr.Instance.CachedStrBuilder.ToString();

            return str;
        }
    }
}