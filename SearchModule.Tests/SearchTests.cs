using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SearchModule.Tests
{
    public class TestPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class SearchTests
    {
        private static List<TestPerson> GetTestData()
        {
            return new List<TestPerson>
            {
                new TestPerson { FirstName = "Jean", LastName = "Dupont" },
                new TestPerson { FirstName = "Pierre", LastName = "Durand" },
                new TestPerson { FirstName = "Marie", LastName = "Martin" },
                new TestPerson { FirstName = "François", LastName = "Lefebvre" },
                new TestPerson { FirstName = "Philippe", LastName = "Bernard" },
                new TestPerson { FirstName = "Catherine", LastName = "Petit" },
                new TestPerson { FirstName = "Nicolas", LastName = "Robert" },
                new TestPerson { FirstName = "Sophie", LastName = "Richard" },
                new TestPerson { FirstName = "Laurent", LastName = "Moreau" },
                new TestPerson { FirstName = "Isabelle", LastName = "Simon" },
            };
        }

        private static string[] Filters => new[] { "FirstName", "LastName" };

        [Fact]
        public void SearchValues_ExactMatch_ReturnsFirstResult()
        {
            var results = Search.SearchValues("dupont", Filters, GetTestData());

            Assert.NotEmpty(results);
            Assert.Equal("Dupont", results[0].objectValue.LastName);
            Assert.Equal(100, results[0].percentageSimilarity);
        }

        [Fact]
        public void SearchValues_PrefixMatch_ReturnsResults()
        {
            var results = Search.SearchValues("dup", Filters, GetTestData());

            Assert.NotEmpty(results);
            // Should find Dupont and Durand (both start with "du", Dupont starts with "dup")
            Assert.Contains(results, r => r.objectValue.LastName == "Dupont");
        }

        [Fact]
        public void SearchValues_CaseInsensitive()
        {
            var results = Search.SearchValues("DUPONT", Filters, GetTestData());

            Assert.NotEmpty(results);
            Assert.Equal("Dupont", results[0].objectValue.LastName);
        }

        [Fact]
        public void SearchValues_AccentInsensitive()
        {
            var results = Search.SearchValues("françois", Filters, GetTestData());

            Assert.NotEmpty(results);
            Assert.Equal("François", results[0].objectValue.FirstName);
        }

        [Fact]
        public void SearchValues_EmptyList_ReturnsEmpty()
        {
            var results = Search.SearchValues("test", Filters, new List<TestPerson>());
            Assert.Empty(results);
        }

        [Fact]
        public void SearchValues_NoMatch_ReturnsEmpty()
        {
            var results = Search.SearchValues("zzzzzzzzz", Filters, GetTestData());
            Assert.Empty(results);
        }

        [Fact]
        public void SearchValues_ResultsAreSortedByCategoryThenSimilarity()
        {
            var results = Search.SearchValues("mar", Filters, GetTestData());

            // Verify ordering: lower category = better match
            for (int i = 1; i < results.Count; i++)
            {
                if (results[i].resultCategory == results[i - 1].resultCategory)
                {
                    Assert.True(results[i].percentageSimilarity <= results[i - 1].percentageSimilarity,
                        "Results within same category should be sorted by descending similarity");
                }
                else
                {
                    Assert.True(results[i].resultCategory >= results[i - 1].resultCategory,
                        "Results should be sorted by ascending category");
                }
            }
        }

        [Fact]
        public void SearchValues_RespectsMaxResultCount()
        {
            var options = new SearchOptions { MaxResultCount = 2 };
            var results = Search.SearchValues("a", Filters, GetTestData(), options);

            Assert.True(results.Count <= 2);
        }

        [Fact]
        public void SearchValues_SubstringMatch()
        {
            var data = new List<TestPerson>
            {
                new TestPerson { FirstName = "Test", LastName = "Delaporte" },
            };

            var results = Search.SearchValues("port", Filters, data);

            Assert.NotEmpty(results);
            Assert.Equal("Delaporte", results[0].objectValue.LastName);
        }

        [Fact]
        public void SearchValues_CustomOptions_StricterSimilarity()
        {
            var looseOptions = new SearchOptions { SimilarityTolerance = 50 };
            var strictOptions = new SearchOptions { SimilarityTolerance = 95 };

            var looseResults = Search.SearchValues("dupon", Filters, GetTestData(), looseOptions);
            var strictResults = Search.SearchValues("dupon", Filters, GetTestData(), strictOptions);

            Assert.True(looseResults.Count >= strictResults.Count,
                "Looser tolerance should return at least as many results");
        }

        [Fact]
        public void SearchValues_WithDiagnostic_DoesNotThrow()
        {
            var options = new SearchOptions { Diagnostic = true };
            var results = Search.SearchValues("dupont", Filters, GetTestData(), options);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void SearchValues_InvalidFilter_DoesNotThrow()
        {
            var filters = new[] { "NonExistentProperty" };
            var results = Search.SearchValues("test", filters, GetTestData());
            Assert.Empty(results);
        }

        [Fact]
        public void SearchValues_SimilarMatch_FindsCloseWords()
        {
            var results = Search.SearchValues("dupon", Filters, GetTestData());

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.objectValue.LastName == "Dupont");
        }
    }
}
