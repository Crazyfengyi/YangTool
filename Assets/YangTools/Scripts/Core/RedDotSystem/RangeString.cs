using System;
using System.Collections.Generic;
using GameMain.RedDot;

namespace GameMain
{
    public struct RangeString : IEquatable<RangeString>
    {
        private readonly string _source;

        private readonly int _startIndex;

        private readonly int _endIndex;

        private readonly int _length;

        private readonly bool _isSourceNullOrEmpty;

        private int _hashCode;

        public RangeString(string source, int startIndex, int endIndex)
        {
            _source = source;
            
            _startIndex = startIndex;
            
            _endIndex = endIndex;

            _length = endIndex - startIndex + 1;
            
            _isSourceNullOrEmpty = string.IsNullOrEmpty(source);

            _hashCode = 0;
        }

        public bool Equals(RangeString other)
        {
            var isOtherIsNullOrEmpty = string.IsNullOrEmpty(other._source);

            if (_isSourceNullOrEmpty && isOtherIsNullOrEmpty)
            {
                return true;
            }

            if (_isSourceNullOrEmpty || isOtherIsNullOrEmpty)
            {
                return false;
            }

            if (_length != other._length)
            {
                return false;
            }

            for (int i = _startIndex, j = other._startIndex; i < _endIndex; i++, j++)
            {
                if (_source[i] != other._source[j])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            if (_hashCode == 0 && !_isSourceNullOrEmpty)
            {
                for (int i = _startIndex; i <= _endIndex; i++)
                {
                    _hashCode = 31 * _hashCode + _source[i];
                }
            }

            return _hashCode;
        }

        public override string ToString()
        {
            RedDotMgr.Instance.CachedSb.Clear();

            for (int i = _startIndex; i <= _endIndex; i++)
            {
                RedDotMgr.Instance.CachedSb.Append(_source[i]);
            }

            var str = RedDotMgr.Instance.CachedSb.ToString();

            return str;
        }
    }
}
