using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace lab1
{
    public class RegexTree
    {
        public Node? root;
        public Node? leftChild;
        public Node? rightChild;

        void add(Node node)
        {
            if (root is null)
                root = node;
            else if (root.leftChild is null)
                root.leftChild = node;
            else if (root.rightChild is null)
                root.rightChild = node;
        }

        static RegexTree create(Node rootNode, Node leftNode, Node rightNode = null)
        {
            var tree = new RegexTree
            {
                root = rootNode
            };
            tree.root.leftChild = leftNode;
            tree.root.rightChild = rightNode;

            return tree;
        }
    }
}
