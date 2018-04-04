using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SuffixTree
{
    public class GeneralizedSuffixTree<T>
    {
        private readonly int _minSuffixLength;
        private const int MaxWordLength = 100; //Max indexed word length

        private readonly Node<T> _root;
        private int _position = -1;
        private int _lastNodeIndex = -1;
        private Node<T> _needSuffixLink;

        private Node<T> _activeNode;
        private int _activeLength;
        //private int _activeEdgePosition;
        private string _source;
        private T _value;
        private Node<T> _lastLeaf;
        private Node<T>[] _nodes = new Node<T>[MaxWordLength];
        private int _lastPositionStored;
        private int _wordNumber = -1;

        public GeneralizedSuffixTree(int minSuffixLength)
        {
            _minSuffixLength = minSuffixLength;
            _root = new Node<T>(string.Empty,++_lastNodeIndex);
            _activeNode = _root;
        }

        public Node<T> Root => _root;

        public void AddWord(string word, T value)
        {
            _activeNode = _root;
            _activeLength = 0;
            _position = -1;
            _value = value;
            _lastLeaf = null;
            _lastPositionStored = -1;

            if (word.Length > MaxWordLength) word = word.Substring(0, MaxWordLength);
            _source = word;
            _nodes[word.Length - 1] = null;
            _wordNumber++;
            foreach (var c in _source) AddChar(c);
            //AddChar('\0');
        }

        private void AddSuffixLink(Node<T> node, Node<T> leaf)
        {
            if (_needSuffixLink != null && node != _root && _needSuffixLink != node && node != null)
            {
                _needSuffixLink.SuffixLink = node;
                Debug.Assert(_needSuffixLink.Label.EndsWith(node.Label), _needSuffixLink.Label+"->"+ node.Label);
            }

            if (_lastLeaf != null && leaf != null)
            {
                _lastLeaf.SuffixLink = leaf;
                //_lastLeafStart.SuffixLink = leafStart;
                Debug.Assert(_lastLeaf.Label.EndsWith(leaf.Label), _lastLeaf.Label + "->" + leaf.Label);
            }

            _needSuffixLink = node;
            if (leaf != null)
            {
                _lastLeaf = leaf;
            }
        }

        private bool WalkDown(Node<T> next) //Canonize
        {
            if (_activeLength < next.Label.Length) return false;
            //_activeEdgePosition = _activeEdgePosition + next.Label.Length;
            _activeLength = _activeLength - next.Label.Length;
            _activeNode = next;
            return true;
        }

        private Node<T> NewNode(int start, int end, string source)
        {
            if (start == source.Length)
            { //Create terminal edge
                return null;
            }
            return new Node<T>(source.Substring(start, end - start + 1), ++_lastNodeIndex);
        }

        private Node<T> NewNode(string label)
        {
            return new Node<T>(label, ++_lastNodeIndex);
        }

        /// <summary>
        /// Adds edge on top of existing nodes created for previous words.
        /// </summary>
        /// <param name="edge"></param>
        public Node<T> AddEdge(Node<T> edge)
        {
            //if (_activeEdgePosition == _source.Length) return null;
            var position = _position;
            var sourceEnd = _source.Length - 1;
            var c = _source[_position];
            if (edge == null)
            {
                edge = NewNode(_source.Substring(position, sourceEnd - position + 1));

                edge.AddData(_value, _wordNumber); //leaf
                SaveLeaf(edge, position, sourceEnd);
                //AddSuffixLink(_activeNode, edge);
                return _activeNode.Edges[c] = edge;
            }

            var activeNode = _activeNode;
            do
            {
                var edgePosition = 0;
                var label = edge.Label;
                var edgeEnd = label.Length-1;

                while (position <= sourceEnd && edgePosition <= edgeEnd && _source[position] == label[edgePosition])
                {
                    if (position > _lastPositionStored) _nodes[position] = null;
                    position++;
                    edgePosition++;
                }
                if (position > sourceEnd && edgePosition > edgeEnd)
                {
                    //Existing edge is equal to substring we want to create edge for.
                    edge.AddData(_value, _wordNumber); //leaf
                    //AddSuffixLink(null, edge);
                    SaveNode(edge, position - 1);
                    return edge;
                }
                if (edgePosition > edgeEnd)
                { //Existing edge is substring.

                    edge.Edges.TryGetValue(_source[position], out var nextEdge);
                    SaveNode(edge, position - 1);
                    if (nextEdge == null)
                    {
                        nextEdge = NewNode(_source.Substring(position, _source.Length - position));
                        nextEdge.AddData(_value, _wordNumber); //leaf
                        SaveLeaf(nextEdge, position, sourceEnd);

                        return edge.Edges[_source[position]] = nextEdge; //Adding leaf
                    }
                    activeNode = edge;
                    edge = nextEdge;
                    continue;
                }
                // symbol mismatch. Need to split.
                var leaf = Split(edge, edgePosition, activeNode, position, out var split);
                SaveNode(split, position);
                if (leaf != null)
                {
                    if (_nodes[sourceEnd] != null) _nodes[sourceEnd].SuffixLink = leaf;
                    _nodes[sourceEnd] = leaf;
                }
                return leaf;

            } while (position <= sourceEnd);
            return edge;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="position">Position where leaf starts</param>
        /// <param name="sourceEnd"></param>
        private void SaveLeaf(Node<T> edge, int position, int sourceEnd)
        {
            if (_nodes[sourceEnd] != null) _nodes[sourceEnd].SuffixLink = edge;
            _nodes[sourceEnd] = edge;
            if (position > _lastPositionStored)
            {
                _nodes[position] = null;
                _lastPositionStored = position;
            }
        }

        private void SaveNode(Node<T> node, int position)
        {
            if (_nodes[position] != null) _nodes[position].SuffixLink = node;
            _nodes[position] = node;
            if (position > _lastPositionStored) _lastPositionStored = position;
        }

        private Node<T> Split(Node<T> edge, int edgePosition, Node<T> activeNode, int position, out Node<T> split)
        {
            split = NewNode(0, edgePosition - 1, edge.Label);
            activeNode.Edges[edge.Label[0]] = split;
            edge.Label = edge.Label.Substring(edgePosition);
            split.Edges[edge.Label[0]] = edge;

            var leaf = position == _source.Length ? null:NewNode(_source.Substring(position, _source.Length - position));
            if (leaf != null)
            {
                leaf.AddData(_value, _wordNumber);
                split.Edges[_source[position]] = leaf;
            }
            else split.AddData(_value, _wordNumber);
            return leaf;
        }

        private void AddChar(char c)
        {
            ++_position;
            _needSuffixLink = null;
            
            //if (_activeLength == 0)
            //{
            //    _activeEdgePosition = _position;
            //}


            var next = _position < _source.Length?
                _activeNode.GetEdge(_source[_position], _wordNumber):
                null;
            if (next == next)
            {
                AddEdge(next);
                // rule 2
            }
            else
            {
                //if (WalkDown(next)) continue; // observation 2
                //if (next.Label[_activeLength] == c)
                //{
                //    // observation 1
                //    _activeLength++;
                //    AddSuffixLink(_activeNode, null);
                //    //  observation 3
                //    break;
                //}

                //var leaf = Split(next, _activeLength, _activeNode, _position, out var split);

                //AddSuffixLink(split, leaf);
                // rule 2
            }


            
        }


        public IEnumerable<T> Retrieve(string word)
        {
            if (word.Length < _minSuffixLength) return Enumerable.Empty<T>();
            var tmpNode = SearchNode(word, _root);
            return tmpNode == null
                ? Enumerable.Empty<T>()
                : tmpNode.GetData();
        }

        /// <summary>
        /// Returns the tree node (if present) that corresponds to the given string.
        /// </summary>
        /// <param name="word">Given word</param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static Node<T> SearchNode(string word, Node<T> root)
        {
              //Verifies if exists a path from the root to a NodeA<T> such that the concatenation
              //of all the labels on the path is a superstring of the given word.
              //If such a path is found, the last NodeA<T> on it is returned.
             
            var currentEdge = root;

            for (var i = 0; i < word.Length; ++i)
            {
                var ch = word[i];
                // follow the EdgeA<T> corresponding to this char
                currentEdge.Edges.TryGetValue(ch, out currentEdge);
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
                    return currentEdge;
                }
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

    }
}
