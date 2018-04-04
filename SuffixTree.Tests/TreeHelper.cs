using System.Diagnostics;
using System.Text;
using NUnit.Framework;
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


        public static void CountNodes<T>(this GeneralizedSuffixTree<T> tree, out int leaves, out int internalNodes,
            out int edges, out int sLinks)
        {
            leaves = CountLeaves(tree.Root);
            internalNodes = CountInternalNodes(tree.Root, tree.Root);
            edges = CountEdges(tree.Root);
            sLinks = CountSLinks(tree.Root);
        }

        private static int CountLeaves<T>(Node<T> x)
        {
            if (x.Edges.Count == 0)
            {
                return 1;
            }

            var result = 0;
            foreach (var child in x.Edges.Values)
            {
                result += CountLeaves(child);
            }

            return result;
        }

        private static int CountInternalNodes<T>(Node<T> x, Node<T> root = null)
        {
            var result = 0;
            if (x != root
                && x.Edges.Count > 0)
            {
                result++;
            }
            
            foreach (var child in x.Edges.Values)
            {
                result += CountInternalNodes(child);
            }

            return result;
        }

        private static int CountEdges<T>(Node<T> x)
        {
            var result = 0;
            foreach (var child in x.Edges.Values)
            {
                result++;
                result += CountEdges(child);
            }

            return result;
        }

        private static int CountSLinks<T>(Node<T> x)
        {
            var result = 0;
            if (x.SuffixLink != null)
            {
                result++;
            }
            foreach (var child in x.Edges.Values)
            {
                result += CountSLinks(child);
            }

            return result;
        }

    }
}
