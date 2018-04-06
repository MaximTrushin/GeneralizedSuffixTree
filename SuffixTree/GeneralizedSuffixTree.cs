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


        private Node<T> _activeNode;
        //private int _activeEdgePosition;
        private string _source;
        private T _value;
        private Node<T>[] _nodes = new Node<T>[MaxWordLength];
        private int _lastPositionStored;
        private int _wordNumber = -1;
        private Node<T> _lastSuffix;
        private int _lastSuffixPosition;

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
            _position = -1;
            _value = value;
            _lastPositionStored = -1;

            if (word.Length > MaxWordLength) word = word.Substring(0, MaxWordLength);
            _source = word;
            _nodes[word.Length - 1] = null;
            _wordNumber++;
            _lastSuffixPosition = -1;
            foreach (var c in _source) AddChar(c);
            //AddChar('\0');
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
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="leafStart">Position where leaf starts</param>
        /// <param name="leafEnd"></param>
        private void SaveLeaf(Node<T> node, int leafStart, int leafEnd)
        {
            if (_nodes[leafEnd] != null && _nodes[leafEnd] != node)
            {
                //Debug.Assert(_nodes[leafEnd].SuffixLink != node, "_nodes[leafEnd].SuffixLink != node");
                _nodes[leafEnd].SuffixLink = node;
            }
            
            _nodes[leafEnd] = node;
            //Debug.Assert(node.Label.EndsWith(_source[leafStart].ToString()));

            if (node.SuffixLink != null)
            {
                _lastSuffix = node.SuffixLink;
                _lastSuffixPosition = leafStart;
            }
        }

        private void SaveNode(Node<T> node, int edgeEnd)
        {
            if (_nodes[edgeEnd] != null && _nodes[edgeEnd] != node)
            {
                //Debug.Assert(_nodes[edgeEnd].SuffixLink != node, "_nodes[edgeEnd].SuffixLink = node");
                _nodes[edgeEnd].SuffixLink = node;
                
            }
            _nodes[edgeEnd] = node;
            Debug.Assert(node.Label.EndsWith(_source[edgeEnd].ToString()));
            if (edgeEnd > _lastPositionStored) _lastPositionStored = edgeEnd;

            if (node.SuffixLink != null)
            {
                _lastSuffix = node.SuffixLink;
                _lastSuffixPosition = edgeEnd;
            }
        }

        private Node<T> Split(Node<T> edge, int edgePosition, Node<T> activeNode, int position, out Node<T> split)
        {
            split = NewNode(0, edgePosition - 1, edge.Label);
            activeNode.AddEdge(edge.Label[0], split);
            edge.Label = edge.Label.Substring(edgePosition);
            split.AddEdge(edge.Label[0],edge);

            var leaf = position == _source.Length ? null:NewNode(_source.Substring(position, _source.Length - position));
            if (leaf != null)
            {
                leaf.AddData(_value, _wordNumber);
                split.AddEdge(_source[position], leaf);
            }
            else split.AddData(_value, _wordNumber);
            return leaf;
        }

        private void AddChar(char c)
        {
            ++_position;
            Node<T> edge = null;
            var position = Math.Max(_position, _lastSuffixPosition + 1);
            //if (position != position && _lastSuffixPosition < _source.Length - 1) position--;
            if (_lastSuffixPosition == _source.Length - 1)
            {
                //Walk through all leafs and mark
                while (_lastSuffix != null)
                {
                    _lastSuffix.AddData(_value, _wordNumber);
                    _lastSuffix = _lastSuffix.SuffixLink;
                }
                return;
            }

            var sourceEnd = _source.Length - 1;
            if (position <= sourceEnd)
            {
                edge = _activeNode.GetEdge(_source[position]);
                
                if (edge == null)
                {
                    edge = NewNode(_source.Substring(position, sourceEnd - position + 1));
                    edge.AddData(_value, _wordNumber); //leaf
                    _activeNode.AddEdge(_source[position], edge);
                    SaveLeaf(edge, position, sourceEnd);
                    _activeNode = _root;
                    _lastSuffix = null;
                    _lastSuffixPosition = -1;
                    return;
                }
            }
            _lastSuffix = null;
            _lastSuffixPosition = -1;
            var activeNode = _activeNode;
            while (position <= sourceEnd){
                var edgePosition = 0;

                Debug.Assert(edge != null, nameof(edge) + " != null");
                var label = edge.Label;
                var edgeEnd = label.Length - 1;

                while (position <= sourceEnd && edgePosition <= edgeEnd && _source[position] == label[edgePosition])
                {
                    if (position > _lastPositionStored && position < sourceEnd) {
                        _nodes[position] = null; 
                        _lastPositionStored = position;
                    }
                    position++;
                    edgePosition++;
                }
                if (position > sourceEnd && edgePosition > edgeEnd)
                {
                    //Existing edge is equal to substring we want to create edge for.
                    edge.AddData(_value, _wordNumber); //leaf
                    SaveNode(edge, position - 1);
                    _activeNode = _lastSuffix ?? _root;
                    return;
                }
                if (edgePosition > edgeEnd)
                { //Existing edge is substring.

                    edge.Edges.TryGetValue(_source[position], out var nextEdge);
                    SaveNode(edge, position - 1);
                    if (nextEdge == null)
                    {
                        nextEdge = NewNode(_source.Substring(position, _source.Length - position));
                        nextEdge.AddData(_value, _wordNumber);
                        _activeNode = _lastSuffix ?? _root;
                        edge.AddEdge(_source[position], nextEdge); //Adding leaf
                        SaveLeaf(nextEdge, position, sourceEnd);//leaf
                        return;
                    }
                    activeNode = edge;
                    edge = nextEdge;
                    continue;
                }
                // symbol mismatch. Need to split.
                var leaf = Split(edge, edgePosition, activeNode, position, out var split);
                SaveNode(split, position - 1);
                if (leaf != null)
                {
                    if (_nodes[sourceEnd] != null && _nodes[sourceEnd] != leaf)
                    {
                        //Debug.Assert(_nodes[sourceEnd].SuffixLink != leaf, "_nodes[sourceEnd].SuffixLink == leaf");
                        _nodes[sourceEnd].SuffixLink = leaf;
                        //Debug.Assert(_nodes[sourceEnd].SuffixLink != leaf);
                    }
                    _nodes[sourceEnd] = leaf;
                    leaf.AddData(_value, _wordNumber);
                }
                _activeNode = _lastSuffix ?? _root; 
                return;
            }
            _activeNode = _lastSuffix ?? _root;
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
