using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using System;
using UnityEditor;

[DisallowMultipleComponent]
public class Node : MonoBehaviour
{
    private float f, g, h;
    private Node cameFrom;

    public enum NodeType
    {
        normal,
        start,
        end
    }

    public NodeType type;

    [HideInInspector]
    public List<Node> toNode;

    private Connection connectionParent;

    void Awake()
    {
        if (type == NodeType.end)
        {
            toNode.Clear();
        }
    }

    public Connection GetConnectionParent()
    {
        return connectionParent;
    }

    public float GetF()
    {
        return f;
    }

    public float GetG()
    {
        return g;
    }

    public float GetH()
    {
        return h;
    }

    public Node GetCameFrom()
    {
        return cameFrom;
    }

    public void SetConnectionParent(Connection parent)
    {
        connectionParent = parent;
    }

    public bool HasSameConnectionParent(Node other)
    {
        return connectionParent == other.GetConnectionParent();
    }

    public void SetF(float newF)
    {
        f = newF;
    }

    public void SetG(float newG)
    {
        g = newG;
    }

    public void SetH(float newH)
    {
        h = newH;
    }

    public void SetCameFrom(Node cF)
    {
        cameFrom = cF;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        foreach (Node n in toNode)
        {
            if (n != null)
            {
                // Draws a blue line from this transform to the target
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, n.transform.position);
            }
        }
    }
}