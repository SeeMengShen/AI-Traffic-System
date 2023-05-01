using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartEndManager : MonoBehaviour
{
    public static StartEndManager Instance;
    public List<Node> startNodes = new List<Node>();
    public List<Node> endNodes = new List<Node>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        Node[] nodes = GetComponentsInChildren<Node>();

        // Classify each type of node into respective list
        foreach (Node node in nodes)
        {
            if (node.type == Node.NodeType.start)
            {
                startNodes.Add(node);
            }
            else if (node.type == Node.NodeType.end)
            {
                endNodes.Add(node);
            }
        }
    }

    public List<Node> GetStartNodes()
    {
        return startNodes;
    }

    public List<Node> GetEndNodes()
    {
        return endNodes;
    }

    public Node RandomStartNode()
    {
        return startNodes[Random.Range(0, startNodes.Count)];
    }

    public Node RandomEndNode()
    {
        return endNodes[Random.Range(0, endNodes.Count)];
    }
}
