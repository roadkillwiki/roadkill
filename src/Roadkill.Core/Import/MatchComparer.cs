using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Import
{
    class MatchComparer : IEqualityComparer<Match>
    {
        public bool Equals(Match a, Match b)
        {
            return a.Value == b.Value;
        }

        public int GetHashCode(Match match)
        {
            return match.Value.GetHashCode();
        }
    }
}