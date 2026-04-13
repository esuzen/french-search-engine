using System.Linq;
using Xunit;
using SearchModule.Objects;

namespace SearchModule.Tests
{
    public class TrieTests
    {
        [Fact]
        public void AddWord_And_HasWord()
        {
            var trie = new Trie();
            trie.AddWord("hello");
            Assert.True(trie.HasWord("hello"));
        }

        [Fact]
        public void HasWord_ReturnsFalse_WhenWordNotPresent()
        {
            var trie = new Trie();
            trie.AddWord("hello");
            Assert.False(trie.HasWord("world"));
        }

        [Fact]
        public void HasWord_ReturnsFalse_ForPrefix()
        {
            var trie = new Trie();
            trie.AddWord("hello");
            Assert.False(trie.HasWord("hel"));
        }

        [Fact]
        public void HasPrefix_ReturnsTrue()
        {
            var trie = new Trie();
            trie.AddWord("hello");
            Assert.True(trie.HasPrefix("hel"));
        }

        [Fact]
        public void HasPrefix_ReturnsFalse_WhenNoPrefixMatch()
        {
            var trie = new Trie();
            trie.AddWord("hello");
            Assert.False(trie.HasPrefix("xyz"));
        }

        [Fact]
        public void GetWords_ReturnsAllWords()
        {
            var trie = new Trie();
            trie.AddWord("chat");
            trie.AddWord("chien");
            trie.AddWord("cheval");

            var words = trie.GetWords();
            Assert.Equal(3, words.Count);
            Assert.Contains("chat", words);
            Assert.Contains("chien", words);
            Assert.Contains("cheval", words);
        }

        [Fact]
        public void GetWords_WithPrefix_ReturnsMatchingWords()
        {
            var trie = new Trie();
            trie.AddWord("chat");
            trie.AddWord("chien");
            trie.AddWord("maison");

            var words = trie.GetWords("ch");
            Assert.Equal(2, words.Count);
            Assert.Contains("chat", words);
            Assert.Contains("chien", words);
        }

        [Fact]
        public void RemoveWord_RemovesWordFromTrie()
        {
            var trie = new Trie();
            trie.AddWord("hello");
            trie.AddWord("help");

            trie.RemoveWord("hello");

            Assert.False(trie.HasWord("hello"));
            Assert.True(trie.HasWord("help"));
        }

        [Fact]
        public void RemovePrefix_RemovesAllWordsWithPrefix()
        {
            var trie = new Trie();
            trie.AddWord("chat");
            trie.AddWord("chien");
            trie.AddWord("maison");

            trie.RemovePrefix("ch");

            Assert.False(trie.HasWord("chat"));
            Assert.False(trie.HasWord("chien"));
            Assert.True(trie.HasWord("maison"));
        }

        [Fact]
        public void Clear_RemovesAllWords()
        {
            var trie = new Trie();
            trie.AddWord("one");
            trie.AddWord("two");
            trie.AddWord("three");

            trie.Clear();

            Assert.Empty(trie.GetWords());
        }

        [Fact]
        public void AddWord_DuplicateWord_IncreasesWordCount()
        {
            var trie = new Trie();
            trie.AddWord("hello");
            trie.AddWord("hello");

            Assert.True(trie.HasWord("hello"));
            // After removing once, word count decreases but still counts as removed
            trie.RemoveWord("hello");
            Assert.False(trie.HasWord("hello"));
        }

        [Fact]
        public void AddWord_NullThrowsException()
        {
            var trie = new Trie();
            Assert.Throws<System.ArgumentNullException>(() => trie.AddWord(null));
        }

        [Fact]
        public void GetWords_EmptyPrefix_ReturnsAllWords()
        {
            var trie = new Trie();
            trie.AddWord("abc");
            trie.AddWord("def");

            var all = trie.GetWords("");
            Assert.Equal(2, all.Count);
        }
    }
}
