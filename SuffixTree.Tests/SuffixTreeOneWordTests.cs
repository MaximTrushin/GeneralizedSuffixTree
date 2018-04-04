using System;
using NUnit.Framework;
using SuffixTree;
using static GeneralizedSuffixTree.Tests.TreeHelper;

namespace GeneralizedSuffixTree.Tests
{
    class SuffixTreeOneWordTests
    {
        [Test]
        public void abracadabra()
        {
            const string s = "abracadabra";
            var tree = new GeneralizedSuffixTree<int>(0);
            tree.AddWord(s, 1);
            var stree = tree.PrintTree().Replace("\0", "");
            Console.WriteLine(stree);

            var expected = @"leaves:{1,5,7,2,3,6,8,}
internal nodes:{4,9,10,11,}
edges:{
0->4=a
4->9=bra
9->1=cadabra
4->5=cadabra
4->7=dabra
0->10=bra
10->2=cadabra
0->11=ra
11->3=cadabra
0->6=cadabra
0->8=dabra
links:{
9->10
1->2
5->6
7->8
10->11
2->3
3->5
6->7
}
";

            Assert.AreEqual(expected, stree);


        }
    }
}
