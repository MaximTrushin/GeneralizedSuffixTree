using System;
using System.Collections.Generic;

namespace SuffixTree
{
    class Node<T>
    {
        public int Start { get; internal set; }
        public readonly int End;
        public int SuffixLink;

        private Dictionary<char, int> _edges;
        public Dictionary<char, int> Edges => _edges ?? (_edges = new Dictionary<char, int>());
        private List<T> _data;
        public List<T> Data => _data ?? (_data = new List<T>());

        public Node(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int EdgeLength(int position)
        {
            return Math.Min(End, position + 1) - Start;
        }

        private T _previouslyAssignedValue;
        public void AddData(T value)
        {
            if (Start > 1 && !value.Equals(_previouslyAssignedValue))
            {
                Data.Add(value);
                _previouslyAssignedValue = value;
            }
            
        }
    }
}