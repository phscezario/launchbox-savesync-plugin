using System;
using System.Collections.Generic;
using System.Text;

namespace SaveSyncPlugin.Core.Sync
{
    /// <summary>
    /// Matches a configured title against the titles of the LaunchBox library.
    ///
    /// LaunchBox/RomM rename games when the metadata is refreshed ("Big Bang Beat:
    /// 1st Impression" vs "BigBang Beat 1st Impression"), and the old exact-match
    /// lookup silently stopped syncing those saves. Exact matching is still tried
    /// first and keeps the previous behaviour, including picking the first game when
    /// two library entries share a title.
    ///
    /// Fuzzy matching is deliberately conservative: a candidate is only accepted
    /// when exactly one library entry looks related. When two entries match, the
    /// result is <c>-1</c> instead of a guess, because a wrong guess would sync the
    /// save of another game.
    /// </summary>
    public static class TitleMatcher
    {
        // Known abbreviations used by the library/configs. Kept tiny on purpose:
        // every entry is a rule that can create a false match.
        private static readonly Dictionary<string, string> Abbreviations =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "tmnt", "teenage mutant ninja turtles" }
            };

        /// <summary>
        /// Returns the index of the single candidate that matches
        /// <paramref name="target"/>, or <c>-1</c> when nothing matches or the match
        /// would be ambiguous. <paramref name="exact"/> is true only for a
        /// case-insensitive exact match.
        /// </summary>
        public static int FindUniqueMatch(string target, IList<string> candidates, out bool exact)
        {
            exact = false;
            if (string.IsNullOrWhiteSpace(target) || candidates == null || candidates.Count == 0)
                return -1;

            var wanted = target.Trim();

            // 1) Exact, case-insensitive. First wins, as before this class existed.
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (!string.IsNullOrWhiteSpace(candidate) &&
                    candidate.Trim().Equals(wanted, StringComparison.OrdinalIgnoreCase))
                {
                    exact = true;
                    return i;
                }
            }

            // 2) Same words once punctuation and spacing are ignored.
            var key = Normalize(wanted);
            if (key.Length == 0)
                return -1;

            var keyCompact = Compact(key);
            var hits = new List<int>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = Normalize(candidates[i]);
                if (candidate.Length == 0)
                    continue;

                if (candidate == key || IsRelated(key, keyCompact, candidate))
                    hits.Add(i);
            }

            return hits.Count == 1 ? hits[0] : -1;
        }

        /// <summary>
        /// Lowercase, punctuation becomes a separator and known abbreviations are
        /// expanded: "Big Bang Beat: 1st Impression" becomes
        /// "big bang beat 1st impression".
        /// </summary>
        public static string Normalize(string title)
        {
            var tokens = Tokenize(title);
            if (tokens.Count == 0)
                return string.Empty;

            var sb = new StringBuilder(title.Length + 16);
            foreach (var token in tokens)
            {
                string word;
                if (!Abbreviations.TryGetValue(token, out word))
                    word = token;

                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(word);
            }

            return sb.ToString();
        }

        private static bool IsRelated(string normalized, string compact, string candidate)
        {
            if (compact.Length == 0)
                return false;

            // "bigbang beat 1st impression" == "big bang beat 1st impression"
            // once the spaces are removed.
            if (compact == Compact(candidate))
                return true;

            var a = Split(normalized);
            var b = Split(candidate);
            if (a.Length == 0 || b.Length == 0)
                return false;

            var shorter = a.Length <= b.Length ? a : b;
            var longer = a.Length <= b.Length ? b : a;

            // "Dusty Revenge" is a prefix of "Dusty Revenge CO-OP Edition";
            // "Crazy Taxi 3" is a prefix of "Crazy Taxi 3: High Roller".
            if (shorter.Length >= 2 && IsPrefix(shorter, longer))
                return true;

            // "Mega Man ZX Prequel" appears in order inside
            // "Mega Man ZX: The Prequel, Light & Dark".
            if (shorter.Length >= 3 && IsSubsequence(shorter, longer))
                return true;

            return false;
        }

        private static List<string> Tokenize(string title)
        {
            var tokens = new List<string>();
            if (string.IsNullOrEmpty(title))
                return tokens;

            var sb = new StringBuilder();
            foreach (var raw in title)
            {
                var ch = char.ToLowerInvariant(raw);
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                }
                else if (sb.Length > 0)
                {
                    tokens.Add(sb.ToString());
                    sb.Clear();
                }
            }

            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens;
        }

        private static string Compact(string normalized)
        {
            if (string.IsNullOrEmpty(normalized))
                return string.Empty;

            var sb = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                if (ch != ' ')
                    sb.Append(ch);
            }

            return sb.ToString();
        }

        private static string[] Split(string normalized)
        {
            return normalized.Length == 0
                ? new string[0]
                : normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool IsPrefix(string[] shorter, string[] longer)
        {
            if (shorter.Length > longer.Length)
                return false;

            for (var i = 0; i < shorter.Length; i++)
            {
                if (!string.Equals(shorter[i], longer[i], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        private static bool IsSubsequence(string[] shorter, string[] longer)
        {
            var j = 0;
            for (var i = 0; i < longer.Length && j < shorter.Length; i++)
            {
                if (string.Equals(shorter[j], longer[i], StringComparison.Ordinal))
                    j++;
            }

            return j == shorter.Length;
        }
    }
}
