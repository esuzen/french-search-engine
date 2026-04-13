using System;
using System.Collections.Generic;

namespace SearchModule.Objects
{
    internal class TrieNode
    {
        internal char Character { get; private set; }
        private IDictionary<char, TrieNode> Children { get; set; }
        internal bool IsWord => WordCount > 0;
        internal int WordCount { get; set; }

        internal TrieNode(char character)
        {
            Character = character;
            Children = new Dictionary<char, TrieNode>();
            WordCount = 0;
        }

        internal IEnumerable<TrieNode> GetChildren() => Children.Values;

        internal TrieNode GetChild(char character)
        {
            Children.TryGetValue(character, out TrieNode trieNode);
            return trieNode;
        }

        internal void SetChild(TrieNode child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            Children[child.Character] = child;
        }

        internal void RemoveChild(char character) => Children.Remove(character);

        internal void Clear()
        {
            WordCount = 0;
            Children.Clear();
        }
    }
}
