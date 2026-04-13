using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;

namespace SearchModule.Demo.Pages
{
    public partial class Index
    {
        private string searchQuery = "";
        private List<SearchResult<Person>> results = new();
        private long elapsedMs = 0;

        private static readonly string[] exampleQueries = new[]
        {
            "farmaci",      // phonetic: pharmacien
            "dupon",        // similar: dupont
            "filosof",      // phonetic: philosophe
            "marseil",      // prefix: marseille
            "boulangé",     // accent: boulanger
            "medsin",       // phonetic: medecin
            "toulous",      // prefix: toulouse
            "informat",     // prefix: informaticien
            "sof",          // prefix: sophie
            "archi",        // prefix: architecte
        };

        private static readonly List<Person> dataset = BuildDataset();

        private static readonly string[] filters = new[] { "FirstName", "LastName", "City", "Profession" };

        private void OnSearch(ChangeEventArgs e)
        {
            searchQuery = e.Value?.ToString() ?? "";
            PerformSearch();
        }

        private void SetQuery(string query)
        {
            searchQuery = query;
            PerformSearch();
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(searchQuery) || searchQuery.Length < 2)
            {
                results = new();
                elapsedMs = 0;
                return;
            }

            var sw = Stopwatch.StartNew();
            results = Search.SearchValues(searchQuery, filters, dataset);
            sw.Stop();
            elapsedMs = sw.ElapsedMilliseconds;
        }

        private static string GetCategoryName(int category) => category switch
        {
            0 => "exact",
            1 => "prefix",
            2 => "substring",
            3 => "similar",
            4 => "phonetic",
            _ => "?"
        };

        private static string GetBadgeClass(int category) => category switch
        {
            0 => "badge-exact",
            1 => "badge-prefix",
            2 => "badge-substring",
            3 => "badge-similar",
            4 => "badge-phonetic",
            _ => ""
        };

        private static string GetBarColor(int category) => category switch
        {
            0 => "#22c55e",
            1 => "#3b82f6",
            2 => "#06b6d4",
            3 => "#f59e0b",
            4 => "#ec4899",
            _ => "#888"
        };

        private static string GetDisplayName(Person p) => $"{p.FirstName} {p.LastName}";
        private static string GetDisplayDetail(Person p) => $"{p.Profession} - {p.City}";

        private static List<Person> BuildDataset()
        {
            return new List<Person>
            {
                // Paris
                new() { FirstName = "Jean", LastName = "Dupont", City = "Paris", Profession = "Informaticien" },
                new() { FirstName = "Marie", LastName = "Martin", City = "Paris", Profession = "Pharmacienne" },
                new() { FirstName = "Pierre", LastName = "Durand", City = "Paris", Profession = "Architecte" },
                new() { FirstName = "Sophie", LastName = "Bernard", City = "Paris", Profession = "Avocate" },
                new() { FirstName = "Nicolas", LastName = "Thomas", City = "Paris", Profession = "Boulanger" },
                new() { FirstName = "Catherine", LastName = "Robert", City = "Paris", Profession = "Medecin" },
                new() { FirstName = "Philippe", LastName = "Richard", City = "Paris", Profession = "Philosophe" },
                new() { FirstName = "Isabelle", LastName = "Petit", City = "Paris", Profession = "Journaliste" },
                new() { FirstName = "Laurent", LastName = "Moreau", City = "Paris", Profession = "Ingenieur" },
                new() { FirstName = "Nathalie", LastName = "Simon", City = "Paris", Profession = "Professeur" },
                new() { FirstName = "Francois", LastName = "Michel", City = "Paris", Profession = "Comptable" },
                new() { FirstName = "Sylvie", LastName = "Leroy", City = "Paris", Profession = "Dentiste" },
                new() { FirstName = "Christophe", LastName = "Roux", City = "Paris", Profession = "Photographe" },
                new() { FirstName = "Veronique", LastName = "David", City = "Paris", Profession = "Psychologue" },
                new() { FirstName = "Thierry", LastName = "Bertrand", City = "Paris", Profession = "Plombier" },

                // Marseille
                new() { FirstName = "Antoine", LastName = "Garcia", City = "Marseille", Profession = "Pecheur" },
                new() { FirstName = "Camille", LastName = "Martinez", City = "Marseille", Profession = "Restaurateur" },
                new() { FirstName = "Julien", LastName = "Lopez", City = "Marseille", Profession = "Marin" },
                new() { FirstName = "Emilie", LastName = "Sanchez", City = "Marseille", Profession = "Infirmiere" },
                new() { FirstName = "Mathieu", LastName = "Fernandez", City = "Marseille", Profession = "Mecanicien" },
                new() { FirstName = "Aurelie", LastName = "Rossi", City = "Marseille", Profession = "Coiffeuse" },
                new() { FirstName = "Sebastien", LastName = "Blanc", City = "Marseille", Profession = "Electricien" },
                new() { FirstName = "Celine", LastName = "Faure", City = "Marseille", Profession = "Sage-femme" },
                new() { FirstName = "Vincent", LastName = "Girard", City = "Marseille", Profession = "Pharmacien" },
                new() { FirstName = "Sandrine", LastName = "Andre", City = "Marseille", Profession = "Veterinaire" },

                // Lyon
                new() { FirstName = "Maxime", LastName = "Lefebvre", City = "Lyon", Profession = "Cuisinier" },
                new() { FirstName = "Charlotte", LastName = "Mercier", City = "Lyon", Profession = "Designer" },
                new() { FirstName = "Alexandre", LastName = "Dupuis", City = "Lyon", Profession = "Chirurgien" },
                new() { FirstName = "Pauline", LastName = "Lambert", City = "Lyon", Profession = "Architecte" },
                new() { FirstName = "Benjamin", LastName = "Bonnet", City = "Lyon", Profession = "Developpeur" },
                new() { FirstName = "Marine", LastName = "Francois", City = "Lyon", Profession = "Traductrice" },
                new() { FirstName = "Romain", LastName = "Fontaine", City = "Lyon", Profession = "Boucher" },
                new() { FirstName = "Laetitia", LastName = "Rousseau", City = "Lyon", Profession = "Fleuriste" },
                new() { FirstName = "Guillaume", LastName = "Vincent", City = "Lyon", Profession = "Notaire" },
                new() { FirstName = "Elodie", LastName = "Muller", City = "Lyon", Profession = "Kinesitherapeute" },

                // Toulouse
                new() { FirstName = "Hugo", LastName = "Fournier", City = "Toulouse", Profession = "Pilote" },
                new() { FirstName = "Manon", LastName = "Giraud", City = "Toulouse", Profession = "Ergotherapeute" },
                new() { FirstName = "Clement", LastName = "Morel", City = "Toulouse", Profession = "Ingenieur" },
                new() { FirstName = "Lea", LastName = "Gauthier", City = "Toulouse", Profession = "Orthophoniste" },
                new() { FirstName = "Thomas", LastName = "Perrin", City = "Toulouse", Profession = "Menuisier" },
                new() { FirstName = "Margaux", LastName = "Robin", City = "Toulouse", Profession = "Sage-femme" },
                new() { FirstName = "Quentin", LastName = "Masson", City = "Toulouse", Profession = "Informaticien" },
                new() { FirstName = "Justine", LastName = "Henry", City = "Toulouse", Profession = "Biologiste" },

                // Bordeaux
                new() { FirstName = "Adrien", LastName = "Chevalier", City = "Bordeaux", Profession = "Viticulteur" },
                new() { FirstName = "Oceane", LastName = "Renard", City = "Bordeaux", Profession = "Sommeliere" },
                new() { FirstName = "Florian", LastName = "Marchand", City = "Bordeaux", Profession = "Oenologue" },
                new() { FirstName = "Amandine", LastName = "Picard", City = "Bordeaux", Profession = "Enseignante" },
                new() { FirstName = "Damien", LastName = "Lemoine", City = "Bordeaux", Profession = "Geologue" },
                new() { FirstName = "Melanie", LastName = "Carpentier", City = "Bordeaux", Profession = "Libraire" },

                // Nantes
                new() { FirstName = "Valentin", LastName = "Hubert", City = "Nantes", Profession = "Osteopathe" },
                new() { FirstName = "Clara", LastName = "Dumas", City = "Nantes", Profession = "Journaliste" },
                new() { FirstName = "Alexis", LastName = "Joly", City = "Nantes", Profession = "Graphiste" },
                new() { FirstName = "Lucie", LastName = "Blanchard", City = "Nantes", Profession = "Dieteticienne" },

                // Strasbourg
                new() { FirstName = "Thibault", LastName = "Schneider", City = "Strasbourg", Profession = "Brasseur" },
                new() { FirstName = "Helene", LastName = "Weber", City = "Strasbourg", Profession = "Traductrice" },
                new() { FirstName = "Arnaud", LastName = "Klein", City = "Strasbourg", Profession = "Patissier" },
                new() { FirstName = "Stephanie", LastName = "Fischer", City = "Strasbourg", Profession = "Pharmacienne" },

                // Lille
                new() { FirstName = "Cedric", LastName = "Lemaire", City = "Lille", Profession = "Brasseur" },
                new() { FirstName = "Audrey", LastName = "Lecomte", City = "Lille", Profession = "Styliste" },
                new() { FirstName = "Mickael", LastName = "Deschamps", City = "Lille", Profession = "Chocolatier" },
                new() { FirstName = "Virginie", LastName = "Collet", City = "Lille", Profession = "Urbaniste" },

                // Nice
                new() { FirstName = "Fabien", LastName = "Olivier", City = "Nice", Profession = "Moniteur" },
                new() { FirstName = "Delphine", LastName = "Philippe", City = "Nice", Profession = "Galeriste" },
                new() { FirstName = "Yannick", LastName = "Perrot", City = "Nice", Profession = "Plongeur" },
                new() { FirstName = "Caroline", LastName = "Renaud", City = "Nice", Profession = "Decoratrice" },

                // Montpellier
                new() { FirstName = "Ludovic", LastName = "Caron", City = "Montpellier", Profession = "Chercheur" },
                new() { FirstName = "Anais", LastName = "Maillard", City = "Montpellier", Profession = "Archeologue" },
                new() { FirstName = "Kevin", LastName = "Guerin", City = "Montpellier", Profession = "Sommelier" },
                new() { FirstName = "Elise", LastName = "Roger", City = "Montpellier", Profession = "Podologue" },

                // Rennes
                new() { FirstName = "Olivier", LastName = "Barbier", City = "Rennes", Profession = "Informaticien" },
                new() { FirstName = "Marion", LastName = "Arnaud", City = "Rennes", Profession = "Sociologue" },
                new() { FirstName = "Sylvain", LastName = "Leclerc", City = "Rennes", Profession = "Charpentier" },
                new() { FirstName = "Gaelle", LastName = "Gaillard", City = "Rennes", Profession = "Orthoptiste" },

                // Dijon
                new() { FirstName = "Pascal", LastName = "Perrier", City = "Dijon", Profession = "Moutardier" },
                new() { FirstName = "Agnes", LastName = "Brunet", City = "Dijon", Profession = "Herboriste" },
                new() { FirstName = "Franck", LastName = "Noel", City = "Dijon", Profession = "Fromager" },
                new() { FirstName = "Monique", LastName = "Legrand", City = "Dijon", Profession = "Couturiere" },

                // Grenoble
                new() { FirstName = "Damien", LastName = "Rey", City = "Grenoble", Profession = "Alpiniste" },
                new() { FirstName = "Chloe", LastName = "Mathieu", City = "Grenoble", Profession = "Physicienne" },
                new() { FirstName = "Raphael", LastName = "Colin", City = "Grenoble", Profession = "Ingenieur" },
                new() { FirstName = "Fanny", LastName = "Vidal", City = "Grenoble", Profession = "Opticienne" },

                // Toulon
                new() { FirstName = "Gregory", LastName = "Lemoine", City = "Toulon", Profession = "Officier" },
                new() { FirstName = "Estelle", LastName = "Dupre", City = "Toulon", Profession = "Navigatrice" },
                new() { FirstName = "Benoit", LastName = "Charles", City = "Toulon", Profession = "Chaudronnier" },
                new() { FirstName = "Patricia", LastName = "Bourgeois", City = "Toulon", Profession = "Comptable" },
            };
        }
    }

    public class Person
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string City { get; set; } = "";
        public string Profession { get; set; } = "";
    }
}
