using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MongoDB.Prime.Extensions
{
    public static class StringExtensions
    {
        public static string ToJsonArray(this IEnumerable<string> jsonArray)
        {
            return string.Concat("[", string.Join(",", jsonArray), "]");
        }
    }
}
