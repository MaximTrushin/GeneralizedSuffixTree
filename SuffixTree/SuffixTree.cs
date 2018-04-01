﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuffixTree
{
    public class SuffixTree<T>
    {
        private readonly int _minSuffixLength;
        private const int MaxWordLength = 100; //Max indexed word length

        private readonly Node<T> _root;
        private int _position = -1;
        private int _lastNodeIndex = -1;
        private Node<T> _needSuffixLink;

        private int _remainder;
        private Node<T> _activeNode;
        private int _activeLength;
        private int _activeEdgePosition;
        private string _source;
        private int _wordNumber;
        private T _value;

        public SuffixTree(int minSuffixLength)
        {
            _minSuffixLength = minSuffixLength;
            _root = new Node<T>(string.Empty,++_lastNodeIndex, _wordNumber);
            _activeNode = _root;
        }

        public void AddWord(string word, T value)
        {
            _wordNumber++;
            _remainder = 0;
            _activeNode = _root;
            _activeLength = 0;
            _activeEdgePosition = -1;
            _position = -1;
            _value = value;

            if (word.Length > MaxWordLength) word = word.Substring(0, MaxWordLength);
            _source = word;
            foreach (var c in _source) AddChar(c);
        }

        private void AddSuffixLink(Node<T> node)
        {
            if (_needSuffixLink != null && node != _root)
            {
                _needSuffixLink.SuffixLink = node;
            }
            _needSuffixLink = node;
        }

        private char _ActiveEdgeChar => _source[_activeEdgePosition];

        private bool WalkDown(Node<T> next) //Canonize
        {
            if (_activeLength < next.Label.Length) return false;
            _activeEdgePosition = _activeEdgePosition + next.Label.Length;
            _activeLength = _activeLength - next.Label.Length;
            _activeNode = next;
            _activeNode.WordNumber = _wordNumber;
            return true;
        }

        private Node<T> NewNode(int start, int end, string source)
        {
            if (start == source.Length)
            { //Create terminal edge
                return null;
                //return new Node<T>(_terminal, ++_lastNodeIndex, _wordNumber);
            }
            return  new Node<T>(source.Substring(start, end - start + 1), ++_lastNodeIndex, _wordNumber);
        }

        private Node<T> NewNode(int start, int end)
        {
            if (start == _source.Length)
            { //Create terminal edge
                return null;
                //return new Node<T>(_terminal, ++_lastNodeIndex, _wordNumber);
            }
            return new Node<T>(_source.Substring(start, end - start + 1), ++_lastNodeIndex, _wordNumber);
        }

        /// <summary>
        /// Adds edge on top of existing nodes created for previous words.
        /// </summary>
        /// <param name="edge"></param>
        public Node<T> AddEdge(Node<T> edge)
        {
            var start = _activeEdgePosition;
            var end = _source.Length - 1;
            var c = _source[start];
            if (edge == null)
            {
                edge = NewNode(start, end);
                edge.WordNumber = _wordNumber;
                edge.AddData(_value, _wordNumber); //leaf
                return _activeNode.Edges[c] = edge;
            }

            var activeNode = _activeNode;
            var position = start;
            var sourceEnd = end;
            
            do
            {
                var edgePosition = 0;
                var edgeEnd = edge.Label.Length-1;
                while (position <= sourceEnd && edgePosition <= edgeEnd && _source[position] == edge.Label[edgePosition])
                {
                    position++;
                    edgePosition++;
                }
                if (position > sourceEnd && edgePosition > edgeEnd)
                {
                    //Existing edge is equal to substring we want to create edge for.
                    edge.AddData(_value, _wordNumber); //leaf
                    edge.WordNumber = _wordNumber;
                    return edge;
                }
                if (edgePosition > edgeEnd)
                { //Existing edge is substring.
                    edge.Edges.TryGetValue(_source[position], out var nextEdge);
                    if (nextEdge == null)
                    {
                        nextEdge = NewNode(position, _source.Length - 1);
                        nextEdge.AddData(_value, _wordNumber); //leaf
                        edge.WordNumber = _wordNumber;
                        return edge.Edges[_source[position]] = nextEdge; //Adding leaf
                    }
                    //start = position;
                    activeNode = edge;
                    edge = nextEdge;
                    continue;
                }
                // symbol mismatch. Need to split.
                var leaf = Split(edge, edgePosition, activeNode, position, out _);

                return leaf;

            } while (position <= end);
            return edge;
        }

        private Node<T> Split(Node<T> edge, int edgePosition, Node<T> activeNode, int position, out Node<T> split)
        {
            split = NewNode(0, edgePosition - 1, edge.Label);
            split.WordNumber = _wordNumber;
            activeNode.Edges[edge.Label[0]] = split;
            edge.Label = edge.Label.Substring(edgePosition);
            split.Edges[edge.Label[0]] = edge;

            var leaf = NewNode(position, _source.Length - 1);
            if (leaf != null)
            {
                leaf.AddData(_value, _wordNumber);
                leaf.WordNumber = _wordNumber;
                split.Edges[_source[position]] = leaf;
            }
            else split.AddData(_value, _wordNumber);
            return leaf;
        }

        private void AddChar(char c)
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
                
                var next = _activeNode.GetEdge(_ActiveEdgeChar, _wordNumber, out var existingEdge);
                if (next == null)
                {
                    AddEdge(existingEdge);
                    // rule 2
                }
                else
                {
                    if (WalkDown(next)) continue; // observation 2
                    if (next.Label[_activeLength] == c)
                    {
                        // observation 1
                        _activeLength++;
                        AddSuffixLink(_activeNode);
                        //  observation 3
                        break;
                    }

                    Split(next, _activeLength, _activeNode, _position, out var split);

                    AddSuffixLink(split);
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
        public Node<T> SearchNode(string word)
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
                    return currentEdge;
                }
                // advance to next NodeA<T>
                currentNode = currentEdge;
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
            PrintInternalNodes(_root, sb, _root);
            sb.AppendLine("}");
            sb.AppendLine("edges:{");
            PrintEdges(_root, sb);
            sb.AppendLine("links:{");
            PrintSLinks(_root, sb);
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
                    PrintLeaves(child, sb);
                }
        }

        static void PrintInternalNodes(Node<T> x, StringBuilder sb, Node<T> root = null)
        {
            if (x != root
                && x.Edges.Count > 0)
            {
                sb.Append(x.Number + ",");
            }

            
            foreach (var child in x.Edges.Values)
            {
                PrintInternalNodes(child, sb);
            }

        }

        static void PrintEdges(Node<T> x, StringBuilder sb)
        {
            foreach (var child in x.Edges.Values)
            {
                sb.AppendLine(x.Number + "->"
                        + child.Number + "=" + child.Label);
                PrintEdges(child, sb);
            }
        }

        static void PrintSLinks(Node<T> x, StringBuilder sb)
        {
            if (x.SuffixLink != null)
            {
                sb.AppendLine(x.Number + "->" + x.SuffixLink.Number);
            }
            foreach (var child in x.Edges.Values)
            {
                PrintSLinks(child, sb);
            }
        }

    }
}
