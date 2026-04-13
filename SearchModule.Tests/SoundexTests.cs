using Xunit;
using SearchModule.Algorithms;

namespace SearchModule.Tests
{
    public class SoundexTests
    {
        [Fact]
        public void GetSoundex_IdenticalWords_ReturnSameValue()
        {
            double s1 = Soundex.GetSoundex("dupont");
            double s2 = Soundex.GetSoundex("dupont");
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void GetSoundex_PhoneticEquivalents_ReturnSameValue()
        {
            // "pharmacie" and "farmacie" should sound the same in French
            double s1 = Soundex.GetSoundex("pharmacie");
            double s2 = Soundex.GetSoundex("farmacie");
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void GetSoundex_DifferentWords_ReturnDifferentValues()
        {
            double s1 = Soundex.GetSoundex("maison");
            double s2 = Soundex.GetSoundex("voiture");
            Assert.NotEqual(s1, s2);
        }

        [Fact]
        public void GetSoundex_ShortWord_ReturnsZero()
        {
            // Words shorter than 3 characters return 0
            Assert.Equal(0, Soundex.GetSoundex("ab"));
        }

        [Fact]
        public void GetSoundex_EmptyString_ReturnsZero()
        {
            Assert.Equal(0, Soundex.GetSoundex(""));
        }

        [Fact]
        public void GetSoundex_WhitespaceOnly_ReturnsZero()
        {
            Assert.Equal(0, Soundex.GetSoundex("   "));
        }

        [Fact]
        public void GetSoundex_CaseInsensitive()
        {
            double s1 = Soundex.GetSoundex("Dupont");
            double s2 = Soundex.GetSoundex("dupont");
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void GetSoundex_MultipleWords_CombinesValues()
        {
            double single = Soundex.GetSoundex("jean");
            double multi = Soundex.GetSoundex("jean pierre");
            Assert.NotEqual(single, multi);
        }

        [Fact]
        public void GetSoundex_AccentedLetters_Normalized()
        {
            double s1 = Soundex.GetSoundex("etude");
            double s2 = Soundex.GetSoundex("etude");
            Assert.Equal(s1, s2);
        }

        [Theory]
        [InlineData("dupont", "dupond")]  // t/d are mapped to same phonetic value
        [InlineData("philippe", "filipe")]  // ph -> f
        public void GetSoundex_FrenchPhoneticPairs_CloseValues(string word1, string word2)
        {
            double s1 = Soundex.GetSoundex(word1);
            double s2 = Soundex.GetSoundex(word2);
            double gap = System.Math.Abs(s1 - s2);
            Assert.True(gap < 0.01, $"Expected close Soundex values for '{word1}' and '{word2}', gap was {gap}");
        }

        [Fact]
        public void GetSoundex_IgnoresParenthesizedContent()
        {
            double s1 = Soundex.GetSoundex("dupont");
            double s2 = Soundex.GetSoundex("dupont (jean)");
            Assert.Equal(s1, s2);
        }
    }
}
