using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SuffixTree
{
    public class Node<T>
    {
        public Node<T> SuffixLink;
        private Dictionary<char, Node<T>> _edges;
        public Dictionary<char, Node<T>> Edges => _edges ?? (_edges = new Dictionary<char, Node<T>>());
        private List<T> _data;
        public List<T> Data => _data ?? (_data = new List<T>());

        public string Label { get; internal set; }
        public int Number { get; }
        public int WordNumber { get; private set; }

        public Node(string label, int number, int wordNumber)
        {
            Label = label;
            Number = number;
            WordNumber = wordNumber;
        }

        private T _previouslyAssignedValue;
        private bool _dataInitialized;

        public void AddData(T value, int wordNumber)
        {
            if (_dataInitialized)
            {
                if (value.Equals(_previouslyAssignedValue)) return;
            }
            else _dataInitialized = true;
            Data.Add(value);
            _previouslyAssignedValue = value;
            WordNumber = wordNumber;
        }


        public IEnumerable<T> GetData()
        {
            if (_data == null) yield break;
            IEnumerable<T> result = _data;
            if (_edges != null)
            {
                var childData = _edges.Values.SelectMany((t) => t.GetData());
                result = _data.Concat(childData).Distinct();
            }

            foreach (var d in result)
                yield return d;
        }

        public Node<T> GetEdge(char activeEdgeChar, int wordNumber)
        {
            if (_edges == null) return null;
            _edges.TryGetValue(activeEdgeChar, out var result);
            if (result?.WordNumber != wordNumber) return null;
            return result;
        }

    }
}