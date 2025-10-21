using System;
using System.Collections.Generic;

namespace Cheat
{
    public class TrieNode
    {
        public bool IsWord { get; set; }
        public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();
    }
}
