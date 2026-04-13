using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SearchModule.Tests
{
    public class ShortQueryTests
    {
        private readonly ITestOutputHelper output;
        public ShortQueryTests(ITestOutputHelper output) { this.output = output; }

        private static readonly string[] filters = { "Name", "Detail", "Category" };

        private void DumpResults(string query, List<SearchResult<DemoDebugItem>> results)
        {
            output.WriteLine($"'{query}' -> {results.Count} results:");
            foreach (var r in results)
            {
                string cat = r.resultCategory switch { 0 => "exact", 1 => "prefix", 2 => "substring", 3 => "similar", 4 => "phonetic", _ => "?" };
                output.WriteLine($"  {r.objectValue.Name} | {r.objectValue.Detail} — {r.percentageSimilarity:F1}% [{cat}]");
            }
        }

        private static List<DemoDebugItem> Cities() => new()
        {
            new() { Name = "Paris", Detail = "Ile-de-France", Category = "2 100 000 hab." },
            new() { Name = "Marseille", Detail = "Provence-Alpes-Cote d'Azur", Category = "870 000 hab." },
            new() { Name = "Toulouse", Detail = "Occitanie", Category = "490 000 hab." },
            new() { Name = "Nantes", Detail = "Pays de la Loire", Category = "320 000 hab." },
            new() { Name = "Rennes", Detail = "Bretagne", Category = "220 000 hab." },
            new() { Name = "Toulon", Detail = "Provence-Alpes-Cote d'Azur", Category = "175 000 hab." },
            new() { Name = "Le Havre", Detail = "Normandie", Category = "170 000 hab." },
            new() { Name = "Grenoble", Detail = "Auvergne-Rhone-Alpes", Category = "160 000 hab." },
            new() { Name = "Caen", Detail = "Normandie", Category = "107 000 hab." },
            new() { Name = "Rouen", Detail = "Normandie", Category = "113 000 hab." },
            new() { Name = "Nancy", Detail = "Grand Est", Category = "105 000 hab." },
            new() { Name = "Lille", Detail = "Hauts-de-France", Category = "235 000 hab." },
        };

        [Fact]
        public void Marseil_OnlyMarseille()
        {
            var results = Search.SearchValues("marseil", filters, Cities());
            DumpResults("marseil", results);
            Assert.NotEmpty(results);
            Assert.Equal("Marseille", results[0].objectValue.Name);
            Assert.True(results.All(r => r.objectValue.Name == "Marseille"), "Only Marseille should match");
        }

        [Fact]
        public void Toulous_FindsToulouse_NotToulon()
        {
            var results = Search.SearchValues("toulous", filters, Cities());
            DumpResults("toulous", results);
            Assert.NotEmpty(results);
            Assert.Equal("Toulouse", results[0].objectValue.Name);
            // Toulon might appear but must be ranked after Toulouse
            if (results.Count > 1)
                Assert.True(results[0].percentageSimilarity > results[1].percentageSimilarity);
        }

        [Fact]
        public void Ren_FindsRennes_NotCaenRouenGrenoble()
        {
            var results = Search.SearchValues("ren", filters, Cities());
            DumpResults("ren", results);
            Assert.NotEmpty(results);
            Assert.Equal("Rennes", results[0].objectValue.Name);
            // Should NOT contain Caen, Rouen, or Grenoble
            var badMatches = results.Where(r =>
                r.objectValue.Name == "Caen" ||
                r.objectValue.Name == "Rouen" ||
                r.objectValue.Name == "Grenoble").ToList();
            Assert.Empty(badMatches);
        }

        [Fact]
        public void Lil_FindsLille()
        {
            var results = Search.SearchValues("lil", filters, Cities());
            DumpResults("lil", results);
            Assert.NotEmpty(results);
            Assert.Equal("Lille", results[0].objectValue.Name);
        }
    }
}
