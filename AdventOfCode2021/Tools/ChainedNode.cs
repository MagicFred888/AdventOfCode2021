using System.Diagnostics;
using System.Text;

namespace AdventOfCode2021.Tools;

public sealed class ChainedNode
{
    public long Value { get; init; }
    public ChainedNode Next { get; set; }
    public ChainedNode Previous { get; set; }

    public ChainedNode(long value)
    {
        Value = value;
        Next = this;
        Previous = this;
    }

    public void InsertBefore(ChainedNode newNode)
    {
        ChainedNode insertStartNode = newNode;
        ChainedNode insertEndNode = newNode.Previous;

        insertStartNode.Previous = Previous;
        Previous.Next = insertStartNode;

        Previous = insertEndNode;
        insertEndNode.Next = this;
    }

    public void InsertAfter(ChainedNode newNode)
    {
        ChainedNode insertStartNode = newNode;
        ChainedNode insertEndNode = newNode.Previous;

        insertEndNode.Next = Next;
        Next.Previous = insertEndNode;

        Next = insertStartNode;
        insertStartNode.Previous = this;
    }

    public ChainedNode RemovePreviousNode(long nbrToRemove = 1)
    {
        ChainedNode lastNodeToRemove = Previous;
        ChainedNode firstNodeToRemove = this.MoveBackward(nbrToRemove);

        // Remove section
        Previous = firstNodeToRemove.Previous;
        firstNodeToRemove.Previous.Next = this;

        // Close removed section
        firstNodeToRemove.Previous = lastNodeToRemove;
        lastNodeToRemove.Next = firstNodeToRemove;

        // Return start point of removed sequence
        return firstNodeToRemove;
    }

    public void Remove()
    {
        (Previous.Next, Next.Previous) = (Next, Previous);
        Next = this;
        Previous = this;
    }

    public ChainedNode RemoveNextNode(long nbrToRemove = 1)
    {
        ChainedNode firstNodeToRemove = Next;
        ChainedNode lastNodeToRemove = this.MoveForward(nbrToRemove);

        // Remove section
        Next = lastNodeToRemove.Next;
        lastNodeToRemove.Next.Previous = this;

        // Close removed section
        firstNodeToRemove.Previous = lastNodeToRemove;
        lastNodeToRemove.Next = firstNodeToRemove;

        // Return start point of removed sequence
        return firstNodeToRemove;
    }

    public bool ChainContain(long nodeValue)
    {
        ChainedNode searchNode = this;
        do
        {
            if (searchNode.Value == nodeValue) return true;
            searchNode = searchNode.Next;
        } while (searchNode != this);
        return false;
    }

    public ChainedNode MoveForward(long nbrOfStep)
    {
        return nbrOfStep == 0 ? this : Next.MoveForward(nbrOfStep - 1);
    }

    public ChainedNode MoveBackward(long nbrOfStep)
    {
        return nbrOfStep == 0 ? this : Previous.MoveBackward(nbrOfStep - 1);
    }

    public override string ToString()
    {
        return $"{Previous.Value} <- ({Value}) -> {Next.Value}";
    }

    public void DebugPrint(ChainedNode? highlight1, ChainedNode? highlight2 = null, ChainedNode? highlight3 = null)
    {
        Dictionary<ChainedNode, string> highlight = [];
        if (highlight1 != null)
        {
            highlight.Add(highlight1, "({0})");
        }
        if (highlight2 != null)
        {
            highlight.Add(highlight2, "[{0}]");
        }
        if (highlight3 != null)
        {
            highlight.Add(highlight3, "{{0}}");
        }

        // Debugging method to print the current state of the game
        ChainedNode tmp = this;
        StringBuilder stringBuilder = new();
        do
        {
            string formatedResult = highlight.TryGetValue(tmp, out string? value) ? string.Format(value, tmp.Value) : tmp.Value.ToString();
            stringBuilder.Append(formatedResult);
            stringBuilder.Append("  ");
            tmp = tmp.Next;
        } while (tmp != this);
        Debug.WriteLine(stringBuilder.ToString().Trim());
    }
}