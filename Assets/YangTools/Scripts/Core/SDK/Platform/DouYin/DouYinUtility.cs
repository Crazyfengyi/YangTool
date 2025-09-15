using System;
using System.Collections.Generic;
using System.Text;

namespace TTSDK
{
    public static class DouYinUtility
    {
        public static string EnCode(string str)
        {
            return Uri.EscapeDataString(str);
        }
        
        public static string Decode(string str)
        {
            return Uri.UnescapeDataString(str);
        }
    }
}