using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

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

        public Node(string label, int number)
        {
            Label = label;
            Number = number;
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
            if (_edges == null) return _data.Distinct();
            var result = _edges.Values.SelectMany(t => t.GetData());
            if (_data == null) return result.Distinct();
            return _data.Concat(result).Distinct();

        }

        public Node<T> GetEdge(char activeEdgeChar, int wordNumber)
        {
            if (_edges == null) return null;
            _edges.TryGetValue(activeEdgeChar, out var found);
            //if (found?.WordNumber != wordNumber) return null;
            return found;
        }

    }
}