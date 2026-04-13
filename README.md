# SearchModule.French

A multi-algorithm text search engine for .NET, optimized for French language.

[![CI](https://github.com/esuzen/search-module/actions/workflows/ci.yml/badge.svg)](https://github.com/esuzen/search-module/actions/workflows/ci.yml)

## Features

SearchModule combines five search strategies, ranked by relevance:

| Priority | Strategy | Description |
|----------|----------|-------------|
| 1 | **Exact match** | The query matches a value perfectly |
| 2 | **Prefix** | The query is a prefix of one or more values |
| 3 | **Substring** | The query is contained within a value |
| 4 | **Similarity** | Levenshtein distance + character overlap scoring |
| 5 | **Phonetic** | French Soundex algorithm for pronunciation-based matching |

### French Soundex

The built-in Soundex implementation is specifically designed for French phonetics (18 transformation steps), handling:
- Nasal vowels (an, on, in, ain, ein...)
- Silent letters and liaisons
- French-specific digraphs (ch, ph, ou, oi...)

## Installation

```
dotnet add package SearchModule.French
```

Or search for `SearchModule.French` in the NuGet Package Manager.

## Quick Start

```csharp
using SearchModule;

// Search across any object's properties
var results = Search.SearchValues(
    searchString: "dupont",
    filters: new[] { "LastName", "FirstName" },
    listOfValues: personList
);

// Results are ranked by category (exact > prefix > substring > similar > phonetic)
foreach (var result in results)
{
    Console.WriteLine($"{result.objectValue} - {result.percentageSimilarity}%");
}
```

### Custom Options

```csharp
var options = new SearchOptions
{
    MaxResultCount = 10,            // Default: 20
    SimilarityTolerance = 85,       // Default: 79 (percentage)
    SoundexTolerance = 0.001,       // Default: 0.0001
    MaxLevenshteinDistance = 3,     // Default: 5
    Diagnostic = true               // Default: false (logs to Debug output)
};

var results = Search.SearchValues("dupont", filters, data, options);
```

### Using Soundex Directly

```csharp
using SearchModule.Algorithms;

double value1 = Soundex.GetSoundex("pharmacie");
double value2 = Soundex.GetSoundex("farmacie");
// value1 == value2 (phonetically identical in French)
```

## Architecture

```
SearchModule/
  Search.cs              - Main search engine (cascading 5-algorithm pipeline)
  SearchOptions.cs       - Configurable search parameters
  Algorithms/
    LevenshteinDistance.cs - Edit distance computation
    Soundex.cs            - French phonetic algorithm (18 steps)
  Objects/
    Trie.cs               - Prefix tree for efficient lookups
    TrieNode.cs           - Trie node implementation
  Utils/
    StringHelpers.cs      - String comparison & normalization utilities
```

## How it works

1. **Initialization**: Input values are normalized (lowercased, accents removed) and inserted into a Trie structure. An association dictionary maps each object to its searchable string values.

2. **Search cascade**: The engine runs five algorithms in sequence, removing matched objects from the Trie after each hit to avoid duplicates:
   - Exact match via Trie lookup
   - Prefix search via Trie traversal
   - Substring search via linear scan
   - Similarity search using Levenshtein distance + character overlap
   - Phonetic search using French Soundex value comparison

3. **Ranking**: Results are sorted by category priority, then by similarity percentage within each category.

## Compatibility

| Target | Supported |
|--------|-----------|
| .NET 8+ | Yes |
| .NET Framework 4.7.2+ | Yes |

## Running Tests

```bash
dotnet test SearchModule.Tests/SearchModule.Tests.csproj
```

77 tests covering Levenshtein, Soundex, Trie, StringHelpers, and end-to-end search scenarios. Tests run on both .NET 8 and .NET Framework 4.7.2.

## License

[MIT](LICENSE)
