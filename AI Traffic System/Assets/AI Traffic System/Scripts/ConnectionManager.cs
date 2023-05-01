using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[ExecuteInEditMode]
public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    public List<Node> entryNodes = new List<Node>();
    public List<Node> exitNodes = new List<Node>();

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

        RefreshNodes();
        ClearConnections();
        GenerateConnections();
    }

    public void RefreshNodes()
    {
        entryNodes.Clear();
        exitNodes.Clear();

        Connection[] connections = GetComponentsInChildren<Connection>();

        foreach (Connection c in connections)
        {
            foreach (Node entryN in c.entryNodes)
            {
                entryN.SetConnectionParent(c);
                entryNodes.Add(entryN);
            }

            foreach (Node exitN in c.exitNodes)
            {
                exitN.SetConnectionParent(c);
                exitNodes.Add(exitN);
            }
        }
    }

    public void GenerateConnections()
    {
        foreach (Node exit in exitNodes)
        {
            float shortestDist = float.MaxValue;
            int shortestIndex = 0;

            for (int i = 0; i < entryNodes.Count; i++)
            {
                if (!entryNodes[i].HasSameConnectionParent(exit))
                {
                    if (SqrDistanceBetween(entryNodes[i], exit) < shortestDist)
                    {
                        shortestIndex = i;
                        shortestDist = SqrDistanceBetween(entryNodes[i], exit);
                    }
                }               
            }

            if (!exit.toNode.Contains(entryNodes[shortestIndex]))
            {
                exit.toNode.Add(entryNodes[shortestIndex]);
            }
        }
    }

    public void ClearConnections()
    {
        foreach (Node exit in exitNodes)
        {
            exit.toNode.Clear();
        }
    }

    private float SqrDistanceBetween(Node n1, Node n2)
    {
        return (n1.transform.position - n2.transform.position).sqrMagnitude;
    }
}
