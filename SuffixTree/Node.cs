using System.Collections.Generic;
using System.Linq;

namespace SuffixTree
{
    public class Node<T>
    {
        public Node<T> SuffixLink;
        private Dictionary<char, Node<T>> _edges;
        public Dictionary<char, Node<T>> Edges => _edges ?? (_edges = new Dictionary<char, Node<T>>());
        private HashSet<T> _data;
        public HashSet<T> Data => _data ?? (_data = new HashSet<T>());

        public string Label { get; internal set; }
        public int Number { get; }
        public int WordNumber { get; internal set; }

        public Node(string label, int number, int wordNumber)
        {
            Label = label;
            Number = number;
            WordNumber = wordNumber;
        }

        private int _prevWordNumber = -1;
        public void AddData(T value, int wordNumber)
        {
            if (_prevWordNumber == wordNumber)
            {
                return;
            }
            _prevWordNumber = wordNumber;
            Data.Add(value);
        }


        public IEnumerable<T> GetData()
        {
            IEnumerable<T> result = _data;
            if (_edges != null)
            {
                result = _edges.Values.SelectMany((t) => t.GetData()).Distinct();
                if (_data != null)
                    result = _data.Concat(result).Distinct();
            }

            foreach (var d in result)
                yield return d;
        }

        public Node<T> GetEdge(char activeEdgeChar, int wordNumber, out Node<T> found)
        {
            found = null;
            if (_edges == null) return null;
            _edges.TryGetValue(activeEdgeChar, out found);
            if (found?.WordNumber != wordNumber) return null;
            return found;
        }

    }
}