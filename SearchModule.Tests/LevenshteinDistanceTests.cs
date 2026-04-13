using Xunit;
using SearchModule.Algorithms;

namespace SearchModule.Tests
{
    public class LevenshteinDistanceTests
    {
        [Fact]
        public void Compute_IdenticalStrings_ReturnsZero()
        {
            Assert.Equal(0, LevenshteinDistance.Compute("hello", "hello"));
        }

        [Fact]
        public void Compute_EmptyFirstString_ReturnsLengthOfSecond()
        {
            Assert.Equal(5, LevenshteinDistance.Compute("", "hello"));
        }

        [Fact]
        public void Compute_EmptySecondString_ReturnsLengthOfFirst()
        {
            Assert.Equal(5, LevenshteinDistance.Compute("hello", ""));
        }

        [Fact]
        public void Compute_BothEmpty_ReturnsZero()
        {
            Assert.Equal(0, LevenshteinDistance.Compute("", ""));
        }

        [Fact]
        public void Compute_SingleInsertion_ReturnsOne()
        {
            Assert.Equal(1, LevenshteinDistance.Compute("chat", "chats"));
        }

        [Fact]
        public void Compute_SingleDeletion_ReturnsOne()
        {
            Assert.Equal(1, LevenshteinDistance.Compute("chats", "chat"));
        }

        [Fact]
        public void Compute_SingleSubstitution_ReturnsOne()
        {
            Assert.Equal(1, LevenshteinDistance.Compute("chat", "chot"));
        }

        [Theory]
        [InlineData("kitten", "sitting", 3)]
        [InlineData("pharmacie", "farmacie", 2)]
        [InlineData("dupont", "dupond", 1)]
        [InlineData("marseille", "marcelle", 2)]
        public void Compute_KnownDistances(string s, string t, int expected)
        {
            Assert.Equal(expected, LevenshteinDistance.Compute(s, t));
        }

        [Fact]
        public void Compute_IsSymmetric()
        {
            int d1 = LevenshteinDistance.Compute("abc", "def");
            int d2 = LevenshteinDistance.Compute("def", "abc");
            Assert.Equal(d1, d2);
        }
    }
}
