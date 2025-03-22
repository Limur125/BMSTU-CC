using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class Node(string value, Node? leftNode = null, Node? rightNode = null)
    {
        public int index;
        public string value = value;
        public Node? leftChild = leftNode;
        public Node? rightChild = rightNode;
        public bool nullable = false;
        public HashSet<int> firstpos = [], lastpos = [];
        public Dictionary<int, HashSet<int>> followpos = [];
        public DFA CreateDFA()
        {
            SetIndex(1);
            NullableFirstposLastpos();
            Followpos();
            Dictionary<int, HashSet<int>> treeFollowpos = [];
            treeFollowpos = GetTreeFollowpos(treeFollowpos);
            Dictionary<int, string> indexedStates = [];
            indexedStates = GetIndexedStates(indexedStates);
            return new DFA(this, indexedStates, treeFollowpos);
        }

        public int SetIndex(int index)
        {
            if (leftChild is null && rightChild is null)
            {
                this.index = index;
                index++;
            }
            else
            {
                if (leftChild is not null)
                    index = leftChild.SetIndex(index);
                if (rightChild is not null)
                    index = rightChild.SetIndex(index);
            }
            return index;
        }

        public Dictionary<int, HashSet<int>> GetTreeFollowpos(Dictionary<int, HashSet<int>> res)
        {
            foreach (var pair in followpos)
            {
                if (res.TryGetValue(pair.Key, out HashSet<int>? value))
                {
                    res[pair.Key] = [.. value, .. pair.Value];
                }
                else
                {
                    res[pair.Key] = [ .. pair.Value];
                }
            }
            if (leftChild is not null)
                res = leftChild.GetTreeFollowpos(res);
            if (rightChild is not null)
                res = rightChild.GetTreeFollowpos(res);
            return res;
        }

        private Dictionary<int, string> GetIndexedStates(Dictionary<int, string> res)
        {
            if (leftChild is null && rightChild is null)
            {
                res[index] = value;
            }
            else
            {
                if (leftChild is not null)
                    res = leftChild.GetIndexedStates(res);
                if (rightChild is not null)
                    res = rightChild.GetIndexedStates(res);
            }
            return res;
        }

        public virtual void NullableFirstposLastpos()
        {
            if (string.IsNullOrEmpty(value))
            {
                nullable = true;
            }
            else
            {
                nullable = false;
                firstpos.Add(index);
                lastpos.Add(index);
            }
        }
        public virtual void Followpos() 
        {
            leftChild?.Followpos();
            rightChild?.Followpos();
        }
    }

    public class NodeOperator(string value, Node? leftNode = null, Node? rightNode = null) : Node(value, leftNode, rightNode)
    {

    }

    public class NodeStar(Node? leftNode = null, Node? rightNode = null) : Node(Consts.starSymbol, leftNode, rightNode)
    {
        public override void NullableFirstposLastpos()
        {
            leftChild?.NullableFirstposLastpos();
            nullable = true;
            firstpos = [.. leftChild!.firstpos];
            lastpos = [.. leftChild!.lastpos];
        }

        public override void Followpos()
        {
            leftChild?.Followpos();
            foreach (var item in leftChild!.lastpos)
            {
                followpos[item] = [.. leftChild.firstpos];
            }
        }

    }

    public class NodeOr(Node ? leftNode = null, Node ? rightNode = null) : Node(Consts.orSymbol, leftNode, rightNode)
    {
        public override void NullableFirstposLastpos()
        {
            leftChild?.NullableFirstposLastpos();
            rightChild?.NullableFirstposLastpos();
            nullable = leftChild!.nullable || rightChild!.nullable;
            firstpos = [.. leftChild!.firstpos, .. rightChild!.firstpos];
            lastpos = [.. leftChild!.lastpos, .. rightChild!.lastpos];
        }
    }

    public class NodeAnd(Node? leftNode = null, Node? rightNode = null) : Node(Consts.andSymbol, leftNode, rightNode)
    {
        public override void NullableFirstposLastpos()
        {
            leftChild?.NullableFirstposLastpos();
            rightChild?.NullableFirstposLastpos();

            nullable = leftChild!.nullable && rightChild!.nullable;
            
            firstpos = [.. leftChild.firstpos];
            if (leftChild.nullable)
                firstpos = [.. firstpos, .. rightChild!.firstpos];

            lastpos = [.. rightChild!.lastpos];
            if (rightChild.nullable)
                lastpos = [.. lastpos, .. leftChild!.lastpos];
        }

        public override void Followpos()
        {
            leftChild?.Followpos();
            rightChild?.Followpos();
            foreach (var item in leftChild!.lastpos)
                followpos[item] = [.. rightChild!.firstpos];
        }
    }
}
