using System;

namespace SuffixTree
{
    public class Edge<T>
    {
        public Edge(string label, Node<T> target)
        {
            Label = label;
            Target = target;
        }

        public string Label { get; internal set; }
        public Node<T> Target { get; }
    }
}