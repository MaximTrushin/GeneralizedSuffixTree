using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace SuffixTree
{
    public class Node<T>
    {
        public Node<T> SuffixLink
        {
            get => _suffixLink;
            set
            {
#if DEBUG
                string suffixLabel = "";
                var sl = value;

                do
                {
                    suffixLabel = sl.Label + suffixLabel;
                    sl = sl.Parent;
                } while (sl != null);

                string thisLabel = "";
                sl = this;

                do
                {
                    thisLabel = sl.Label + thisLabel;
                    sl = sl.Parent;
                } while (sl != null);



                Debug.Assert(suffixLabel.Length + 1 == thisLabel.Length, "s.Length + 1 == Label.Length");
                Debug.Assert(thisLabel.EndsWith(suffixLabel), "Label.EndsWith(s)");
                Debug.Assert(value != this, "value != this");
#endif
                _suffixLink = value;
            }
        }

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

        public Node<T> AddEdge(char key, Node<T> node)
        {
            node.Parent = this;
            Edges[key] = node;
            Debug.Assert(node.Label.StartsWith(key.ToString()));
            return this;
        }

        public Node<T> Parent { get; set; }

        private int _prevWordNumber = -1;
        private Node<T> _suffixLink;

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

        public Node<T> GetEdge(char activeEdgeChar)
        {
            if (_edges == null) return null;
            _edges.TryGetValue(activeEdgeChar, out var found);
            return found;
        }

    }
}