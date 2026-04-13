using System.Collections.Generic;
using Xunit;

namespace SearchModule.Tests
{
    public class DemoDataItem
    {
        public string Name { get; set; } = "";
        public string Detail { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class DemoExamplesTests
    {
        private static readonly string[] filters = { "Name", "Detail", "Category" };

        // --- All clickable examples from the demo ---

        [Theory]
        [InlineData("dupon")]
        [InlineData("farmacien")]
        [InlineData("filosof")]
        [InlineData("marseil")]
        [InlineData("boulangé")]
        [InlineData("medsin")]
        [InlineData("archi")]
        [InlineData("sophi")]
        public void PeopleExamples_ReturnResults(string query)
        {
            var data = PeopleData();
            var results = Search.SearchValues(query, filters, data);
            Assert.NotEmpty(results);
        }

        [Theory]
        [InlineData("idrogène")]
        [InlineData("hélium")]
        [InlineData("karbone")]
        [InlineData("oksygène")]
        [InlineData("azot")]
        [InlineData("soufr")]
        [InlineData("silicium")]
        [InlineData("magnez")]
        public void ElementExamples_ReturnResults(string query)
        {
            var data = ElementsData();
            var results = Search.SearchValues(query, filters, data);
            Assert.NotEmpty(results);
        }

        [Theory]
        [InlineData("paracetamole")]
        [InlineData("amoksiciline")]
        [InlineData("ibuprofène")]
        [InlineData("dolipran")]
        [InlineData("vaksin")]
        [InlineData("pénicil")]
        [InlineData("aspiryne")]
        [InlineData("insulyne")]
        public void VaccineExamples_ReturnResults(string query)
        {
            var data = VaccinesData();
            var results = Search.SearchValues(query, filters, data);
            Assert.NotEmpty(results);
        }

        [Theory]
        [InlineData("marseil")]
        [InlineData("toulous")]
        [InlineData("bordo")]
        [InlineData("strasbour")]
        [InlineData("monpelié")]
        [InlineData("nante")]
        [InlineData("ren")]
        [InlineData("lil")]
        public void CityExamples_ReturnResults(string query)
        {
            var data = CitiesData();
            var results = Search.SearchValues(query, filters, data);
            Assert.NotEmpty(results);
        }

        // --- Minimal datasets matching the demo ---

        private static List<DemoDataItem> PeopleData() => new()
        {
            new() { Name = "Jean Dupont", Detail = "Paris", Category = "Informaticien" },
            new() { Name = "Marie Martin", Detail = "Paris", Category = "Pharmacienne" },
            new() { Name = "Sophie Bernard", Detail = "Paris", Category = "Avocate" },
            new() { Name = "Nicolas Thomas", Detail = "Paris", Category = "Boulanger" },
            new() { Name = "Catherine Robert", Detail = "Paris", Category = "Medecin" },
            new() { Name = "Philippe Richard", Detail = "Paris", Category = "Philosophe" },
            new() { Name = "Antoine Garcia", Detail = "Marseille", Category = "Pecheur" },
            new() { Name = "Pierre Durand", Detail = "Paris", Category = "Architecte" },
        };

        private static List<DemoDataItem> ElementsData() => new()
        {
            new() { Name = "Hydrogene", Detail = "H", Category = "Non-metal" },
            new() { Name = "Helium", Detail = "He", Category = "Gaz noble" },
            new() { Name = "Carbone", Detail = "C", Category = "Non-metal" },
            new() { Name = "Azote", Detail = "N", Category = "Non-metal" },
            new() { Name = "Oxygene", Detail = "O", Category = "Non-metal" },
            new() { Name = "Silicium", Detail = "Si", Category = "Metalloide" },
            new() { Name = "Soufre", Detail = "S", Category = "Non-metal" },
            new() { Name = "Magnesium", Detail = "Mg", Category = "Metal alcalino-terreux" },
        };

        private static List<DemoDataItem> VaccinesData() => new()
        {
            new() { Name = "Paracetamol", Detail = "Analgesique", Category = "Douleur" },
            new() { Name = "Amoxicilline", Detail = "Antibiotique", Category = "Infection" },
            new() { Name = "Ibuprofene", Detail = "Anti-inflammatoire", Category = "Douleur" },
            new() { Name = "Doliprane", Detail = "Paracetamol", Category = "Douleur" },
            new() { Name = "Vaccin BCG", Detail = "Bacille Calmette-Guerin", Category = "Vaccin" },
            new() { Name = "Penicilline", Detail = "Antibiotique", Category = "Infection" },
            new() { Name = "Aspirine", Detail = "Analgesique", Category = "Douleur" },
            new() { Name = "Insuline", Detail = "Hormone", Category = "Diabete" },
        };

        private static List<DemoDataItem> CitiesData() => new()
        {
            new() { Name = "Marseille", Detail = "Provence-Alpes-Cote d'Azur", Category = "870 000 hab." },
            new() { Name = "Toulouse", Detail = "Occitanie", Category = "490 000 hab." },
            new() { Name = "Bordeaux", Detail = "Nouvelle-Aquitaine", Category = "260 000 hab." },
            new() { Name = "Strasbourg", Detail = "Grand Est", Category = "290 000 hab." },
            new() { Name = "Montpellier", Detail = "Occitanie", Category = "300 000 hab." },
            new() { Name = "Nantes", Detail = "Pays de la Loire", Category = "320 000 hab." },
            new() { Name = "Rennes", Detail = "Bretagne", Category = "220 000 hab." },
            new() { Name = "Lille", Detail = "Hauts-de-France", Category = "235 000 hab." },
        };
    }
}
