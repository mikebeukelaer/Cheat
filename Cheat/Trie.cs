using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cheat
{
    public class Trie
    {
        private readonly TrieNode _root = new TrieNode();

        public void AddWord(string word)
        {
            var node = _root;

            foreach (char c in word.ToLower())
            {
                if (!node.Children.ContainsKey(c))
                    node.Children[c] = new TrieNode();

                node = node.Children[c];
            }

            node.IsWord = true;
        }

        public bool Search(string word)
        {
            var node = _root;

            foreach (char c in word)
            {
                if (!node.Children.ContainsKey(c))
                    return false;

                node = node.Children[c];
            }

            return node.IsWord;
        }
    }
}
