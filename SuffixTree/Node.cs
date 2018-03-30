using System.Collections.Generic;

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
            //if (Start > 1 && !value.Equals(_previouslyAssignedValue))
            //{
            //    Data.Add(value);
            //    _previouslyAssignedValue = value;
            //}
            
        }
    }
}