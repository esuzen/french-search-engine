using System;
using System.Linq;
using System.Reflection;
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
        public double SimilarityTolerance { get; set; } = 68;

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
            double soundexTolerance = options.SoundexTolerance;
            double similarityTolerance = options.SimilarityTolerance;

            var dataTrie = new Trie();
            var resultsList = new List<SearchResult<T>>();

            // Reverse lookup: word -> object (O(1) instead of scanning)
            var wordToObject = new Dictionary<string, T>();
            // Forward lookup: object -> words (for trie removal)
            var objectToWords = new Dictionary<T, List<string>>();
            // Pre-computed Soundex cache: word -> soundex value
            var soundexCache = new Dictionary<string, double>();

            // Normalize search input
            searchString = searchString.ToLower();
            searchString = RemoveAccentedLetters(searchString);
            double searchSoundex = Soundex.GetSoundex(searchString);

            // Pre-compute k/ch alternative for search string
            double? altSearchSoundex = null;
            if (searchString.Contains("k"))
                altSearchSoundex = Soundex.GetSoundex(searchString.Replace("k", "ch"));
            else if (searchString.Contains("ch"))
                altSearchSoundex = Soundex.GetSoundex(searchString.Replace("ch", "k"));

            // Cache PropertyInfo per filter (avoid repeated reflection)
            var type = typeof(T);
            var propertyCache = new Dictionary<string, PropertyInfo>(filters.Length);
            foreach (var col in filters)
            {
                var prop = type.GetProperty(col);
                if (prop != null)
                    propertyCache[col] = prop;
            }

            // Build trie, lookups, and soundex cache
            foreach (T value in listOfValues)
            {
                var words = new List<string>();

                foreach (var kvp in propertyCache)
                {
                    var rawValue = kvp.Value.GetValue(value, null);
                    if (rawValue == null) continue;

                    string columnValue = rawValue.ToString().ToLower().Trim();
                    columnValue = GetStringWithoutSpecialChar(columnValue);
                    if (string.IsNullOrWhiteSpace(columnValue)) continue;

                    dataTrie.AddWord(columnValue);
                    words.Add(columnValue);

                    // Reverse lookup (first object wins if duplicate values)
                    if (!wordToObject.ContainsKey(columnValue))
                        wordToObject[columnValue] = value;

                    // Pre-compute Soundex (once per unique word)
                    if (!soundexCache.ContainsKey(columnValue))
                    {
                        soundexCache[columnValue] = Soundex.GetSoundex(columnValue);

                        if (columnValue.Contains("ch"))
                            soundexCache[columnValue + ":alt"] = Soundex.GetSoundex(columnValue.Replace("ch", "k"));
                    }
                }

                if (words.Count > 0)
                    objectToWords[value] = words;
            }

            // Track already-matched objects to avoid duplicates
            var matchedObjects = new HashSet<T>();

            // Helper: add result if object not already matched
            bool TryAddResult(T obj, int category, double similarity)
            {
                if (obj == null || !matchedObjects.Add(obj)) return false;
                resultsList.Add(new SearchResult<T>()
                {
                    objectValue = obj,
                    resultCategory = category,
                    percentageSimilarity = similarity
                });
                return true;
            }

            // --- Search cascade ---

            // 1. Exact match
            if (dataTrie.HasWord(searchString))
            {
                T obj;
                if (wordToObject.TryGetValue(searchString, out obj))
                    TryAddResult(obj, (int)ResultCategories.Matching, 100);
            }

            // 2. Prefix match
            if (dataTrie.HasPrefix(searchString))
            {
                foreach (string word in dataTrie.GetWords(searchString))
                {
                    if (matchedObjects.Count >= maxResultCount) break;
                    T obj;
                    if (wordToObject.TryGetValue(word, out obj))
                        TryAddResult(obj, (int)ResultCategories.Prefix, CompareTwoWords(searchString, word));
                }
            }

            // 3. Substring match — get all words once, filter in-place
            if (matchedObjects.Count < maxResultCount)
            {
                var allWords = dataTrie.GetWords();
                foreach (string word in allWords)
                {
                    if (matchedObjects.Count >= maxResultCount) break;
                    if (!word.Contains(searchString)) continue;

                    T obj;
                    if (wordToObject.TryGetValue(word, out obj) && !matchedObjects.Contains(obj))
                        TryAddResult(obj, (int)ResultCategories.Substring, CompareTwoWords(searchString, word));
                }

                // 4. Phonetic + 5. Similarity — reuse same word list
                // Phonetic first (cached double comparison = O(1)) then Levenshtein (O(n*m))
                int searchLen = searchString.Length;

                if (matchedObjects.Count < maxResultCount)
                {
                    foreach (var word in allWords)
                    {
                        if (matchedObjects.Count >= maxResultCount) break;

                        T obj;
                        if (!wordToObject.TryGetValue(word, out obj) || matchedObjects.Contains(obj))
                            continue;

                        // 4a. Phonetic: cached Soundex comparison (very fast — just double comparisons)
                        double wordSoundex;
                        if (soundexCache.TryGetValue(word, out wordSoundex))
                        {
                            bool phoneticMatch = Math.Abs(searchSoundex - wordSoundex) <= soundexTolerance;

                            if (!phoneticMatch)
                            {
                                double altWordSoundex;
                                if (soundexCache.TryGetValue(word + ":alt", out altWordSoundex))
                                    phoneticMatch = Math.Abs(searchSoundex - altWordSoundex) <= soundexTolerance;
                            }

                            if (!phoneticMatch && altSearchSoundex.HasValue)
                                phoneticMatch = Math.Abs(altSearchSoundex.Value - wordSoundex) <= soundexTolerance;

                            if (phoneticMatch)
                            {
                                // Guard against Soundex false positives:
                                // reject if character overlap is too low
                                double sim = CompareTwoWords(searchString, word);
                                if (sim >= 30)
                                {
                                    TryAddResult(obj, (int)ResultCategories.Paronym, sim);
                                }
                                continue;
                            }
                        }

                        // 4b. Similarity: skip if length difference too large (can't match)
                        int wordLen = word.Length;
                        int lenDiff = Math.Abs(searchLen - wordLen);
                        if (lenDiff >= options.MaxLevenshteinDistance) continue;

                        int dist = LevenshteinDistance.Compute(searchString, word);
                        if (dist < options.MaxLevenshteinDistance)
                        {
                            double sim = CompareTwoWords(searchString, word);
                            if (sim > similarityTolerance)
                                TryAddResult(obj, (int)ResultCategories.Similar, sim);
                        }
                    }
                }
            }

            if (options.Diagnostic)
            {
                executionTimer.Stop();
                Debug.WriteLine("========== End of search ==========");
                Debug.WriteLine(" > Number of results: {0}", resultsList.Count);
                Debug.WriteLine(" > Search time: {0} ms", executionTimer.ElapsedMilliseconds);
            }

            resultsList.Sort((a, b) =>
            {
                int cmp = a.resultCategory.CompareTo(b.resultCategory);
                return cmp != 0 ? cmp : b.percentageSimilarity.CompareTo(a.percentageSimilarity);
            });

            if (resultsList.Count > maxResultCount)
                resultsList.RemoveRange(maxResultCount, resultsList.Count - maxResultCount);

            return resultsList;
        }
    }
}
