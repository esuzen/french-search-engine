using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SearchModule.Demo.Pages
{
    public partial class Index
    {
        [Inject] private IJSRuntime JS { get; set; }

        private string searchQuery = "";
        private string lang = "fr";
        private string theme = "dark";
        private string activeTab = "people";
        private List<SearchResult<DemoItem>> results = new();
        private long elapsedMs = 0;

        // --- Localization ---

        private static readonly Dictionary<string, Dictionary<string, string>> i18n = new()
        {
            ["title"] = new() { ["fr"] = "Moteur de Recherche Francais", ["en"] = "French Search Engine" },
            ["subtitle"] = new() { ["fr"] = "Tapez un mot — meme avec des fautes ou en phonetique — et voyez les 5 algorithmes trouver ce que vous cherchez.", ["en"] = "Type a word — even with typos or phonetically — and watch 5 algorithms find what you're looking for." },
            ["results"] = new() { ["fr"] = "resultats", ["en"] = "results" },
            ["in"] = new() { ["fr"] = "sur", ["en"] = "in" },
            ["entries"] = new() { ["fr"] = "fiches", ["en"] = "entries" },
            ["no_results"] = new() { ["fr"] = "Aucun resultat pour", ["en"] = "No results for" },
            ["try_examples"] = new() { ["fr"] = "Essayez ces exemples", ["en"] = "Try these examples" },
            ["algo_exact"] = new() { ["fr"] = "Exact", ["en"] = "Exact" },
            ["algo_prefix"] = new() { ["fr"] = "Prefixe", ["en"] = "Prefix" },
            ["algo_substring"] = new() { ["fr"] = "Contenu", ["en"] = "Contains" },
            ["algo_similar"] = new() { ["fr"] = "Similaire", ["en"] = "Similar" },
            ["algo_phonetic"] = new() { ["fr"] = "Phonetique", ["en"] = "Phonetic" },
            ["footer_line1"] = new() { ["fr"] = "Cette demo tourne entierement dans votre navigateur grace a WebAssembly — aucun serveur, aucune donnee envoyee.", ["en"] = "This demo runs entirely in your browser via WebAssembly — no server, no data sent." },
            ["footer_line2"] = new() { ["fr"] = "Le moteur de recherche est une librairie .NET open-source specialisee dans la recherche floue et phonetique en francais.", ["en"] = "The search engine is an open-source .NET library specialized in fuzzy and phonetic French text search." },
            ["footer_line3"] = new() { ["fr"] = "Code source et documentation sur", ["en"] = "Source code and documentation on" },
        };

        private string L(string key) => i18n.ContainsKey(key) && i18n[key].ContainsKey(lang) ? i18n[key][lang] : key;

        private bool HasActiveCategory(int category) => results != null && results.Exists(r => r.resultCategory == category);

        // --- Dataset tabs ---

        private static readonly Dictionary<string, Dictionary<string, string>> datasetTabs = new()
        {
            ["people"] = new() { ["fr"] = "Personnes", ["en"] = "People" },
            ["elements"] = new() { ["fr"] = "Elements chimiques", ["en"] = "Chemical elements" },
            ["vaccines"] = new() { ["fr"] = "Vaccins & medicaments", ["en"] = "Vaccines & medicine" },
            ["cities"] = new() { ["fr"] = "Villes de France", ["en"] = "French cities" },
        };

        private static readonly string[] filters = new[] { "Name", "Detail", "Category" };

        // --- Examples per tab ---

        private static readonly Dictionary<string, string[]> examples = new()
        {
            ["people"] = new[] { "dupon", "farmacien", "filosof", "marseil", "boulangé", "medsin", "archi", "sophi" },
            ["elements"] = new[] { "idrogène", "hélium", "karbone", "oksygène", "azot", "soufr", "silicium", "magnez" },
            ["vaccines"] = new[] { "paracetamole", "amoksiciline", "ibuprofène", "dolipran", "vaksin", "pénicil", "aspiryne", "insulyne" },
            ["cities"] = new[] { "marseil", "toulous", "bordo", "strasbour", "monpelié", "nante", "ren", "lil" },
        };

        private string[] GetExamples() => examples.ContainsKey(activeTab) ? examples[activeTab] : examples["people"];

        private string GetPlaceholder() => activeTab switch
        {
            "people" => lang == "fr" ? "Cherchez un nom, une ville, un metier..." : "Search a name, city, profession...",
            "elements" => lang == "fr" ? "Cherchez un element chimique..." : "Search a chemical element...",
            "vaccines" => lang == "fr" ? "Cherchez un vaccin ou medicament..." : "Search a vaccine or medicine...",
            "cities" => lang == "fr" ? "Cherchez une ville de France..." : "Search a French city...",
            _ => ""
        };

        private string GetEmptyText() => activeTab switch
        {
            "people" => lang == "fr" ? "Tapez un nom, une ville ou un metier pour chercher parmi 90 fiches" : "Type a name, city, or profession to search across 90 entries",
            "elements" => lang == "fr" ? "Tapez un nom d'element pour chercher dans le tableau periodique" : "Type an element name to search the periodic table",
            "vaccines" => lang == "fr" ? "Tapez un nom de vaccin ou medicament" : "Type a vaccine or medicine name",
            "cities" => lang == "fr" ? "Tapez un nom de ville francaise" : "Type a French city name",
            _ => ""
        };

        // --- Actions ---

        private void SetLang(string newLang) { lang = newLang; }

        private async void SetTheme(string newTheme)
        {
            theme = newTheme;
            await JS.InvokeVoidAsync("setBodyTheme", theme);
        }

        private void SetTab(string tab)
        {
            activeTab = tab;
            searchQuery = "";
            results = new();
            elapsedMs = 0;
        }

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

            var data = GetActiveDataset();
            var sw = Stopwatch.StartNew();
            results = Search.SearchValues(searchQuery, filters, data);
            sw.Stop();
            elapsedMs = sw.ElapsedMilliseconds;
        }

        private List<DemoItem> GetActiveDataset() => activeTab switch
        {
            "people" => peopleData,
            "elements" => elementsData,
            "vaccines" => vaccinesData,
            "cities" => citiesData,
            _ => peopleData
        };

        // --- Display ---

        private string GetCategoryLabel(int category) => category switch
        {
            0 => L("algo_exact"),
            1 => L("algo_prefix"),
            2 => L("algo_substring"),
            3 => L("algo_similar"),
            4 => L("algo_phonetic"),
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

        private static string GetDisplayName(DemoItem item) => item.Name;
        private static string GetDisplayDetail(DemoItem item) =>
            string.IsNullOrEmpty(item.Category) ? item.Detail : $"{item.Detail} — {item.Category}";

        // =====================================================
        //                    DATASETS
        // =====================================================

        private static readonly List<DemoItem> peopleData = new()
        {
            new() { Name = "Jean Dupont", Detail = "Paris", Category = "Informaticien" },
            new() { Name = "Marie Martin", Detail = "Paris", Category = "Pharmacienne" },
            new() { Name = "Pierre Durand", Detail = "Paris", Category = "Architecte" },
            new() { Name = "Sophie Bernard", Detail = "Paris", Category = "Avocate" },
            new() { Name = "Nicolas Thomas", Detail = "Paris", Category = "Boulanger" },
            new() { Name = "Catherine Robert", Detail = "Paris", Category = "Medecin" },
            new() { Name = "Philippe Richard", Detail = "Paris", Category = "Philosophe" },
            new() { Name = "Isabelle Petit", Detail = "Paris", Category = "Journaliste" },
            new() { Name = "Laurent Moreau", Detail = "Paris", Category = "Ingenieur" },
            new() { Name = "Nathalie Simon", Detail = "Paris", Category = "Professeur" },
            new() { Name = "Francois Michel", Detail = "Paris", Category = "Comptable" },
            new() { Name = "Sylvie Leroy", Detail = "Paris", Category = "Dentiste" },
            new() { Name = "Christophe Roux", Detail = "Paris", Category = "Photographe" },
            new() { Name = "Veronique David", Detail = "Paris", Category = "Psychologue" },
            new() { Name = "Thierry Bertrand", Detail = "Paris", Category = "Plombier" },
            new() { Name = "Antoine Garcia", Detail = "Marseille", Category = "Pecheur" },
            new() { Name = "Camille Martinez", Detail = "Marseille", Category = "Restaurateur" },
            new() { Name = "Julien Lopez", Detail = "Marseille", Category = "Marin" },
            new() { Name = "Emilie Sanchez", Detail = "Marseille", Category = "Infirmiere" },
            new() { Name = "Mathieu Fernandez", Detail = "Marseille", Category = "Mecanicien" },
            new() { Name = "Aurelie Rossi", Detail = "Marseille", Category = "Coiffeuse" },
            new() { Name = "Sebastien Blanc", Detail = "Marseille", Category = "Electricien" },
            new() { Name = "Celine Faure", Detail = "Marseille", Category = "Sage-femme" },
            new() { Name = "Vincent Girard", Detail = "Marseille", Category = "Pharmacien" },
            new() { Name = "Sandrine Andre", Detail = "Marseille", Category = "Veterinaire" },
            new() { Name = "Maxime Lefebvre", Detail = "Lyon", Category = "Cuisinier" },
            new() { Name = "Charlotte Mercier", Detail = "Lyon", Category = "Designer" },
            new() { Name = "Alexandre Dupuis", Detail = "Lyon", Category = "Chirurgien" },
            new() { Name = "Pauline Lambert", Detail = "Lyon", Category = "Architecte" },
            new() { Name = "Benjamin Bonnet", Detail = "Lyon", Category = "Developpeur" },
            new() { Name = "Marine Francois", Detail = "Lyon", Category = "Traductrice" },
            new() { Name = "Romain Fontaine", Detail = "Lyon", Category = "Boucher" },
            new() { Name = "Laetitia Rousseau", Detail = "Lyon", Category = "Fleuriste" },
            new() { Name = "Guillaume Vincent", Detail = "Lyon", Category = "Notaire" },
            new() { Name = "Elodie Muller", Detail = "Lyon", Category = "Kinesitherapeute" },
            new() { Name = "Hugo Fournier", Detail = "Toulouse", Category = "Pilote" },
            new() { Name = "Manon Giraud", Detail = "Toulouse", Category = "Ergotherapeute" },
            new() { Name = "Clement Morel", Detail = "Toulouse", Category = "Ingenieur" },
            new() { Name = "Lea Gauthier", Detail = "Toulouse", Category = "Orthophoniste" },
            new() { Name = "Thomas Perrin", Detail = "Toulouse", Category = "Menuisier" },
            new() { Name = "Margaux Robin", Detail = "Toulouse", Category = "Sage-femme" },
            new() { Name = "Quentin Masson", Detail = "Toulouse", Category = "Informaticien" },
            new() { Name = "Justine Henry", Detail = "Toulouse", Category = "Biologiste" },
            new() { Name = "Adrien Chevalier", Detail = "Bordeaux", Category = "Viticulteur" },
            new() { Name = "Oceane Renard", Detail = "Bordeaux", Category = "Sommeliere" },
            new() { Name = "Florian Marchand", Detail = "Bordeaux", Category = "Oenologue" },
            new() { Name = "Amandine Picard", Detail = "Bordeaux", Category = "Enseignante" },
            new() { Name = "Damien Lemoine", Detail = "Bordeaux", Category = "Geologue" },
            new() { Name = "Melanie Carpentier", Detail = "Bordeaux", Category = "Libraire" },
            new() { Name = "Valentin Hubert", Detail = "Nantes", Category = "Osteopathe" },
            new() { Name = "Clara Dumas", Detail = "Nantes", Category = "Journaliste" },
            new() { Name = "Alexis Joly", Detail = "Nantes", Category = "Graphiste" },
            new() { Name = "Lucie Blanchard", Detail = "Nantes", Category = "Dieteticienne" },
            new() { Name = "Thibault Schneider", Detail = "Strasbourg", Category = "Brasseur" },
            new() { Name = "Helene Weber", Detail = "Strasbourg", Category = "Traductrice" },
            new() { Name = "Arnaud Klein", Detail = "Strasbourg", Category = "Patissier" },
            new() { Name = "Stephanie Fischer", Detail = "Strasbourg", Category = "Pharmacienne" },
            new() { Name = "Cedric Lemaire", Detail = "Lille", Category = "Brasseur" },
            new() { Name = "Audrey Lecomte", Detail = "Lille", Category = "Styliste" },
            new() { Name = "Mickael Deschamps", Detail = "Lille", Category = "Chocolatier" },
            new() { Name = "Virginie Collet", Detail = "Lille", Category = "Urbaniste" },
            new() { Name = "Fabien Olivier", Detail = "Nice", Category = "Moniteur" },
            new() { Name = "Delphine Philippe", Detail = "Nice", Category = "Galeriste" },
            new() { Name = "Yannick Perrot", Detail = "Nice", Category = "Plongeur" },
            new() { Name = "Caroline Renaud", Detail = "Nice", Category = "Decoratrice" },
            new() { Name = "Ludovic Caron", Detail = "Montpellier", Category = "Chercheur" },
            new() { Name = "Anais Maillard", Detail = "Montpellier", Category = "Archeologue" },
            new() { Name = "Kevin Guerin", Detail = "Montpellier", Category = "Sommelier" },
            new() { Name = "Elise Roger", Detail = "Montpellier", Category = "Podologue" },
            new() { Name = "Olivier Barbier", Detail = "Rennes", Category = "Informaticien" },
            new() { Name = "Marion Arnaud", Detail = "Rennes", Category = "Sociologue" },
            new() { Name = "Sylvain Leclerc", Detail = "Rennes", Category = "Charpentier" },
            new() { Name = "Gaelle Gaillard", Detail = "Rennes", Category = "Orthoptiste" },
            new() { Name = "Pascal Perrier", Detail = "Dijon", Category = "Moutardier" },
            new() { Name = "Agnes Brunet", Detail = "Dijon", Category = "Herboriste" },
            new() { Name = "Franck Noel", Detail = "Dijon", Category = "Fromager" },
            new() { Name = "Monique Legrand", Detail = "Dijon", Category = "Couturiere" },
            new() { Name = "Damien Rey", Detail = "Grenoble", Category = "Alpiniste" },
            new() { Name = "Chloe Mathieu", Detail = "Grenoble", Category = "Physicienne" },
            new() { Name = "Raphael Colin", Detail = "Grenoble", Category = "Ingenieur" },
            new() { Name = "Fanny Vidal", Detail = "Grenoble", Category = "Opticienne" },
            new() { Name = "Gregory Lemoine", Detail = "Toulon", Category = "Officier" },
            new() { Name = "Estelle Dupre", Detail = "Toulon", Category = "Navigatrice" },
            new() { Name = "Benoit Charles", Detail = "Toulon", Category = "Chaudronnier" },
            new() { Name = "Patricia Bourgeois", Detail = "Toulon", Category = "Comptable" },
        };

        private static readonly List<DemoItem> elementsData = new()
        {
            new() { Name = "Hydrogene", Detail = "H", Category = "Non-metal" },
            new() { Name = "Helium", Detail = "He", Category = "Gaz noble" },
            new() { Name = "Lithium", Detail = "Li", Category = "Metal alcalin" },
            new() { Name = "Beryllium", Detail = "Be", Category = "Metal alcalino-terreux" },
            new() { Name = "Bore", Detail = "B", Category = "Metalloide" },
            new() { Name = "Carbone", Detail = "C", Category = "Non-metal" },
            new() { Name = "Azote", Detail = "N", Category = "Non-metal" },
            new() { Name = "Oxygene", Detail = "O", Category = "Non-metal" },
            new() { Name = "Fluor", Detail = "F", Category = "Halogene" },
            new() { Name = "Neon", Detail = "Ne", Category = "Gaz noble" },
            new() { Name = "Sodium", Detail = "Na", Category = "Metal alcalin" },
            new() { Name = "Magnesium", Detail = "Mg", Category = "Metal alcalino-terreux" },
            new() { Name = "Aluminium", Detail = "Al", Category = "Metal pauvre" },
            new() { Name = "Silicium", Detail = "Si", Category = "Metalloide" },
            new() { Name = "Phosphore", Detail = "P", Category = "Non-metal" },
            new() { Name = "Soufre", Detail = "S", Category = "Non-metal" },
            new() { Name = "Chlore", Detail = "Cl", Category = "Halogene" },
            new() { Name = "Argon", Detail = "Ar", Category = "Gaz noble" },
            new() { Name = "Potassium", Detail = "K", Category = "Metal alcalin" },
            new() { Name = "Calcium", Detail = "Ca", Category = "Metal alcalino-terreux" },
            new() { Name = "Titane", Detail = "Ti", Category = "Metal de transition" },
            new() { Name = "Chrome", Detail = "Cr", Category = "Metal de transition" },
            new() { Name = "Manganese", Detail = "Mn", Category = "Metal de transition" },
            new() { Name = "Fer", Detail = "Fe", Category = "Metal de transition" },
            new() { Name = "Cobalt", Detail = "Co", Category = "Metal de transition" },
            new() { Name = "Nickel", Detail = "Ni", Category = "Metal de transition" },
            new() { Name = "Cuivre", Detail = "Cu", Category = "Metal de transition" },
            new() { Name = "Zinc", Detail = "Zn", Category = "Metal de transition" },
            new() { Name = "Arsenic", Detail = "As", Category = "Metalloide" },
            new() { Name = "Brome", Detail = "Br", Category = "Halogene" },
            new() { Name = "Krypton", Detail = "Kr", Category = "Gaz noble" },
            new() { Name = "Argent", Detail = "Ag", Category = "Metal de transition" },
            new() { Name = "Etain", Detail = "Sn", Category = "Metal pauvre" },
            new() { Name = "Iode", Detail = "I", Category = "Halogene" },
            new() { Name = "Xenon", Detail = "Xe", Category = "Gaz noble" },
            new() { Name = "Platine", Detail = "Pt", Category = "Metal de transition" },
            new() { Name = "Or", Detail = "Au", Category = "Metal de transition" },
            new() { Name = "Mercure", Detail = "Hg", Category = "Metal de transition" },
            new() { Name = "Plomb", Detail = "Pb", Category = "Metal pauvre" },
            new() { Name = "Uranium", Detail = "U", Category = "Actinide" },
        };

        private static readonly List<DemoItem> vaccinesData = new()
        {
            new() { Name = "Paracetamol", Detail = "Analgesique", Category = "Douleur" },
            new() { Name = "Ibuprofene", Detail = "Anti-inflammatoire", Category = "Douleur" },
            new() { Name = "Aspirine", Detail = "Analgesique", Category = "Douleur" },
            new() { Name = "Doliprane", Detail = "Paracetamol", Category = "Douleur" },
            new() { Name = "Amoxicilline", Detail = "Antibiotique", Category = "Infection" },
            new() { Name = "Penicilline", Detail = "Antibiotique", Category = "Infection" },
            new() { Name = "Azithromycine", Detail = "Antibiotique", Category = "Infection" },
            new() { Name = "Metformine", Detail = "Antidiabetique", Category = "Diabete" },
            new() { Name = "Insuline", Detail = "Hormone", Category = "Diabete" },
            new() { Name = "Omeprazole", Detail = "Inhibiteur pompe protons", Category = "Estomac" },
            new() { Name = "Levothyroxine", Detail = "Hormone thyroidienne", Category = "Thyroide" },
            new() { Name = "Atorvastatine", Detail = "Statine", Category = "Cholesterol" },
            new() { Name = "Simvastatine", Detail = "Statine", Category = "Cholesterol" },
            new() { Name = "Amlodipine", Detail = "Antihypertenseur", Category = "Tension" },
            new() { Name = "Lisinopril", Detail = "Inhibiteur ECA", Category = "Tension" },
            new() { Name = "Lorazepam", Detail = "Benzodiazepine", Category = "Anxiete" },
            new() { Name = "Diazepam", Detail = "Benzodiazepine", Category = "Anxiete" },
            new() { Name = "Sertraline", Detail = "Antidepresseur", Category = "Depression" },
            new() { Name = "Fluoxetine", Detail = "Antidepresseur", Category = "Depression" },
            new() { Name = "Vaccin BCG", Detail = "Bacille Calmette-Guerin", Category = "Vaccin" },
            new() { Name = "Vaccin ROR", Detail = "Rougeole Oreillons Rubeole", Category = "Vaccin" },
            new() { Name = "Vaccin DTP", Detail = "Diphterie Tetanos Polio", Category = "Vaccin" },
            new() { Name = "Vaccin Grippe", Detail = "Influenza saisonnier", Category = "Vaccin" },
            new() { Name = "Vaccin Hepatite B", Detail = "Virus Hepatite B", Category = "Vaccin" },
            new() { Name = "Vaccin Covid", Detail = "SARS-CoV-2", Category = "Vaccin" },
            new() { Name = "Vaccin Papillomavirus", Detail = "HPV", Category = "Vaccin" },
            new() { Name = "Vaccin Pneumocoque", Detail = "Streptococcus pneumoniae", Category = "Vaccin" },
            new() { Name = "Cortisone", Detail = "Corticosteroide", Category = "Inflammation" },
            new() { Name = "Prednisolone", Detail = "Corticosteroide", Category = "Inflammation" },
            new() { Name = "Morphine", Detail = "Opoide", Category = "Douleur severe" },
            new() { Name = "Codeine", Detail = "Opoide", Category = "Douleur severe" },
            new() { Name = "Ventoline", Detail = "Bronchodilatateur", Category = "Asthme" },
            new() { Name = "Salbutamol", Detail = "Bronchodilatateur", Category = "Asthme" },
        };

        private static readonly List<DemoItem> citiesData = new()
        {
            new() { Name = "Paris", Detail = "Ile-de-France", Category = "2 100 000 hab." },
            new() { Name = "Marseille", Detail = "Provence-Alpes-Cote d'Azur", Category = "870 000 hab." },
            new() { Name = "Lyon", Detail = "Auvergne-Rhone-Alpes", Category = "520 000 hab." },
            new() { Name = "Toulouse", Detail = "Occitanie", Category = "490 000 hab." },
            new() { Name = "Nice", Detail = "Provence-Alpes-Cote d'Azur", Category = "340 000 hab." },
            new() { Name = "Nantes", Detail = "Pays de la Loire", Category = "320 000 hab." },
            new() { Name = "Montpellier", Detail = "Occitanie", Category = "300 000 hab." },
            new() { Name = "Strasbourg", Detail = "Grand Est", Category = "290 000 hab." },
            new() { Name = "Bordeaux", Detail = "Nouvelle-Aquitaine", Category = "260 000 hab." },
            new() { Name = "Lille", Detail = "Hauts-de-France", Category = "235 000 hab." },
            new() { Name = "Rennes", Detail = "Bretagne", Category = "220 000 hab." },
            new() { Name = "Reims", Detail = "Grand Est", Category = "185 000 hab." },
            new() { Name = "Saint-Etienne", Detail = "Auvergne-Rhone-Alpes", Category = "175 000 hab." },
            new() { Name = "Toulon", Detail = "Provence-Alpes-Cote d'Azur", Category = "175 000 hab." },
            new() { Name = "Le Havre", Detail = "Normandie", Category = "170 000 hab." },
            new() { Name = "Grenoble", Detail = "Auvergne-Rhone-Alpes", Category = "160 000 hab." },
            new() { Name = "Dijon", Detail = "Bourgogne-Franche-Comte", Category = "160 000 hab." },
            new() { Name = "Angers", Detail = "Pays de la Loire", Category = "155 000 hab." },
            new() { Name = "Nimes", Detail = "Occitanie", Category = "150 000 hab." },
            new() { Name = "Clermont-Ferrand", Detail = "Auvergne-Rhone-Alpes", Category = "147 000 hab." },
            new() { Name = "Le Mans", Detail = "Pays de la Loire", Category = "145 000 hab." },
            new() { Name = "Aix-en-Provence", Detail = "Provence-Alpes-Cote d'Azur", Category = "145 000 hab." },
            new() { Name = "Brest", Detail = "Bretagne", Category = "140 000 hab." },
            new() { Name = "Tours", Detail = "Centre-Val de Loire", Category = "138 000 hab." },
            new() { Name = "Amiens", Detail = "Hauts-de-France", Category = "135 000 hab." },
            new() { Name = "Limoges", Detail = "Nouvelle-Aquitaine", Category = "132 000 hab." },
            new() { Name = "Perpignan", Detail = "Occitanie", Category = "120 000 hab." },
            new() { Name = "Besancon", Detail = "Bourgogne-Franche-Comte", Category = "118 000 hab." },
            new() { Name = "Orleans", Detail = "Centre-Val de Loire", Category = "116 000 hab." },
            new() { Name = "Metz", Detail = "Grand Est", Category = "115 000 hab." },
            new() { Name = "Rouen", Detail = "Normandie", Category = "113 000 hab." },
            new() { Name = "Mulhouse", Detail = "Grand Est", Category = "110 000 hab." },
            new() { Name = "Caen", Detail = "Normandie", Category = "107 000 hab." },
            new() { Name = "Nancy", Detail = "Grand Est", Category = "105 000 hab." },
            new() { Name = "Avignon", Detail = "Provence-Alpes-Cote d'Azur", Category = "92 000 hab." },
            new() { Name = "Poitiers", Detail = "Nouvelle-Aquitaine", Category = "90 000 hab." },
            new() { Name = "La Rochelle", Detail = "Nouvelle-Aquitaine", Category = "80 000 hab." },
            new() { Name = "Pau", Detail = "Nouvelle-Aquitaine", Category = "78 000 hab." },
            new() { Name = "Calais", Detail = "Hauts-de-France", Category = "73 000 hab." },
            new() { Name = "Ajaccio", Detail = "Corse", Category = "72 000 hab." },
        };
    }

    public class DemoItem
    {
        public string Name { get; set; } = "";
        public string Detail { get; set; } = "";
        public string Category { get; set; } = "";
    }
}
