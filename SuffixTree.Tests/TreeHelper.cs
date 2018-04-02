using System.Diagnostics;
using System.Text;
using SuffixTree;

namespace GeneralizedSuffixTree.Tests
{
    public static class TreeHelper
    {
        public static string PrintTree<T>(this GeneralizedSuffixTree<T> tree)
        {
            var sb = new StringBuilder();
            sb.Append("leaves:{");
            PrintLeaves(tree.Root, sb);
            sb.AppendLine("}");
            sb.Append("internal nodes:{");
            PrintInternalNodes(tree.Root, sb, tree.Root);
            sb.AppendLine("}");
            sb.AppendLine("edges:{");
            PrintEdges(tree.Root, sb);
            sb.AppendLine("links:{");
            PrintSLinks(tree.Root, sb);
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void PrintLeaves<T>(Node<T> x, StringBuilder sb)
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

        private static void PrintInternalNodes<T>(Node<T> x, StringBuilder sb, Node<T> root = null)
        {
            if (x != root
                && x.Edges.Count > 0)
            {
                sb.Append(x.Number + ",");
            }

            Debug.Assert(x != null, nameof(x) + " != null");
            foreach (var child in x.Edges.Values)
            {
                PrintInternalNodes(child, sb);
            }
        }

        private static void PrintEdges<T>(Node<T> x, StringBuilder sb)
        {
            foreach (var child in x.Edges.Values)
            {
                sb.AppendLine(x.Number + "->"
                        + child.Number + "=" + child.Label);
                PrintEdges(child, sb);
            }
        }

        private static void PrintSLinks<T>(Node<T> x, StringBuilder sb)
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
