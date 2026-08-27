using System;

namespace YangTools.Scripts.Core.RedDotSystem
{
    /// <summary>
    /// 表示源字符串中的一段连续字符
    /// </summary>
    public struct RangeString : IEquatable<RangeString>
    {
        private readonly string source; //源字符串
        private readonly int startIndex; //起始下标
        private readonly int endIndex; //结束下标
        private readonly int length; //字符数量
        private readonly bool isSourceNullOrEmpty; //源字符串是否为空
        private int hashCode; //缓存哈希值

        /// <summary>
        /// 创建字符串范围
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="startIndex">起始下标</param>
        /// <param name="endIndex">结束下标</param>
        public RangeString(string source, int startIndex, int endIndex)
        {
            this.source = source;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            length = endIndex - startIndex + 1;
            isSourceNullOrEmpty = string.IsNullOrEmpty(source);
            hashCode = 0;
        }

        /// <summary>
        /// 判断两个字符串范围的内容是否相同
        /// </summary>
        /// <param name="other">另一个字符串范围</param>
        /// <returns>内容是否相同</returns>
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

            for (int i = startIndex, j = other.startIndex; i <= endIndex; i++, j++)
            {
                if (source[i] != other.source[j])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获得范围内容的哈希值
        /// </summary>
        /// <returns>哈希值</returns>
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

        /// <summary>
        /// 获得范围内的字符串
        /// </summary>
        /// <returns>范围字符串</returns>
        public override string ToString()
        {
            if (isSourceNullOrEmpty || length <= 0)
            {
                return string.Empty;
            }

            return source.Substring(startIndex, length);
        }
    }
}
