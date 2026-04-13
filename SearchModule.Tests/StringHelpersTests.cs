using Xunit;
using SearchModule.Utils;

namespace SearchModule.Tests
{
    public class StringHelpersTests
    {
        // --- CompareTwoWords ---

        [Fact]
        public void CompareTwoWords_IdenticalWords_Returns100()
        {
            double result = StringHelpers.CompareTwoWords("hello", "hello");
            Assert.Equal(100, result);
        }

        [Fact]
        public void CompareTwoWords_CompletelyDifferent_ReturnsLowScore()
        {
            double result = StringHelpers.CompareTwoWords("abc", "xyz");
            Assert.True(result < 30, $"Expected low score, got {result}");
        }

        [Fact]
        public void CompareTwoWords_CaseInsensitive()
        {
            double result = StringHelpers.CompareTwoWords("Hello", "hello");
            Assert.Equal(100, result);
        }

        [Fact]
        public void CompareTwoWords_SimilarWords_ReturnsHighScore()
        {
            double result = StringHelpers.CompareTwoWords("dupont", "dupond");
            Assert.True(result > 80, $"Expected high score for similar words, got {result}");
        }

        [Fact]
        public void CompareTwoWords_MultipleWords_HandledCorrectly()
        {
            double result = StringHelpers.CompareTwoWords("jean dupont", "jean dupond");
            Assert.True(result > 80, $"Expected high score for multi-word similarity, got {result}");
        }

        // --- GetStringWithoutSpecialChar ---

        [Fact]
        public void GetStringWithoutSpecialChar_RemovesAccents()
        {
            string result = StringHelpers.GetStringWithoutSpecialChar("éèêàâôçûî");
            Assert.Equal("eeeaaocui", result);
        }

        [Fact]
        public void GetStringWithoutSpecialChar_ReplacesDotsWithSpaces()
        {
            string result = StringHelpers.GetStringWithoutSpecialChar("hello.world");
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void GetStringWithoutSpecialChar_PlainTextUnchanged()
        {
            string result = StringHelpers.GetStringWithoutSpecialChar("hello");
            Assert.Equal("hello", result);
        }

        // --- RemoveAccentedLetters ---

        [Theory]
        [InlineData("café", "cafe")]
        [InlineData("résumé", "resume")]
        [InlineData("naïve", "naive")]
        [InlineData("Zürich", "Zurich")]
        [InlineData("François", "Francois")]
        public void RemoveAccentedLetters_RemovesAllAccents(string input, string expected)
        {
            Assert.Equal(expected, StringHelpers.RemoveAccentedLetters(input));
        }

        [Fact]
        public void RemoveAccentedLetters_NoAccents_ReturnsSame()
        {
            Assert.Equal("hello", StringHelpers.RemoveAccentedLetters("hello"));
        }

        // --- RemoveNonLetterChar ---

        [Fact]
        public void RemoveNonLetterChar_RemovesDigitsAndSymbols()
        {
            Assert.Equal("hll", StringHelpers.RemoveNonLetterChar("h3ll0!"));
        }

        [Fact]
        public void RemoveNonLetterChar_LettersOnly_ReturnsSame()
        {
            Assert.Equal("abc", StringHelpers.RemoveNonLetterChar("abc"));
        }

        // --- RemoveLineEndings ---

        [Fact]
        public void RemoveLineEndings_RemovesAllTypes()
        {
            string input = "line1\r\nline2\nline3\r";
            Assert.Equal("line1line2line3", StringHelpers.RemoveLineEndings(input));
        }

        [Fact]
        public void RemoveLineEndings_EmptyString_ReturnsEmpty()
        {
            Assert.Equal("", StringHelpers.RemoveLineEndings(""));
        }

        [Fact]
        public void RemoveLineEndings_Null_ReturnsNull()
        {
            Assert.Null(StringHelpers.RemoveLineEndings(null));
        }

        // --- RemoveDuplicateLetters ---

        [Fact]
        public void RemoveDuplicateLetters_RemovesDuplicates()
        {
            Assert.Equal("abc", StringHelpers.RemoveDuplicateLetters("aabbcc"));
        }

        [Fact]
        public void RemoveDuplicateLetters_NoDuplicates_ReturnsSame()
        {
            Assert.Equal("abc", StringHelpers.RemoveDuplicateLetters("abc"));
        }

        [Fact]
        public void RemoveDuplicateLetters_PreservesOrder()
        {
            Assert.Equal("bac", StringHelpers.RemoveDuplicateLetters("baac"));
        }

        // --- GetAllIndexesOfString ---

        [Fact]
        public void GetAllIndexesOfString_FindsAllOccurrences()
        {
            var indexes = StringHelpers.GetAllIndexesOfString("abcabc", "a");
            Assert.Equal(2, indexes.Count);
            Assert.Equal(0, indexes[0]);
            Assert.Equal(3, indexes[1]);
        }

        [Fact]
        public void GetAllIndexesOfString_NoMatch_ReturnsEmpty()
        {
            var indexes = StringHelpers.GetAllIndexesOfString("hello", "z");
            Assert.Empty(indexes);
        }

        // --- GetTotalRate ---

        [Fact]
        public void GetTotalRate_SameLength_ReturnsSimilarity()
        {
            double rate = StringHelpers.GetTotalRate("hello", "hello");
            Assert.Equal(100, rate);
        }

        [Fact]
        public void GetTotalRate_VeryDifferentLengths_ReturnsZero()
        {
            // word2 is way longer than 125% of word1
            double rate = StringHelpers.GetTotalRate("ab", "abcdefgh");
            Assert.Equal(0, rate);
        }

        [Fact]
        public void GetTotalRate_EmptyStrings_ReturnsZero()
        {
            Assert.Equal(0, StringHelpers.GetTotalRate("", ""));
        }
    }
}
