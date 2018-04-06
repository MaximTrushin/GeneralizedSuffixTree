using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;

namespace GeneralizedSuffixTree.Tests.Ukkonen
{
    public static class UTreeHelper
    {
        

        public static void CountNodes<T>(this UkkonenTrie<T> tree, out int leaves, out int internalNodes,
            out int edges, out int sLinks)
        {
            leaves = CountLeaves(tree.Root);
            internalNodes = CountInternalNodes(tree.Root, tree.Root);
            edges = CountEdges(tree.Root);
            sLinks = CountSLinks(tree.Root, tree.Root);
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
                result += CountLeaves(child.Target);
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
                result += CountInternalNodes(child.Target);
            }

            return result;
        }

        private static int CountEdges<T>(Node<T> x)
        {
            var result = 0;
            foreach (var child in x.Edges.Values)
            {
                result++;
                result += CountEdges(child.Target);
            }

            return result;
        }

        private static int CountSLinks<T>(Node<T> x, Node<T> treeRoot)
        {
            var result = 0;
            if (x.Suffix != null && x.Suffix != x && x.Suffix != treeRoot)
            {
                result++;
            }
            foreach (var child in x.Edges.Values)
            {
                result += CountSLinks(child.Target, treeRoot);
            }

            return result;
        }

    }

}
