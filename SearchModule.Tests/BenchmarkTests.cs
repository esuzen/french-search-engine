using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace SearchModule.Tests
{
    public class BenchmarkItem
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Job { get; set; }
    }

    public class BenchmarkTests
    {
        private readonly ITestOutputHelper output;

        public BenchmarkTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        private static readonly string[] FirstNames = {
            "Jean", "Marie", "Pierre", "Sophie", "Nicolas", "Catherine", "Philippe", "Isabelle",
            "Laurent", "Nathalie", "Francois", "Sylvie", "Christophe", "Veronique", "Thierry",
            "Antoine", "Camille", "Julien", "Emilie", "Mathieu", "Aurelie", "Sebastien", "Celine",
            "Vincent", "Sandrine", "Maxime", "Charlotte", "Alexandre", "Pauline", "Benjamin",
            "Hugo", "Manon", "Clement", "Lea", "Thomas", "Margaux", "Quentin", "Justine",
            "Adrien", "Oceane", "Florian", "Amandine", "Damien", "Melanie", "Valentin", "Clara",
            "Raphael", "Fanny", "Gregory", "Estelle"
        };

        private static readonly string[] LastNames = {
            "Dupont", "Martin", "Durand", "Bernard", "Thomas", "Robert", "Richard", "Petit",
            "Moreau", "Simon", "Michel", "Leroy", "Roux", "David", "Bertrand", "Garcia",
            "Martinez", "Lopez", "Sanchez", "Fernandez", "Rossi", "Blanc", "Faure", "Girard",
            "Andre", "Lefebvre", "Mercier", "Dupuis", "Lambert", "Bonnet", "Fournier", "Giraud",
            "Morel", "Gauthier", "Perrin", "Robin", "Masson", "Henry", "Chevalier", "Renard",
            "Marchand", "Picard", "Lemoine", "Carpentier", "Hubert", "Dumas", "Joly", "Blanchard",
            "Schneider", "Weber"
        };

        private static readonly string[] Cities = {
            "Paris", "Marseille", "Lyon", "Toulouse", "Nice", "Nantes", "Montpellier", "Strasbourg",
            "Bordeaux", "Lille", "Rennes", "Reims", "Toulon", "Grenoble", "Dijon", "Angers",
            "Nimes", "Brest", "Tours", "Amiens"
        };

        private static readonly string[] Jobs = {
            "Informaticien", "Pharmacien", "Architecte", "Medecin", "Boulanger", "Philosophe",
            "Ingenieur", "Professeur", "Dentiste", "Photographe", "Plombier", "Cuisinier",
            "Chirurgien", "Developpeur", "Electricien", "Mecanicien", "Pilote", "Biologiste",
            "Comptable", "Journaliste"
        };

        private static readonly string[] filters = { "Name", "City", "Job" };

        private List<BenchmarkItem> GenerateData(int count)
        {
            var rng = new Random(42);
            var data = new List<BenchmarkItem>(count);
            for (int i = 0; i < count; i++)
            {
                data.Add(new BenchmarkItem
                {
                    Name = FirstNames[rng.Next(FirstNames.Length)] + " " + LastNames[rng.Next(LastNames.Length)],
                    City = Cities[rng.Next(Cities.Length)],
                    Job = Jobs[rng.Next(Jobs.Length)]
                });
            }
            return data;
        }

        // --- Realistic query categories ---

        // Best case: exact match or prefix — exits early
        private static readonly string[] bestCaseQueries = { "dupont", "paris", "lyon", "jean", "informaticien" };

        // Typical case: short fuzzy/prefix queries (what real users type)
        private static readonly string[] typicalQueries = { "dupon", "marseil", "informat", "boul", "toulous" };

        // Worst case: phonetic with no match or long gibberish — must scan everything
        private static readonly string[] worstCaseQueries = { "xyzqwrtu", "abcdefgh", "zzzzzzz", "qqqqqq", "wwwwww" };

        // Phonetic: French phonetic misspellings — triggers Soundex path
        private static readonly string[] phoneticQueries = { "farmacien", "filosof", "chirurgein", "elektricien", "fotografe" };

        // Short queries (2-3 chars): many prefix matches, lots of comparisons
        private static readonly string[] shortQueries = { "ma", "du", "pa", "le", "bo" };

        // Long queries: fewer matches but heavier per-word computation
        private static readonly string[] longQueries = { "kinesitherapeute", "informaticien", "electricien", "photographe", "developpeur" };

        [Theory]
        [InlineData(50000)]
        public void Benchmark_ByQueryType(int dataSize)
        {
            var data = GenerateData(dataSize);

            // Warm up
            Search.SearchValues("test", filters, data);

            output.WriteLine($"=== Benchmark on {dataSize:N0} items ===");
            output.WriteLine("");

            RunCategory("Best case (exact/prefix)", bestCaseQueries, data);
            RunCategory("Typical (short fuzzy)", typicalQueries, data);
            RunCategory("Phonetic (Soundex)", phoneticQueries, data);
            RunCategory("Short (2-3 chars)", shortQueries, data);
            RunCategory("Long words", longQueries, data);
            RunCategory("Worst case (no match)", worstCaseQueries, data);
        }

        private void RunCategory(string label, string[] queries, List<BenchmarkItem> data)
        {
            var sw = new Stopwatch();
            long totalMs = 0;
            long minMs = long.MaxValue;
            long maxMs = 0;

            foreach (var query in queries)
            {
                sw.Restart();
                var results = Search.SearchValues(query, filters, data);
                sw.Stop();
                long ms = sw.ElapsedMilliseconds;
                totalMs += ms;
                if (ms < minMs) minMs = ms;
                if (ms > maxMs) maxMs = ms;
            }

            double avgMs = (double)totalMs / queries.Length;
            output.WriteLine($"  {label,-30} avg: {avgMs,6:F0}ms | min: {minMs,5}ms | max: {maxMs,5}ms | [{string.Join(", ", queries)}]");
        }

        // Keep the scaling test too
        [Theory]
        [InlineData(1000)]
        [InlineData(5000)]
        [InlineData(10000)]
        [InlineData(50000)]
        [InlineData(100000)]
        public void Benchmark_Scaling(int dataSize)
        {
            var data = GenerateData(dataSize);

            // Mix of all query types for realistic average
            var allQueries = new List<string>();
            allQueries.AddRange(bestCaseQueries);
            allQueries.AddRange(typicalQueries);
            allQueries.AddRange(phoneticQueries);
            allQueries.AddRange(shortQueries);
            allQueries.AddRange(longQueries);
            allQueries.AddRange(worstCaseQueries);

            // Warm up
            Search.SearchValues("test", filters, data);

            var sw = new Stopwatch();
            long totalMs = 0;

            foreach (var query in allQueries)
            {
                sw.Restart();
                Search.SearchValues(query, filters, data);
                sw.Stop();
                totalMs += sw.ElapsedMilliseconds;
            }

            double avgMs = (double)totalMs / allQueries.Count;
            output.WriteLine($"[{dataSize,7:N0}] {allQueries.Count} queries | Total: {totalMs,5}ms | Avg: {avgMs,6:F1}ms");

            Assert.True(avgMs < 3000, $"Avg {avgMs:F1}ms exceeds 3s for {dataSize:N0} items");
        }
    }
}
