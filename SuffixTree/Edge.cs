using System;

namespace SuffixTree
{
    public class Edge<T>
    {
        public Edge(int start, int end, string source, Node<T> target)
        {
            Start = start;
            End = end;
            Source = source;
            Target = target;
        }

        public int Start { get; internal set; }
        public int End { get; } //End not included in the range [start, end)
        public string Source { get; }
        public Node<T> Target { get; }

        //End not included in the range [start, end)
        public string Label => Source.Substring(Start, Math.Min(End, Source.Length) - Start);

        public int EdgeLength(int position)
        {
            //return Math.Min(End, position + 1) - Start;
            return (End != int.MaxValue?End:Source.Length) - Start;
        }
    }
}