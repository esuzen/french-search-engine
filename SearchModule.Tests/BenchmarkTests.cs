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

        private List<BenchmarkItem> GenerateData(int count)
        {
            var rng = new Random(42); // Fixed seed for reproducibility
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

        private static readonly string[] filters = { "Name", "City", "Job" };
        private static readonly string[] queries = { "dupon", "marseil", "farmacien", "filosof", "klor", "informat", "boul" };

        [Theory]
        [InlineData(1000)]
        [InlineData(5000)]
        [InlineData(10000)]
        [InlineData(50000)]
        [InlineData(100000)]
        public void Benchmark_SearchPerformance(int dataSize)
        {
            var data = GenerateData(dataSize);

            // Warm up
            Search.SearchValues("test", filters, data);

            var sw = new Stopwatch();
            long totalMs = 0;
            int runs = queries.Length;

            foreach (var query in queries)
            {
                sw.Restart();
                var results = Search.SearchValues(query, filters, data);
                sw.Stop();
                totalMs += sw.ElapsedMilliseconds;
            }

            double avgMs = (double)totalMs / runs;
            output.WriteLine($"[{dataSize:N0} items] Total: {totalMs}ms | Avg per query: {avgMs:F1}ms ({runs} queries)");

            // Soft assertion: average should stay under 2 seconds per query
            Assert.True(avgMs < 2000, $"Average search time {avgMs:F1}ms exceeds 2000ms for {dataSize:N0} items");
        }
    }
}
