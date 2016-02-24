using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MattermostCrmService
{

    public static class StringHelper
    {
        public static IEnumerable<string> tokens(this string input)
        {
            Regex tokenSplit = new Regex("(?:^|\\s)(\"(?:[^\"]+|\"\")*\"|[^\\s]*)", RegexOptions.Compiled);

            foreach (Match match in tokenSplit.Matches(input ?? ""))
            {
                var val = match.Value.Trim();
                if (val.StartsWith("\""))
                {
                    val = val.Substring(1, val.Length - 2).Trim();
                }
                if (!string.IsNullOrWhiteSpace(val))
                {
                    yield return val;
                }
            }
        }
    }
}