using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuffixTree
{
    public class SuffixTree<T>
    {
        private readonly int _minSuffixLength;
        private static char Eow = '\0'; //End of word
        private const int MaxWordLength = 100; //Max indexed word length

        private const int oo = int.MaxValue;
        private readonly Node<T> _root;
        private int _position = -1;
        private int _lastNodeIndex = -1;
        private Node<T> _needSuffixLink;

        private int _remainder;
        private Node<T> _activeNode;
        private int _activeLength;
        //private Edge<T> _activeEdge;
        private int _activeEdgePosition;
        private string _source;
        private int _wordNumber = 0;

        public SuffixTree(int minSuffixLength)
        {
            _minSuffixLength = minSuffixLength;
            _root = new Node<T>(++_lastNodeIndex, _wordNumber);
            _activeNode = _root;
        }

        public void AddWord(string word, T value)
        {
            _wordNumber++;
            _remainder = 0;
            _activeNode = _root;
            _activeLength = 0;
            //_activeEdge = null;
            _activeEdgePosition = -1;
            _position = -1;

            if (word.Length > MaxWordLength) word = word.Substring(0, MaxWordLength);
            _source = string.Concat(word, '\0');
            foreach (var c in _source) AddChar(c, value, _source);

            //_activeNode.Edges.TryGetValue(_ActiveEdgeChar, out var next);
            //next?.Target.AddData(value, _wordNumber); // 
        }

        private void AddSuffixLink(Node<T> node)
        {
            if (_needSuffixLink != null && node != _root)
            {
                _needSuffixLink.SuffixLink = node;
            }
            _needSuffixLink = node;
        }

        private char _ActiveEdgeChar => /*_activeEdge?.Source[_activeEdgePosition]??*/_source[_activeEdgePosition];

        private bool WalkDown(Edge<T> next, T value) //Canonize
        {
            if (/*_activeLength > 0 && */_activeLength >= next.EdgeLength(_position))
            {
                _activeEdgePosition = _activeEdgePosition + next.EdgeLength(_position);
                _activeLength = _activeLength - next.EdgeLength(_position);
                _activeNode = next.Target;
                _activeNode.AddData(value, _wordNumber);
                return true;
            }
            return false;
        }

        private Edge<T> NewEdge(int start, int end, string source)
        {
            var node = new Node<T>(++_lastNodeIndex, _wordNumber);
            var edge = new Edge<T>(start, end, source, node);
            return edge;
        }

        /// <summary>
        /// Adds edge on top of existing nodes created for previous words.
        /// </summary>
        /// <param name="activeNode"></param>
        /// <param name="start"></param>
        /// <param name="end">End of edge (not included).</param>
        /// <param name="source"></param>
        /// <param name="wordNumber"></param>
        /// <param name="value"></param>
        public Edge<T> AddEdge(Node<T> activeNode, int start, int end, string source, int wordNumber, T value)
        {
            var c = source[start];
            activeNode.Edges.TryGetValue(c, out var edge);
            if (edge == null)
            {
                edge = NewEdge(start, end, source);
                edge.Target.AddData(value, wordNumber);
                return activeNode.Edges[c] = edge;
            }
            
            var position = start;
            var sourceEnd = Math.Min(end, source.Length - 1);
            
            do
            {
                var edgePosition = edge.Start;
                var edgeEnd = Math.Min(edge.End - 1, edge.Source.Length - 1);
                while (position <= sourceEnd && edgePosition <= edgeEnd && source[position] == edge.Source[edgePosition])
                {
                    position++;
                    edgePosition++;
                }
                if (position > sourceEnd)
                {
                    //Existing edge is equal to substring we want to create edge for.
                    edge.Target.AddData(value, wordNumber);
                    return edge;
                }
                if (edgePosition > edgeEnd)
                { //Existing edge is substring.
                    
                    edge.Target.AddData(value, wordNumber);
                    edge.Target.Edges.TryGetValue(source[position], out var nextEdge);
                    if (nextEdge == null)
                    {
                        nextEdge = NewEdge(position, source.Length, source);
                        nextEdge.Target.AddData(value, wordNumber);
                        return edge.Target.Edges[source[position]] = nextEdge; //Adding leaf
                    }
                    //start = position;
                    activeNode = edge.Target;
                    edge = nextEdge;
                    continue;
                }
                // symbol mismatch. Need to split.
                var split = NewEdge(edge.Start, edgePosition, edge.Source);
                activeNode.Edges[edge.Source[edge.Start]] = split;
                var leaf = NewEdge(position, source.Length, source);
                leaf.Target.AddData(value, _wordNumber);
                split.Target.Edges[source[position]] = leaf;
                edge.Start = edgePosition;
                split.Target.Edges[edge.Source[edge.Start]] = edge;
                split.Target.Data.AddRange(edge.Target.Data.Where(i => !i.Equals(value)));
                //_activeNode.Data.ForEach(i => { if (!i.Equals(value)) split.Target.Data.Add(i); });
                split.Target.AddData(value, _wordNumber);
                return leaf;

            } while (position <= end);
            return edge;
        }

        private void AddChar(char c, T value, string source)
        {
            ++_position;
            _needSuffixLink = null;
            _remainder++;
            while (_remainder > 0)
            {
                if (_activeLength == 0)
                {
                    _activeEdgePosition = _position;
                }

                //_activeNode.Edges.TryGetValue(_ActiveEdgeChar, out var next);
                var next = _activeNode.GetEdge(_ActiveEdgeChar, _wordNumber);
                if (next == null)
                {
                    //var newEdge = NewEdge(_activeEdgePosition, oo, source);
                    //_activeNode.Edges[_ActiveEdgeChar] = newEdge;
                    var newEdge = AddEdge(_activeNode, _activeEdgePosition, source.Length, source, _wordNumber, value);
                    newEdge.Target.AddData(value, _wordNumber);//TODO put inside AddEdge
                    if (_ActiveEdgeChar!= Eow) AddSuffixLink(_activeNode);
                    // rule 2
                }
                else
                {
                    //_activeEdge = next;
                    _activeNode.AddData(value, _wordNumber);
                    //next.Target.AddData(value);
                    if (WalkDown(next, value)) continue; // observation 2


                    if (next.Source[next.Start + _activeLength] == c)
                    {
                        // observation 1
                        _activeLength++;
                        AddSuffixLink(_activeNode);
                        //  observation 3
                        break;
                    }

                    var split = NewEdge(next.Start, next.Start + _activeLength, next.Source);
                    _activeNode.Edges[next.Source[next.Start]] = split;
                    var leaf = NewEdge(_position, source.Length, source);
                    leaf.Target.AddData(value, _wordNumber);
                    split.Target.Edges[source[_position]] = leaf;
                    next.Start = next.Start + _activeLength;
                    split.Target.Edges[next.Source[next.Start]] = next;
                    //_activeNode.Data.ForEach(i => { if (!i.Equals(value)) split.Target.Data.Add(i); });
                    split.Target.Data.AddRange(next.Target.Data.Where(i => !i.Equals(value)));
                    split.Target.AddData(value, _wordNumber);
                    AddSuffixLink(split.Target);
                    // rule 2
                }

                _remainder--;
                if (_activeNode == _root && _activeLength > 0)
                {
                    // rule 1
                    _activeLength--;
                    _activeEdgePosition = _position - _remainder + 1;
                }
                else
                {
                    _activeNode = _activeNode.SuffixLink ?? _root;
                }
            }
        }


        public IEnumerable<T> Retrieve(string word)
        {
            if (word.Length < _minSuffixLength) return Enumerable.Empty<T>();
            var tmpNode = SearchNode(word);
            return tmpNode == null
                ? Enumerable.Empty<T>()
                : tmpNode.GetData();
        }


        /**
         * Returns the tree NodeA<T> (if present) that corresponds to the given string.
         */
        private Node<T> SearchNode(string word)
        {
            /*
             * Verifies if exists a path from the root to a NodeA<T> such that the concatenation
             * of all the labels on the path is a superstring of the given word.
             * If such a path is found, the last NodeA<T> on it is returned.
             */
            var currentNode = _root;

            for (var i = 0; i < word.Length; ++i)
            {
                var ch = word[i];
                // follow the EdgeA<T> corresponding to this char
                var currentEdge = currentNode.Edges[ch];
                if (null == currentEdge)
                {
                    // there is no EdgeA<T> starting with this char
                    return null;
                }
                var label = currentEdge.Label;
                var lenToMatch = Math.Min(word.Length - i, label.Length);

                if (!RegionMatches(word, i, label, 0, lenToMatch))
                {
                    // the label on the EdgeA<T> does not correspond to the one in the string to search
                    return null;
                }

                if (label.Length >= word.Length - i)
                {
                    return currentEdge.Target;
                }
                // advance to next NodeA<T>
                currentNode = currentEdge.Target;
                i += lenToMatch - 1;
            }

            return null;
        }

        private static bool RegionMatches(string first, int toffset, string second, int ooffset, int len)
        {
            for (var i = 0; i < len; i++)
            {
                var one = first[toffset + i];
                var two = second[ooffset + i];
                if (one != two) return false;
            }
            return true;
        }

        private string EdgeString(Edge<T> edge)
        {
            return edge.Source.Substring(edge.Start, Math.Min(edge.End, edge.Source.Length) - edge.Start);
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

        private void PrintLeaves(Node<T> x, StringBuilder sb)
        {
            if (x.Edges.Count == 0)
            {
                sb.AppendLine("\tnode" + x.Number + " ");
            }
            else
                foreach (var child in x.Edges.Values)
                {
                    PrintLeaves(child.Target, sb);
                }
        }

        void printInternalNodes(Node<T> x, StringBuilder sb)
        {
            if (x != _root
                && x.Edges.Count > 0)
            {
                sb.AppendLine("\tnode" + x.Number + " ");
            }

            
            foreach (var child in x.Edges.Values)
            {
                printInternalNodes(child.Target, sb);
            }

        }

        void printEdges(Node<T> x, StringBuilder sb)
        {
            foreach (var child in x.Edges.Values)
            {
                sb.AppendLine("\tnode"
                        + x.Number + " -> node"
                        + child.Target.Number + " [label=" + EdgeString(child));
                printEdges(child.Target, sb);
            }
        }

        void printSLinks(Node<T> x, StringBuilder sb)
        {
            if (x.SuffixLink != null)
            {
                sb.AppendLine("\tnode" + x.Number + " -> node" + x.SuffixLink.Number);
            }
            foreach (var child in x.Edges.Values)
            {
                printSLinks(child.Target, sb);
            }
        }

    }
}
