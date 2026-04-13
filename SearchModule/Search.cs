using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using SearchModule.Objects;
using SearchModule.Algorithms;
using static SearchModule.Utils.StringHelpers;

namespace SearchModule
{
    /// <summary>
    /// Configuration options for the search engine.
    /// </summary>
    public class SearchOptions
    {
        /// <summary>Maximum number of results to return.</summary>
        public int MaxResultCount { get; set; } = 20;

        /// <summary>Tolerance for phonetic (Soundex) matching. Lower = stricter.</summary>
        public double SoundexTolerance { get; set; } = 0.0001;

        /// <summary>Minimum similarity percentage (0-100) for Levenshtein-based matching.</summary>
        public double SimilarityTolerance { get; set; } = 79;

        /// <summary>Maximum Levenshtein distance to consider two words similar.</summary>
        public int MaxLevenshteinDistance { get; set; } = 5;

        /// <summary>Enable diagnostic logging via Debug.WriteLine.</summary>
        public bool Diagnostic { get; set; } = false;
    }

    public class SearchResult<T>
    {
        public T objectValue;
        public int resultCategory;
        public double percentageSimilarity;
    }


    public class Search
    {
        private enum ResultCategories { Matching, Prefix, Substring, Similar, Paronym };

        /// <summary>
        /// Search with default options.
        /// </summary>
        public static List<SearchResult<T>> SearchValues<T>(string searchString, string[] filters, List<T> listOfValues)
        {
            return SearchValues(searchString, filters, listOfValues, new SearchOptions());
        }

        /// <summary>
        /// Search with custom options.
        /// </summary>
        public static List<SearchResult<T>> SearchValues<T>(string searchString, string[] filters, List<T> listOfValues, SearchOptions options)
        {
            Stopwatch executionTimer = new Stopwatch();
            if (options.Diagnostic)
            {
                executionTimer.Start();
                Debug.WriteLine("========== Search started ==========");
                Debug.WriteLine(" > Search string: " + searchString);
            }

            int maxResultCount = options.MaxResultCount;
            double soundexTolerence = options.SoundexTolerance;
            double similarityTolerence = options.SimilarityTolerance;

            var dataTrie = new Trie();
            var resultsList = new List<SearchResult<T>>();
            var associationList = new Dictionary<T, List<string>>();

            // Normalize input
            searchString = searchString.ToLower();
            searchString = RemoveAccentedLetters(searchString);
            double searchStringSoundex = Soundex.GetSoundex(searchString);

            // Build trie and association dictionary from object properties
            foreach (T value in listOfValues)
            {
                var tmp = new List<string>();

                foreach (string col in filters)
                {
                    try
                    {
                        string columnValue = value.GetType().GetProperty(col).GetValue(value, null).ToString().ToLower().Trim();
                        columnValue = GetStringWithoutSpecialChar(columnValue);
                        dataTrie.AddWord(columnValue);
                        tmp.Add(columnValue);
                    }
                    catch (Exception ex) when (ex is NullReferenceException || ex is ArgumentNullException)
                    {
                        // Property not found or null value for this filter — skip silently
                    }
                }

                if (tmp.Count > 0)
                    associationList.Add(value, tmp);
            }

            // --- Search cascade ---

            // 1. Exact match
            if (dataTrie.HasWord(searchString))
            {
                T associatedObject = GetAssociatedObjectByValue(searchString, associationList);
                SearchResult<T> result = new SearchResult<T>()
                {
                    objectValue = associatedObject,
                    percentageSimilarity = 100,
                    resultCategory = (int)ResultCategories.Matching
                };
                resultsList.Add(result);
                dataTrie = RemoveObjectFromTrie(dataTrie, associatedObject, associationList);
            }

            // 2. Prefix match
            if (dataTrie.HasPrefix(searchString))
            {
                foreach (string word in dataTrie.GetWords(searchString))
                {
                    T associatedObject = GetAssociatedObjectByValue(word, associationList);
                    SearchResult<T> result = new SearchResult<T>()
                    {
                        objectValue = associatedObject,
                        resultCategory = (int)ResultCategories.Prefix,
                        percentageSimilarity = CompareTwoWords(searchString, word)
                    };
                    resultsList.Add(result);
                    dataTrie = RemoveObjectFromTrie(dataTrie, associatedObject, associationList);

                }
            }

            // 3. Substring match
            if (resultsList.Count < maxResultCount)
            {
                foreach (string word in dataTrie.GetWords().Where(a => a.Contains(searchString)))
                {
                    T associatedObject = GetAssociatedObjectByValue(word, associationList);
                    SearchResult<T> result = new SearchResult<T>()
                    {
                        objectValue = associatedObject,
                        resultCategory = (int)ResultCategories.Substring,
                        percentageSimilarity = CompareTwoWords(searchString, word)
                    };
                    resultsList.Add(result);
                    dataTrie = RemoveObjectFromTrie(dataTrie, associatedObject, associationList);
                }
            }

            // 4. Similarity (Levenshtein) + 5. Phonetic (Soundex) match
            if (resultsList.Count < maxResultCount)
            {
                foreach (var word in dataTrie.GetWords())
                {
                    bool matched = false;

                    // Try similarity match first (close Levenshtein distance + high overlap)
                    int levenshteinDistance = LevenshteinDistance.Compute(searchString, word);
                    if (levenshteinDistance < options.MaxLevenshteinDistance)
                    {
                        double stringSimilarity = CompareTwoWords(searchString, word);
                        if (stringSimilarity > similarityTolerence)
                        {
                            T associatedObject = GetAssociatedObjectByValue(word, associationList);
                            resultsList.Add(new SearchResult<T>()
                            {
                                resultCategory = (int)ResultCategories.Similar,
                                objectValue = associatedObject,
                                percentageSimilarity = stringSimilarity
                            });
                            matched = true;
                        }
                    }

                    // Always try phonetic match if similarity didn't match
                    if (!matched)
                    {
                        bool areSimilar = false;
                        double wordSoundex = Soundex.GetSoundex(word);
                        double soundexGap = Math.Abs(searchStringSoundex - wordSoundex);

                        if (soundexGap <= soundexTolerence)
                        {
                            areSimilar = true;
                        }
                        else if (word.StartsWith("ch"))
                        {
                            double altWordSoundex = Soundex.GetSoundex(word.Replace("ch", "k"));
                            if (Math.Abs(searchStringSoundex - altWordSoundex) <= soundexTolerence)
                                areSimilar = true;
                        }

                        // Also try the reverse: if search starts with "ch" or "k", test alternative
                        if (!areSimilar && searchString.StartsWith("k"))
                        {
                            double altSearchSoundex = Soundex.GetSoundex(searchString.Replace("k", "ch"));
                            if (Math.Abs(altSearchSoundex - wordSoundex) <= soundexTolerence)
                                areSimilar = true;
                        }

                        if (areSimilar)
                        {
                            T associatedObject = GetAssociatedObjectByValue(word, associationList);
                            resultsList.Add(new SearchResult<T>()
                            {
                                objectValue = associatedObject,
                                resultCategory = (int)ResultCategories.Paronym,
                                percentageSimilarity = CompareTwoWords(searchString, word)
                            });
                        }
                    }
                }
            }

            if (options.Diagnostic)
            {
                executionTimer.Stop();
                Debug.WriteLine("========== End of search ==========");
                int numberOfResults = resultsList.Count();
                long totalExecutionTime = executionTimer.ElapsedMilliseconds;
                Debug.WriteLine(" > Number of results: {0}", numberOfResults);
                Debug.WriteLine(" > Search time: {0} ms", totalExecutionTime);
            }

            resultsList = resultsList.OrderBy(result => result.resultCategory).ThenByDescending(result => result.percentageSimilarity).ToList();
            return resultsList.Take(maxResultCount).ToList();

        }

        private static Trie RemoveObjectFromTrie<T>(Trie valueTrie, T objectToRemove, Dictionary<T, List<string>> associationList)
        {
            List<string> list2Del = associationList.Where(l => l.Key.Equals(objectToRemove)).Select(l => l.Value).FirstOrDefault();
            if (list2Del != null)
            {
                foreach (var e in list2Del)
                {
                    if (valueTrie.HasWord(e))
                        valueTrie.RemoveWord(e);
                }
            }
            return valueTrie;
        }

        private static T GetAssociatedObjectByValue<T>(string value, Dictionary<T, List<string>> associationList)
        {
            return associationList.Where(a => a.Value.Any(l => l == value)).Select(a => a.Key).FirstOrDefault();
        }

    }
}
