using System;
using System.Collections.Generic;
using System.Text;

namespace SuffixTree
{
    public class SuffixTree<T>
    {
        private static char Eow = '\0'; //End of word
        private const int MaxWordLength = 100; //Max indexed word length

        private const int oo = int.MaxValue;
        private readonly List<Node<T>> _nodes;
        char[] text;
        private readonly int _root;
        private int _position = -1;
        private int _lastNodeIndex = -1;
        private int _needSuffixLink;

        private int _remainder;
        private int _activeNode;
        private int _activeLength;
        private int _activeEdge;

        public SuffixTree()
        {
            _nodes = new List<Node<T>>(2 * MaxWordLength + 2);

            for (var i = 0; i < 100; i++) _nodes.Add(null);

            _root = NewNode(-1, -1);
            _activeNode = _root;
        }

        public void AddWord(string word, T value)
        {
            _remainder = 0;
            _activeNode = _root;
            _activeLength = 0;
            _activeEdge = 0;
            _position = -1;

            if (word.Length > MaxWordLength) word = word.Substring(0, MaxWordLength);
            text = string.Concat(word, '\0').ToCharArray();
            foreach (var c in word) AddChar(c, value);
            AddChar(Eow, value);
        }

        private void AddSuffixLink(int node)
        {
            if (_needSuffixLink > 0)
            {
                _nodes[_needSuffixLink].SuffixLink = node;
            }
            _needSuffixLink = node;
        }

        private char _ActiveEdgeChar => text[_activeEdge];

        private bool WalkDown(int next)
        {
            if (_activeLength > 0 && _activeLength >= _nodes[next].EdgeLength(_position))
            {
                _activeEdge = _activeEdge + _nodes[next].EdgeLength(_position);
                _activeLength = _activeLength - _nodes[next].EdgeLength(_position);
                _activeNode = next;
                return true;
            }
            return false;
        }

        private int NewNode(int start, int end)
        {
            _nodes[++_lastNodeIndex] = new Node<T>(start, end);
            return _lastNodeIndex;
        }

        private void AddChar(char c, T value)
        {
            ++_position;
            //text[++_position] = c;
            _needSuffixLink = -1;
            _remainder++;
            while (_remainder > 0)
            {
                if (_activeLength == 0)
                {
                    _activeEdge = _position;
                }

                if (!_nodes[_activeNode].Edges.ContainsKey(_ActiveEdgeChar))
                {
                    int leaf = NewNode(_position, oo);
                    _nodes[_activeNode].Edges[_ActiveEdgeChar] = leaf;
                    _nodes[leaf].AddData(value);
                    if (_ActiveEdgeChar!= Eow) AddSuffixLink(_activeNode);
                    // rule 2
                }
                else
                {
                    _nodes[_activeNode].AddData(value);
                    int next = _nodes[_activeNode].Edges[_ActiveEdgeChar];
                    _nodes[next].AddData(value);
                    if (WalkDown(next)) continue;

                    // observation 2
                    if (text[_nodes[next].Start + _activeLength] == c)
                    //if (_activeEdge + _activeLength == c)
                    {
                        // observation 1
                        _activeLength++;
                        AddSuffixLink(_activeNode);
                        //  observation 3
                        break;
                    }

                    int split = NewNode(_nodes[next].Start, _nodes[next].Start + _activeLength);
                    _nodes[_activeNode].Edges[_ActiveEdgeChar] = split;
                    int leaf = NewNode(_position, oo);
                    _nodes[leaf].AddData(value);
                    _nodes[split].Edges[c] = leaf;
                    _nodes[next].Start = _nodes[next].Start + _activeLength;
                    _nodes[split].Edges[text[_nodes[next].Start]] = next;
                    AddSuffixLink(split);
                    // rule 2
                }

                _remainder--;
                if (_activeNode == _root && _activeLength > 0)
                {
                    // rule 1
                    _activeLength--;
                    _activeEdge = _position - _remainder + 1;
                }
                else
                {
                    _activeNode = _nodes[_activeNode].SuffixLink > 0 ? _nodes[_activeNode].SuffixLink : _root;
                }
            }
        }

        private string EdgeString(int node)
        {
            return new string( text, _nodes[node].Start, Math.Min(_position + 1, _nodes[node].End) - _nodes[node].Start);
        }

        public string PrintTree()
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph {");

            sb.AppendLine("//------leaves------");
            PrintLeaves(_root, sb);
            sb.AppendLine("//------internal _nodes------");
            printInternalNodes(_root, sb);
            sb.AppendLine("//------edges------");
            printEdges(_root, sb);
            sb.AppendLine("//------suffix links------");
            printSLinks(_root, sb);
            sb.AppendLine("}");
            return sb.ToString();
        }

        private void PrintLeaves(int x, StringBuilder sb)
        {
            if (_nodes[x].Edges.Count == 0)
            {
                sb.AppendLine("\tnode" + x + " ");
            }
            else
                foreach (int child in _nodes[x].Edges.Values)
                {
                    PrintLeaves(child, sb);
                }
        }

        void printInternalNodes(int x, StringBuilder sb)
        {
            if (x != _root
                && _nodes[x].Edges.Count > 0)
            {
                sb.AppendLine("\tnode" + x + " ");
            }

            
            foreach (int child in _nodes[x].Edges.Values)
            {
                printInternalNodes(child, sb);
            }

        }

        void printEdges(int x, StringBuilder sb)
        {
            foreach (int child in _nodes[x].Edges.Values)
            {
                sb.AppendLine("\tnode"
                        + x + " -> node"
                        + child + " [label=" +EdgeString(child));
                printEdges(child, sb);
            }
        }

        void printSLinks(int x, StringBuilder sb)
        {
            if (_nodes[x].SuffixLink > 0)
            {
                sb.AppendLine("\tnode" + x + " -> node" + _nodes[x].SuffixLink);
            }
            foreach (int child in _nodes[x].Edges.Values)
            {
                printSLinks(child, sb);
            }
        }
    }
}
