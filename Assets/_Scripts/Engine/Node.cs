using System; 
using UnityEngine;

public class Node : IComparable<Node>
{
  
    public int X { get; private set; }
    public int Y { get; private set; }

  
    public bool IsWalkable { get; set; }

    public float G { get; set; } 
    public float H { get; set; } 
    public float F => G + H;    


    public Node Parent { get; set; }

    public Node(int x, int y, bool isWalkable)
    {
        X = x;
        Y = y;
        IsWalkable = isWalkable;
    }


    public void Reset()
    {
        G = float.MaxValue;
        H = 0;
        Parent = null;
    }

    public int CompareTo(Node other)
    {
        if (other == null) return 1;


        int compare = F.CompareTo(other.F);


        if (compare == 0)
        {
            compare = H.CompareTo(other.H);
        }

        return compare;
    }
}