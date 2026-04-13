using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SearchModule.Utils
{
    public class StringHelpers
    {
        /// <summary>
        /// Returns the similarity percentage (0-100) between two strings.
        /// Compares word-by-word: counts shared letters, then finds the best
        /// rotational alignment to measure positional matches.
        /// </summary>
        public static double CompareTwoWords(string word1, string word2)
        {
            word1 = word1.ToLower().Trim();
            word2 = word2.ToLower().Trim();

            word1 = GetStringWithoutSpecialChar(word1);
            word2 = GetStringWithoutSpecialChar(word2);

            string[] word1Split = word1.Split(new Char[] { ' ' });
            string[] word2Split = word2.Split(new Char[] { ' ' });

            int minWordCount = Math.Min(word1Split.Length, word2Split.Length);

            double totalPercentage = 0;

            for (int k = 0; k < minWordCount; k++)
            {
                double len1 = word1Split[k].Length;
                double len2 = word2Split[k].Length;
                double minLen = Math.Min(len1, len2);

                // Count shared letters (order-independent)
                double sharedLetters = 0;
                string word2Remaining = word2Split[k];

                for (int i = 0; i < len1; i++)
                {
                    for (int j = 0; j < word2Remaining.Length; j++)
                    {
                        if (word1Split[k][i].Equals(word2Remaining[j]))
                        {
                            sharedLetters++;
                            word2Remaining = word2Remaining.Remove(j, 1);
                            break;
                        }
                    }
                }

                // Find best rotational alignment using modulo offset
                double bestPositionalMatch = 0;
                string w1 = word1Split[k];
                string w2 = word2Split[k];
                int w2Len = w2.Length;

                for (int offset = 0; offset < w2Len; offset++)
                {
                    double positionMatches = 0;
                    int compareLen = (int)minLen;

                    for (int i = 0; i < compareLen; i++)
                    {
                        int rotatedIndex = (i + offset + 1) % w2Len;
                        if (w1[i].Equals(w2[rotatedIndex]))
                        {
                            positionMatches++;
                        }
                        else if (i < compareLen - 1)
                        {
                            int nextRotated = (i + offset + 2) % w2Len;
                            if (w1[i].Equals(w2[nextRotated]) && w1[i + 1].Equals(w2[rotatedIndex]))
                            {
                                positionMatches++;
                            }
                        }
                    }

                    if (positionMatches > bestPositionalMatch)
                    {
                        bestPositionalMatch = positionMatches;
                    }
                }

                // Combine shared letters and positional matching
                double percentage = ((bestPositionalMatch + sharedLetters) / (len1 + len2)) * 100;

                // Adjust for word length (longer words get less boost)
                if (percentage < 100)
                {
                    percentage *= (1 + 1 / (len1 + len2));
                }

                totalPercentage += percentage;
            }

            double maxWordCount = Math.Max(word1Split.Length, word2Split.Length);
            return totalPercentage / maxWordCount;
        }

        /// <summary>
        /// Compares two words only if their lengths are within acceptable tolerance.
        /// Returns 0 if the length difference is too large.
        /// </summary>
        public static double GetTotalRate(string word1, string word2)
        {
            word1 = word1.Trim();
            word2 = word2.Trim();

            if (word1.Length == 0 || word2.Length == 0)
                return 0;

            double lengthRatio = word2.Length * 100.0 / word1.Length;

            if (word1.Length <= 2)
            {
                return lengthRatio == 100 ? CompareTwoWords(word1, word2) : 0;
            }
            else if (word1.Length == 3)
            {
                return (65 <= lengthRatio && lengthRatio <= 135) ? CompareTwoWords(word1, word2) : 0;
            }
            else
            {
                return (75 <= lengthRatio && lengthRatio <= 125) ? CompareTwoWords(word1, word2) : 0;
            }
        }

        /// <summary>
        /// Removes French accented characters and replaces dots with spaces.
        /// </summary>
        public static string GetStringWithoutSpecialChar(string word)
        {
            word = word.Replace("é", "e").Replace("è", "e").Replace("ê", "e");
            word = word.Replace("à", "a").Replace("â", "a");
            word = word.Replace("ô", "o");
            word = word.Replace("ç", "c");
            word = word.Replace("û", "u");
            word = word.Replace("î", "i");
            word = word.Replace(".", " ");
            return word;
        }

        /// <summary>
        /// Removes all non-letter characters from a string.
        /// </summary>
        public static string RemoveNonLetterChar(string value)
        {
            return Regex.Replace(value, "[^a-zA-Z]", "");
        }

        /// <summary>
        /// Removes all line ending characters from a string.
        /// </summary>
        public static string RemoveLineEndings(string value)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty)
                        .Replace(lineSeparator, string.Empty)
                        .Replace(paragraphSeparator, string.Empty);
        }

        /// <summary>
        /// Removes duplicate letters, keeping only the first occurrence of each character.
        /// </summary>
        public static string RemoveDuplicateLetters(string input)
        {
            var seen = new HashSet<char>();
            var result = new System.Text.StringBuilder(input.Length);

            foreach (char c in input)
            {
                if (seen.Add(c))
                    result.Append(c);
            }

            return result.ToString();
        }

        /// <summary>
        /// Replaces accented characters with their ASCII equivalents.
        /// Supports a wide range of Latin accented characters.
        /// </summary>
        public static string RemoveAccentedLetters(string input)
        {
            const string accented    = "ÀÁÂÃÄÅàáâãäåÒÓÔÕÖØòóôõöøÈÉÊËèéêëÌÍÎÏìíîïÙÚÛÜùúûüÿÑñÇç";
            const string replacement = "AAAAAAaaaaaaOOOOOOooooooEEEEeeeeIIIIiiiiUUUUuuuuyNnCc";

            for (int i = 0; i < accented.Length; i++)
            {
                input = input.Replace(accented[i], replacement[i]);
            }

            return input;
        }

        /// <summary>
        /// Returns all indexes where the given substring appears in the word.
        /// </summary>
        public static List<int> GetAllIndexesOfString(string word, string letterToFind)
        {
            var foundIndexes = new List<int>();

            for (int i = word.IndexOf(letterToFind); i > -1; i = word.IndexOf(letterToFind, i + 1))
            {
                foundIndexes.Add(i);
            }

            return foundIndexes;
        }
    }
}
