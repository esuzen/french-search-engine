using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchModule.Objects
{
    /// <summary>
    /// Prefix tree (trie) data structure for efficient word storage and lookup.
    /// Supports add, remove, prefix search, and word enumeration.
    /// </summary>
    public class Trie
    {
        private TrieNode RootTrieNode { get; set; }

        public Trie()
        {
            RootTrieNode = new TrieNode(' ');
        }

        /// <summary>Adds a word to the trie.</summary>
        public void AddWord(string word)
        {
            if (word == null) throw new ArgumentNullException(nameof(word));
            AddWord(RootTrieNode, word.ToCharArray());
        }

        /// <summary>Removes a word from the trie. Returns the word count that was removed.</summary>
        public int RemoveWord(string word)
        {
            if (word == null) throw new ArgumentNullException(nameof(word));
            return RemoveWord(GetTrieNodesStack(word));
        }

        /// <summary>Removes all words starting with the given prefix.</summary>
        public void RemovePrefix(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            RemovePrefix(GetTrieNodesStack(prefix, false));
        }

        /// <summary>Returns all words in the trie.</summary>
        public ICollection<string> GetWords()
        {
            return GetWords("");
        }

        /// <summary>Returns all words starting with the given prefix.</summary>
        public ICollection<string> GetWords(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            var words = new List<string>();
            var buffer = new StringBuilder();
            buffer.Append(prefix);
            GetWords(GetTrieNode(prefix), words, buffer);
            return words;
        }

        /// <summary>Returns true if the exact word exists in the trie.</summary>
        public bool HasWord(string word)
        {
            if (word == null) throw new ArgumentNullException(nameof(word));
            var trieNode = GetTrieNode(word);
            return trieNode?.IsWord ?? false;
        }

        /// <summary>Returns true if any word in the trie starts with the given prefix.</summary>
        public bool HasPrefix(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            return GetTrieNode(prefix) != null;
        }

        /// <summary>Removes all words from the trie.</summary>
        public void Clear()
        {
            RootTrieNode.Clear();
        }

        private TrieNode GetTrieNode(string prefix)
        {
            var trieNode = RootTrieNode;
            foreach (var prefixChar in prefix)
            {
                trieNode = trieNode.GetChild(prefixChar);
                if (trieNode == null) break;
            }
            return trieNode;
        }

        private void AddWord(TrieNode trieNode, char[] word)
        {
            foreach (var c in word)
            {
                var child = trieNode.GetChild(c);
                if (child == null)
                {
                    child = new TrieNode(c);
                    trieNode.SetChild(child);
                }
                trieNode = child;
            }
            trieNode.WordCount++;
        }

        private void GetWords(TrieNode trieNode, ICollection<string> words, StringBuilder buffer)
        {
            if (trieNode == null) return;
            if (trieNode.IsWord) words.Add(buffer.ToString());

            foreach (var child in trieNode.GetChildren())
            {
                buffer.Append(child.Character);
                GetWords(child, words, buffer);
                buffer.Length--;
            }
        }

        private Stack<TrieNode> GetTrieNodesStack(string s, bool isWord = true)
        {
            var nodes = new Stack<TrieNode>(s.Length + 1);
            var trieNode = RootTrieNode;
            nodes.Push(trieNode);
            foreach (var c in s)
            {
                trieNode = trieNode.GetChild(c);
                if (trieNode == null)
                {
                    nodes.Clear();
                    break;
                }
                nodes.Push(trieNode);
            }
            if (isWord)
            {
                if (!trieNode?.IsWord ?? true)
                {
                    throw new ArgumentOutOfRangeException($"{s} does not exist in trie.");
                }
            }
            return nodes;
        }

        private int RemoveWord(Stack<TrieNode> trieNodes)
        {
            var removeCount = trieNodes.Peek().WordCount;
            trieNodes.Peek().WordCount = 0;
            Trim(trieNodes);
            return removeCount;
        }

        private void RemovePrefix(Stack<TrieNode> trieNodes)
        {
            if (trieNodes.Any())
            {
                trieNodes.Peek().Clear();
                Trim(trieNodes);
            }
        }

        /// <summary>Trims unused nodes after removal to keep the trie compact.</summary>
        private void Trim(Stack<TrieNode> trieNodes)
        {
            while (trieNodes.Count > 1)
            {
                var trieNode = trieNodes.Pop();
                var parentTrieNode = trieNodes.Peek();
                if (trieNode.IsWord || trieNode.GetChildren().Any())
                    break;
                parentTrieNode.RemoveChild(trieNode.Character);
            }
        }
    }
}
