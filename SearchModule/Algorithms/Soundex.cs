using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using static SearchModule.Utils.StringHelpers;

namespace SearchModule.Algorithms
{
    /// <summary>
    /// French phonetic algorithm that computes a numeric value representing
    /// the pronunciation of a word. Words that sound alike produce identical
    /// or very close values. Based on an 18-step transformation pipeline.
    /// See Documentation/SoundexEtapes.txt for detailed step descriptions.
    /// </summary>
    public class Soundex
    {
        /// <summary>
        /// Computes the phonetic value for an expression (may contain multiple words).
        /// Parenthesized content is stripped before processing.
        /// </summary>
        public static double GetSoundex(string expression)
        {
            expression = Regex.Replace(expression, @" ?\(.*?\)", string.Empty);
            double soundexValue = 0;
            foreach (string substring in expression.Split(' '))
            {
                if (!string.IsNullOrWhiteSpace(substring))
                {
                    soundexValue += ComputeSoundex(substring);
                }
            }
            return soundexValue;
        }

        private static double ComputeSoundex(string word)
        {
            word = word.ToLower().Trim();
            word = RemoveNonLetterChar(word);
            word = RemoveLineEndings(word);
            word = RemoveAccentedLetters(word);

            if (string.IsNullOrWhiteSpace(word) || word.Length < 3) return 0;

            // Step 1: Replace y with i
            word = word.Replace('y', 'i');

            // Step 2: Remove silent h (keep after c, s, p)
            foreach (int index in GetAllIndexesOfString(word, "h"))
            {
                if (index == 0)
                    word = word.Remove(index, 1);
                else if (new[] { "c", "s", "p" }.Contains(word.Substring(index - 1, 1)) == false)
                    word = word.Remove(index, 1);
            }

            // Step 3: ph -> f
            word = word.Replace("ph", "f");

            // Step 4: Soft g before nasal vowels -> k
            var step4 = new Dictionary<string, string>
            {
                ["gan"] = "kan", ["gam"] = "kam",
                ["gain"] = "kain", ["gaim"] = "kaim"
            };
            word = step4.Aggregate(word, (result, s) => result.Replace(s.Key, s.Value));

            // Step 5: Nasal vowels before vowel -> yn
            string[] step5 = { "ain", "ein", "aim", "eim" };
            foreach (var pattern in step5)
            {
                foreach (int index in GetAllIndexesOfString(word, pattern))
                {
                    if (index + 3 < word.Length && new[] { "a", "e", "i", "o", "u" }.Contains(word.Substring(index + 3, 1)))
                    {
                        word = word.Remove(index, 3);
                        word = word.Insert(index, "yn");
                    }
                }
            }

            // Step 6: Compound vowels -> numeric codes
            var step6 = new Dictionary<string, string>
            {
                ["eau"] = "o", ["oua"] = "2",
                ["ein"] = "4", ["ain"] = "4", ["eim"] = "4", ["aim"] = "4"
            };
            word = step6.Aggregate(word, (result, s) => result.Replace(s.Key, s.Value));

            // Step 7: E-sounds -> y
            var step7 = new Dictionary<string, string>
            {
                ["é"] = "y", ["è"] = "y", ["ê"] = "y",
                ["ai"] = "y", ["ei"] = "y",
                ["er"] = "yr", ["ess"] = "yss", ["et"] = "yt",
            };
            word = step7.Aggregate(word, (result, s) => result.Replace(s.Key, s.Value));

            // Step 8: Nasal vowels (not before vowel) -> numeric codes
            var step8 = new Dictionary<string, string>
            {
                ["an"] = "1", ["on"] = "1", ["am"] = "1",
                ["en"] = "1", ["em"] = "1", ["in"] = "4"
            };
            foreach (KeyValuePair<string, string> kvp in step8)
            {
                foreach (int index in GetAllIndexesOfString(word, kvp.Key))
                {
                    if (index + 2 < word.Length && new[] { "a", "e", "i", "o", "u", "1", "2", "3", "4" }.Contains(word.Substring(index + 2, 1)) == false)
                    {
                        word = word.Remove(index, 2);
                        word = word.Insert(index, kvp.Value);
                    }
                }
            }

            // Step 9: Intervocalic s -> z
            string[] vowelsAndCodes = { "a", "e", "i", "o", "u", "1", "2", "3", "4" };
            foreach (int index in GetAllIndexesOfString(word, "s"))
            {
                if (index + 1 < word.Length && index > 0 &&
                    vowelsAndCodes.Contains(word.Substring(index + 1, 1)) &&
                    vowelsAndCodes.Contains(word.Substring(index - 1, 1)))
                {
                    word = word.Remove(index, 1);
                    word = word.Insert(index, "z");
                }
            }

            // Step 10: Diphthongs -> codes
            var step10 = new Dictionary<string, string>
            {
                ["oe"] = "e", ["eu"] = "e", ["au"] = "o",
                ["oi"] = "2", ["oy"] = "2", ["ou"] = "3"
            };
            word = step10.Aggregate(word, (result, s) => result.Replace(s.Key, s.Value));

            // Step 11: ch/sch/sh -> 5, ss/sc -> s
            var step11 = new Dictionary<string, string>
            {
                ["ch"] = "5", ["sch"] = "5", ["sh"] = "5",
                ["ss"] = "s", ["sc"] = "s"
            };
            word = step11.Aggregate(word, (result, s) => result.Replace(s.Key, s.Value));

            // Step 12: c before e/i -> s
            foreach (int index in GetAllIndexesOfString(word, "c"))
            {
                if (index + 1 < word.Length && new[] { "e", "i" }.Contains(word.Substring(index + 1, 1)))
                {
                    word = word.Remove(index, 1);
                    word = word.Insert(index, "s");
                }
            }

            // Step 13: Hard consonant normalization -> k
            var step13 = new Dictionary<string, string>
            {
                ["c"] = "k", ["q"] = "k", ["qu"] = "k", ["gu"] = "k",
                ["ga"] = "ka", ["go"] = "ko", ["gy"] = "ky"
            };
            word = step13.Aggregate(word, (result, s) => result.Replace(s.Key, s.Value));

            // Step 14: Voiced/unvoiced consonant pairs
            var step14 = new Dictionary<string, string>
            {
                ["a"] = "o", ["d"] = "t", ["p"] = "t",
                ["j"] = "g", ["b"] = "f", ["v"] = "f", ["m"] = "n"
            };
            word = step14.Aggregate(word, (result, s) => result.Replace(s.Key, s.Value));

            // Step 15: Remove duplicate letters
            word = RemoveDuplicateLetters(word);

            // Step 16: Remove trailing x or t
            if (word.Length > 0)
            {
                char last = word[word.Length - 1];
                if (last == 'x' || last == 't')
                    word = word.Substring(0, word.Length - 1);
            }

            // Step 17: Map remaining characters to numeric values (base-22)
            var charToValue = new Dictionary<string, string>
            {
                ["1"] = "0",  ["2"] = "1",  ["3"] = "2",  ["4"] = "3",  ["5"] = "4",
                ["e"] = "5",  ["f"] = "6",  ["g"] = "7",  ["h"] = "8",  ["i"] = "9",
                ["k"] = "10", ["l"] = "11", ["n"] = "12", ["o"] = "13", ["r"] = "14",
                ["s"] = "15", ["t"] = "16", ["u"] = "17", ["w"] = "18", ["x"] = "19",
                ["y"] = "20", ["z"] = "21"
            };

            int[] code = new int[word.Length];
            for (int i = word.Length - 1; i >= 0; i--)
            {
                var keyValue = charToValue[word[i].ToString()];
                word = word.Replace(word[i].ToString(), keyValue);
                code[i] = int.Parse(keyValue);
            }

            // Step 18: Compute final value as base-22 fractional number
            double result = 0;
            int power = -1;
            foreach (var digit in code)
            {
                result += digit * Math.Pow(22, power);
                power--;
            }

            return result;
        }
    }
}
