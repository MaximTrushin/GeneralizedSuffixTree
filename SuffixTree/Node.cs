using System;
using System.Collections.Generic;
using System.Linq;

namespace SuffixTree
{
    public class Node<T>
    {
        public Node<T> SuffixLink;
        private Dictionary<char, Edge<T>> _edges;
        public Dictionary<char, Edge<T>> Edges => _edges ?? (_edges = new Dictionary<char, Edge<T>>());
        private List<T> _data;
        public List<T> Data => _data ?? (_data = new List<T>());

        public int Number { get; }

        public Node(int number)
        {
            Number = number;
        }

        private T _previouslyAssignedValue;

        public void AddData(T value)
        {
            if (value.Equals(_previouslyAssignedValue) && _data != null) return;
            Data.Add(value);
            _previouslyAssignedValue = value;
        }


        public IEnumerable<T> GetData()
        {
            if (_data == null) yield break;
            IEnumerable<T> result = _data;
            if (_edges != null)
            {
                var childData = _edges.Values.Select((e) => e.Target).SelectMany((t) => t.GetData());
                result = _data.Concat(childData).Distinct();
            }
            
            foreach (var d in result)
                yield return d;
        }
    }
}