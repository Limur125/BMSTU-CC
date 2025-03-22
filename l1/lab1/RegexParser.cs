using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace lab1
{
    public class RegexParser
    {
        private int i = 0;
        private int stackBrackets = 0;

        public Node parseExpression(string regexStr)
        {
            regexStr = regexStr.Replace(" ", "");
            return parse(regexStr);
        }

        Node parse(string regexStr)
        {
            var nodeArr = _createNodeLst(regexStr);
            return _buildTreeNode(nodeArr);
        }
        List<Node> _createNodeLst(string regexStr)
        {

            List<Node> nodeLst = [];
            while (i < regexStr.Length && regexStr[i].ToString() != Consts.closeSymbol)
            {
                var newNode = createNode(regexStr);
                if (newNode == null)
                    break;
                nodeLst.Add(newNode);
                i++;
            }
            return nodeLst;
        }
        Node? createNode(string regexStr)
        {
            char elem = regexStr[i];

            if (!Consts.allControlSymbols.Contains(elem.ToString()))  // not (,),*,|,•
                return new Node(value: elem.ToString());
            if (Consts.allOperators.Contains(elem.ToString()))  // *,|,•,+
                return new NodeOperator(elem.ToString());
            if (elem.ToString() == Consts.openSymbol)
            {
                i++;
                return parse(regexStr);
            }
            if (elem.ToString() == Consts.closeSymbol)
                return null;
            throw new Exception("Не знаю как до этого добрались но что-то пошло не так");
        }
        Node _buildTreeNode(List<Node> nodeLst)
        {
            nodeLst = _parseStar(nodeLst);
            nodeLst = _parsePlus(nodeLst);
            nodeLst = _parseAnd(nodeLst);
            nodeLst = _parseOr(nodeLst);

            if (nodeLst.Count != 1)
                throw new Exception("Ошибка в процессе построения дерева: больше, чем 1 элемент в массиве");

            return nodeLst[0];
        }
        List<Node> _parseStar(List<Node> nodeLst)
        {
            List<Node> updLst = [];
            for (int i = 0; i < nodeLst.Count; i++)
            {
                var node = nodeLst[i];

                if (node is NodeOperator && node.value == Consts.starSymbol)
                {
                    if (i == 0)
                        throw new Exception("Ошибка: неверная постановка символа *");
                    var nodeLeft = updLst.Last();
                    updLst.Remove(nodeLeft);
                    node = new NodeStar(leftNode: nodeLeft);
                }
                updLst.Add(node);
            }
            return updLst;
        }

        List<Node> _parsePlus(List<Node> nodeLst)
        {
            List<Node> updLst = [];
            for (int i = 0; i < nodeLst.Count; i++)
            {
                var node = nodeLst[i];

                if (node is NodeOperator && node.value == Consts.plusSymbol)
                {
                    if (i == 0)
                        throw new Exception("Ошибка: неверная постановка символа +");
                    var nodeLeft = updLst.Last();
                    updLst.Remove(nodeLeft);
                    node = new NodeAnd(leftNode: nodeLeft, rightNode: new NodeStar(leftNode: nodeLeft));
                }
                updLst.Add(node);
            }
            return updLst;
        }


        List<Node> _parseAnd(List<Node> nodeLst)
        {
            var i = 0;
            List<Node> updLst = [];
            var isPreviousNode = false;

            while (i < nodeLst.Count)
            {
                Node node = nodeLst[i];
                if (node is NodeOperator && node.value == Consts.andSymbol)
                {
                    if (i == 0 || i == (nodeLst.Count - 1)
                        || (nodeLst[i - 1] is NodeOperator && (new string[] { Consts.orSymbol, Consts.andSymbol, Consts.openSymbol }).Contains(nodeLst[i - 1].value))
                        || (nodeLst[i + 1] is NodeOperator && (new string[] { Consts.orSymbol, Consts.andSymbol, Consts.closeSymbol, Consts.starSymbol, Consts.plusSymbol }).Contains(nodeLst[i + 1].value)))
                        throw new Exception("Ошибка: неверная постановка символа .");
                    var nodeLeft = updLst.Last();
                    updLst.Remove(nodeLeft);
                    node = new NodeAnd(leftNode: nodeLeft, rightNode: nodeLst[i + 1]);
                    i++;
                    isPreviousNode = false;
                }
                else if (node is not NodeOperator)
                {
                    if (isPreviousNode)
                    {
                        var nodeLeft = updLst.Last();
                        updLst.Remove(nodeLeft);
                        node = new NodeAnd(nodeLeft, node);
                    }
                    isPreviousNode = true;
                }
                else 
                {
                    isPreviousNode = false;
                }
                updLst.Add(node);
                i++;
            }
            return updLst;

        }
        List<Node> _parseOr(List<Node> nodeLst) {
            var i = 0;
            List<Node> updLst = [];

            while (i < nodeLst.Count)
            {
                var node = nodeLst[i];

                if (node.value == Consts.orSymbol)
                {
                    if (i == 0 || i == (nodeLst.Count - 1)
                        || (nodeLst[i - 1] is NodeOperator && (new string[] { Consts.orSymbol, Consts.andSymbol, Consts.openSymbol }).Contains(nodeLst[i - 1].value))
                        || (nodeLst[i + 1] is NodeOperator && (new string[] { Consts.orSymbol, Consts.andSymbol, Consts.closeSymbol, Consts.starSymbol, Consts.plusSymbol }).Contains(nodeLst[i + 1].value)))
                        throw new Exception("Ошибка: неверная постановка символа |");

                    var nodeLeft = updLst.Last();
                    updLst.Remove(nodeLeft);
                    node = new NodeOr(nodeLeft, nodeLst[i + 1]);
                    i++;
                }
                updLst.Add(node);
                i++;
            }
            return updLst;
        }
    }
}
