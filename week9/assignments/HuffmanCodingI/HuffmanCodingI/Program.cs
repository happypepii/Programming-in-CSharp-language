using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HuffmanCodingI.Tests")]

namespace HuffmanCodingI;

class Program
{
    static public void Main(string[] args)
    {
        // Argument count check
        if (args.Length != 1)
        {
            Console.Write("Argument Error");
            return;
        }

        try
        {
            // Open file as stream (streaming, O(1) memory)
            using FileStream fs = new FileStream(
                args[0],
                FileMode.Open,
                FileAccess.Read
            );

            // Build frequency table
            var tableBuilder = new TableBuilder();
            int[] freqTable = tableBuilder.BuildFrequencyTable(fs);

            // Build Huffman tree
            var treeBuilder = new TreeBuilder();
            Node? root = treeBuilder.BuildTree(freqTable);

            // Empty file → print nothing
            if (root == null)
                return;

            // Print tree
            var printer = new TreePrinter();
            printer.PrintTree(root);
        }
        catch
        {
            // Any IO / read error
            Console.Write("File Error");
        }
    }
}


class TableBuilder
{
    public int[] BuildFrequencyTable(Stream input)
    {
        int[] freq = new int[256];

        int b;
        while ((b = input.ReadByte()) != -1)
        {
            freq[b]++;
        }
        return freq;

    }
}


class TreeBuilder
{
    private int creationOrder = 0;

    public Node? BuildTree(int[] frequencyTable)
    {
        var nodes = BuildLeafNodes(frequencyTable);

        if (nodes.Count == 0)
            return null;

        if (nodes.Count == 1)
            return nodes[0];

        return BuildMergedTree(nodes);
    }
    private List<Node> BuildLeafNodes(int[] frequencyTable)
    {
        var nodes = new List<Node>();

        for (int i = 0; i < frequencyTable.Length; i++)
        {
            if (frequencyTable[i] > 0)
            {
                nodes.Add(new LeafNode(
                    (byte)i,
                    frequencyTable[i],
                    creationOrder++
                ));
            }
        }

        return nodes;
    }

    private Node BuildSingleNodeTree(Node leaf)
    {
        return new InnerNode(leaf, leaf, creationOrder++);
    }

    private Node BuildMergedTree(List<Node> nodes)
    {
        while (nodes.Count > 1)
        {
            nodes.Sort();

            var left = nodes[0];
            var right = nodes[1];

            nodes.RemoveRange(0, 2);

            var parent = new InnerNode(left, right, creationOrder++);
            nodes.Add(parent);
        }

        return nodes[0];
    }
}

class TreePrinter
{
    public void PrintTree(Node root)
    {
        PrintNode(root);
    }

    private void PrintNode(Node node)
    {
        if (node.IsLeaf)
        {
            PrintLeaf((LeafNode)node);
        }
        else
        {
            PrintInner((InnerNode)node);
        }
    }

    private void PrintLeaf(LeafNode leaf)
    {
        Console.Write($"*{leaf.Symbol}:{leaf.Weight}");
    }

    private void PrintInner(InnerNode inner)
    {
        Console.Write(inner.Weight);

        Console.Write(" ");
        PrintNode(inner.Left);

        Console.Write(" ");
        PrintNode(inner.Right);
    }
}




abstract class Node : IComparable<Node>
{
    public int Weight { get; }
    public int CreationOrder { get; }

    protected Node(int weight, int creationOrder)
    {
        Weight = weight;
        CreationOrder = creationOrder;
    }

    public abstract bool IsLeaf { get; }

    // for priority queue
    public int CompareTo(Node? other)
    {
        if (other == null) return 1;

        // 1. weight
        int cmp = Weight.CompareTo(other.Weight);
        if (cmp != 0) return cmp;

        // 2. if weights are the same, compare leaf > inner
        if (this.IsLeaf && !other.IsLeaf) return -1;
        if (!this.IsLeaf && other.IsLeaf) return 1;

        // 3. if both leaf => symbol
        if (this.IsLeaf && other.IsLeaf)
        {
            LeafNode thisLeaf = (LeafNode)this;
            LeafNode otherLeaf = (LeafNode)other;
            return thisLeaf.Symbol.CompareTo(otherLeaf.Symbol);
        }

        // 4. if both inner => creationOrder
        return CreationOrder.CompareTo(other.CreationOrder);
    }
}



class LeafNode : Node
{
    public byte Symbol { get; }

    public LeafNode(byte symbol, int weight, int creationOrder)
        : base(weight, creationOrder)
    {
        Symbol = symbol;
    }

    public override bool IsLeaf => true;
}



class InnerNode : Node
{
    public Node Left { get; }
    public Node Right { get; }

    public InnerNode(Node left, Node right, int creationOrder)
        : base(left.Weight + right.Weight, creationOrder)
    {
        Left = left;
        Right = right;
    }

    public override bool IsLeaf => false;
}