using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SearchModule.Tests
{
    public class DemoDebugItem
    {
        public string Name { get; set; } = "";
        public string Detail { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class FalsePositiveRegressionTests
    {
        private readonly ITestOutputHelper output;
        public FalsePositiveRegressionTests(ITestOutputHelper output) { this.output = output; }

        private static readonly string[] filters = { "Name", "Detail", "Category" };

        private void AssertFirstResult(string query, List<DemoDebugItem> data, string expectedInName, string label)
        {
            var results = Search.SearchValues(query, filters, data);

            output.WriteLine($"'{query}' -> {results.Count} results:");
            foreach (var r in results)
            {
                string cat = r.resultCategory switch { 0 => "exact", 1 => "prefix", 2 => "substring", 3 => "similar", 4 => "phonetic", _ => "?" };
                output.WriteLine($"  {r.objectValue.Name} | {r.objectValue.Detail} | {r.objectValue.Category} — {r.percentageSimilarity:F1}% [{cat}]");
            }

            Assert.NotEmpty(results);
            Assert.True(
                results[0].objectValue.Name.Contains(expectedInName) ||
                results[0].objectValue.Detail.Contains(expectedInName) ||
                results[0].objectValue.Category.Contains(expectedInName),
                $"Expected first result to contain '{expectedInName}' but got: {results[0].objectValue.Name} / {results[0].objectValue.Detail} / {results[0].objectValue.Category} ({label})");

            // No false positive should appear before the real match
            if (results.Count > 1)
            {
                Assert.True(results[0].resultCategory <= results[1].resultCategory,
                    $"First result category ({results[0].resultCategory}) should be <= second ({results[1].resultCategory})");
            }
        }

        [Fact]
        public void Boulange_FindsBoulanger_NotBoucher()
        {
            var data = new List<DemoDebugItem>
            {
                new() { Name = "Nicolas Thomas", Detail = "Paris", Category = "Boulanger" },
                new() { Name = "Romain Fontaine", Detail = "Lyon", Category = "Boucher" },
            };
            AssertFirstResult("boulangé", data, "Boulanger", "boulangé should find Boulanger");
        }

        [Fact]
        public void Medsin_FindsMedecin_NotMenuisier()
        {
            var data = new List<DemoDebugItem>
            {
                new() { Name = "Catherine Robert", Detail = "Paris", Category = "Medecin" },
                new() { Name = "Thomas Perrin", Detail = "Toulouse", Category = "Menuisier" },
            };
            AssertFirstResult("medsin", data, "Medecin", "medsin should find Medecin");
        }

        [Fact]
        public void Archi_FindsArchitecte_NotArcheologue()
        {
            var data = new List<DemoDebugItem>
            {
                new() { Name = "Pierre Durand", Detail = "Paris", Category = "Architecte" },
                new() { Name = "Anais Maillard", Detail = "Montpellier", Category = "Archeologue" },
            };
            var results = Search.SearchValues("archi", filters, data);

            output.WriteLine($"'archi' -> {results.Count} results:");
            foreach (var r in results)
            {
                string cat = r.resultCategory switch { 0 => "exact", 1 => "prefix", 2 => "substring", 3 => "similar", 4 => "phonetic", _ => "?" };
                output.WriteLine($"  {r.objectValue.Name} | {r.objectValue.Category} — {r.percentageSimilarity:F1}% [{cat}]");
            }

            Assert.NotEmpty(results);
            // Both are valid matches (archi is prefix of both architecte and archeologue)
            // But architecte should rank first (higher similarity)
            Assert.Equal("Architecte", results[0].objectValue.Category);
        }

        [Fact]
        public void Amoksiciline_FindsAmoxicilline_NotParacetamol()
        {
            var data = new List<DemoDebugItem>
            {
                new() { Name = "Amoxicilline", Detail = "Antibiotique", Category = "Infection" },
                new() { Name = "Paracetamol", Detail = "Analgesique", Category = "Douleur" },
                new() { Name = "Omeprazole", Detail = "Inhibiteur pompe protons", Category = "Estomac" },
            };
            AssertFirstResult("amoksiciline", data, "Amoxicilline", "amoksiciline should find Amoxicilline");
        }

        [Fact]
        public void Marseil_FindsMarseille_NotLeHavre()
        {
            var data = new List<DemoDebugItem>
            {
                new() { Name = "Marseille", Detail = "Provence-Alpes-Cote d'Azur", Category = "870 000 hab." },
                new() { Name = "Le Havre", Detail = "Normandie", Category = "170 000 hab." },
            };
            AssertFirstResult("marseil", data, "Marseille", "marseil should find Marseille");
        }

        [Fact]
        public void Aspiryne_FindsAspirine_NotVentoline()
        {
            var data = new List<DemoDebugItem>
            {
                new() { Name = "Aspirine", Detail = "Analgesique", Category = "Douleur" },
                new() { Name = "Ventoline", Detail = "Bronchodilatateur", Category = "Asthme" },
            };
            AssertFirstResult("aspiryne", data, "Aspirine", "aspiryne should find Aspirine");
        }

        [Fact]
        public void Monpelie_FindsMontpellier_NotNantes()
        {
            var data = new List<DemoDebugItem>
            {
                new() { Name = "Montpellier", Detail = "Occitanie", Category = "300 000 hab." },
                new() { Name = "Nantes", Detail = "Pays de la Loire", Category = "320 000 hab." },
                new() { Name = "Nancy", Detail = "Grand Est", Category = "105 000 hab." },
            };
            AssertFirstResult("monpelié", data, "Montpellier", "monpelié should find Montpellier");
        }
    }
}
