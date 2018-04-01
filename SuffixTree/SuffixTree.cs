using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuffixTree
{
    public class SuffixTree<T>
    {
        private readonly int _minSuffixLength;
        private static char Eow = '\0'; //End of word
        private const int MaxWordLength = 100; //Max indexed word length

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
        private int _wordNumber;
        private const string _terminal = "\0";

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
            //_source = string.Concat(word, '\0');
            _source = word;
            foreach (var c in _source) AddChar(c, value);
            //AddChar('\0', value, _source);

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
            if (/*_activeLength > 0 && */_activeLength >= next.Label.Length)
            {
                _activeEdgePosition = _activeEdgePosition + next.Label.Length;
                _activeLength = _activeLength - next.Label.Length;
                _activeNode = next.Target;
                _activeNode.AddData(value, _wordNumber);
                return true;
            }
            return false;
        }

        private Edge<T> NewEdge(int start, int end, string source)
        {
            var node = new Node<T>(++_lastNodeIndex, _wordNumber);

            if (start == source.Length)
            { //Create terminal edge
                return new Edge<T>(_terminal, node);
            }


            var edge = new Edge<T>(source.Substring(start, end - start), node);
            return edge;
        }

        /// <summary>
        /// Adds edge on top of existing nodes created for previous words.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end">End of edge (not included).</param>
        /// <param name="value"></param>
        public Edge<T> AddEdge(int start, int end, T value)
        {
            var c = _source[start];
            _activeNode.Edges.TryGetValue(c, out var edge);
            if (edge == null)
            {
                edge = NewEdge(start, end, _source);
                edge.Target.AddData(value, _wordNumber);
                return _activeNode.Edges[c] = edge;
            }

            var activeNode = _activeNode;
            var position = start;
            var sourceEnd = Math.Min(end, _source.Length - 1); //TODO: Remove Min
            
            do
            {
                var edgePosition = 0;
                var edgeEnd = edge.Label.Length-1; //TODO remove Min
                while (position <= sourceEnd && edgePosition <= edgeEnd && _source[position] == edge.Label[edgePosition])
                {
                    position++;
                    edgePosition++;
                }
                if (position > sourceEnd && edgePosition > edgeEnd)
                {
                    //Existing edge is equal to substring we want to create edge for.
                    edge.Target.AddData(value, _wordNumber);
                    return edge;
                }
                if (edgePosition > edgeEnd)
                { //Existing edge is substring.
                    
                    edge.Target.AddData(value, _wordNumber);
                    edge.Target.Edges.TryGetValue(_source[position], out var nextEdge);
                    if (nextEdge == null)
                    {
                        nextEdge = NewEdge(position, _source.Length, _source);
                        nextEdge.Target.AddData(value, _wordNumber);
                        return edge.Target.Edges[_source[position]] = nextEdge; //Adding leaf
                    }
                    //start = position;
                    activeNode = edge.Target;
                    edge = nextEdge;
                    continue;
                }
                // symbol mismatch. Need to split.
                var split = NewEdge(0, edgePosition, edge.Label); 
                activeNode.Edges[edge.Label[0]] = split;
                var leaf = NewEdge(position, _source.Length, _source);
                leaf.Target.AddData(value, _wordNumber);
                split.Target.Edges[position<_source.Length?_source[position]:'\0'] = leaf; 
                edge.Label = edge.Label.Substring(edgePosition);
                split.Target.Edges[edge.Label[0]] = edge;
                split.Target.Data.AddRange(edge.Target.Data.Where(i => !i.Equals(value)));
                //_activeNode.Data.ForEach(i => { if (!i.Equals(value)) split.Target.Data.Add(i); });
                split.Target.AddData(value, _wordNumber);
                return leaf;

            } while (position <= end);
            return edge;
        }

        private void AddChar(char c, T value)
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
                
                var next = _activeNode.GetEdge(_ActiveEdgeChar, _wordNumber);
                if (next == null)
                {
                    var newEdge = AddEdge(_activeEdgePosition, _source.Length, value);
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


                    if (next.Label[_activeLength] == c)
                    {
                        // observation 1
                        _activeLength++;
                        AddSuffixLink(_activeNode);
                        //  observation 3
                        break;
                    }

                    var split = NewEdge(0, _activeLength, next.Label);
                    _activeNode.Edges[next.Label[0]] = split;
                    var leaf = NewEdge(_position, _source.Length, _source);
                    leaf.Target.AddData(value, _wordNumber);
                    split.Target.Edges[_source[_position]] = leaf;
                    next.Label = next.Label.Substring(_activeLength);
                    split.Target.Edges[next.Label[0]] = next;
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
                currentNode.Edges.TryGetValue(ch, out var currentEdge);
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

        public string PrintTree()
        {
            var sb = new StringBuilder();
            sb.Append("leaves:{");
            PrintLeaves(_root, sb);
            sb.AppendLine("}");
            sb.Append("nodes:{");
            printInternalNodes(_root, sb);
            sb.AppendLine("}");
            sb.AppendLine("edges:{");
            printEdges(_root, sb);
            sb.AppendLine("links:{");
            printSLinks(_root, sb);
            sb.AppendLine("}");
            return sb.ToString();
        }

        private void PrintLeaves(Node<T> x, StringBuilder sb)
        {
            if (x.Edges.Count == 0)
            {
                sb.Append(x.Number + ",");
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
                sb.Append(x.Number + ",");
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
                sb.AppendLine(x.Number + "->"
                        + child.Target.Number + "=" + child.Label);
                printEdges(child.Target, sb);
            }
        }

        void printSLinks(Node<T> x, StringBuilder sb)
        {
            if (x.SuffixLink != null)
            {
                sb.AppendLine(x.Number + "->" + x.SuffixLink.Number);
            }
            foreach (var child in x.Edges.Values)
            {
                printSLinks(child.Target, sb);
            }
        }

    }
}
