using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph : MonoBehaviour
{
    public static Graph Instance;
    List<Edge> edges = new List<Edge>();
    public List<Node> nodes = new List<Node>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        GameObject[] nodeObj = GameObject.FindGameObjectsWithTag("Node");
        Node node;

        foreach (GameObject n in nodeObj)
        {
            Destroy(n.GetComponent<Collider>());
            Destroy(n.GetComponent<Renderer>());

            node = n.GetComponent<Node>();
            nodes.Add(node);
            foreach (Node nextNode in node.toNode)
            {
                if (nextNode.toNode != null)
                {
                    AddEdge(node, nextNode);
                }
            }
        }
    }

    void AddEdge(Node from, Node to)
    {
        if (from != null && to != null)
        {
            Edge e = new Edge(from, to);
            edges.Add(e);
        }
    }

    public Node[] AStar(Node start, Node end)
    {
        if (start == null || end == null)
        {
            return null;
        }

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();
        float tentative_g_score = 0;
        bool tentative_is_better;

        start.SetG(0f);
        start.SetH(Distance(start, end));
        start.SetF(start.GetH());
        open.Add(start);

        while (open.Count > 0)
        {
            int i = LowestF(open);
            Node thisnode = open[i];
            if (thisnode == end)  //path found
            {
                return ReconstructPath(start, end);
            }
            open.RemoveAt(i);
            closed.Add(thisnode);

            //foreach (Edge e in thisnode.edgelist)
            foreach (Node neighbour in thisnode.toNode)
            {
                //neighbour = e.endNode;
                neighbour.SetG(thisnode.GetG() + Distance(thisnode, neighbour));

                if (closed.IndexOf(neighbour) > -1)
                    continue;

                tentative_g_score = thisnode.GetG() + Distance(thisnode, neighbour);

                if (open.IndexOf(neighbour) == -1)
                {
                    open.Add(neighbour);
                    tentative_is_better = true;
                }
                else if (tentative_g_score < neighbour.GetG())
                {
                    tentative_is_better = true;
                }
                else
                    tentative_is_better = false;

                if (tentative_is_better)
                {
                    neighbour.SetCameFrom(thisnode);
                    neighbour.SetG(tentative_g_score);
                    neighbour.SetH(Distance(thisnode, end));
                    neighbour.SetF(neighbour.GetG() + neighbour.GetH());
                }
            }

        }
        return null;
    }

    public Node[] ReconstructPath(Node startId, Node endId)
    {
        List<Node> pathList = new List<Node>();
        pathList.Clear();
        pathList.Add(endId);

        var p = endId.GetCameFrom();
        while (p != startId && p != null)
        {
            pathList.Insert(0, p);
            p = p.GetCameFrom();
        }
        pathList.Insert(0, startId);

        return pathList.ToArray();
    }

    float Distance(Node a, Node b)
    {
        if (a == null)
        {
            Debug.Log(a.name);
        }

        if (b == null)
        {
            Debug.Log(b.name);
        }

        float dx = a.transform.position.x - b.transform.position.x;
        float dy = a.transform.position.y - b.transform.position.y;
        float dz = a.transform.position.z - b.transform.position.z;

        float dist = dx * dx + dy * dy + dz * dz;
        return dist;
    }

    int LowestF(List<Node> l)
    {
        float lowestf = 0;
        int count = 0;
        int iteratorCount = 0;

        for (int i = 0; i < l.Count; i++)
        {
            if (i == 0)
            {
                lowestf = l[i].GetF();
                iteratorCount = count;
            }
            else if (l[i].GetF() <= lowestf)
            {
                lowestf = l[i].GetF();
                iteratorCount = count;
            }
            count++;
        }
        return iteratorCount;
    }
}
