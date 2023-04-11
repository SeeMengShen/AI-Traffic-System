using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph : MonoBehaviour
{
    public static Graph Instance;
    List<Edge> edges = new List<Edge>();
    List<Node> nodes = new List<Node>();
    //List<Node> pathList = new List<Node>();

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

        foreach(GameObject n in nodeObj)
        {
            node = n.GetComponent<Node>();
            nodes.Add(node);

            foreach(Node nextNode in node.toNode)
            {
                if(nextNode.toNode !=  null)
                {
                    AddEdge(node, nextNode);
                }                
            }
        }
    }

    /*    public void AddNode(GameObject id, bool removeRenderer = true, bool removeCollider = true)
        {
            Node node = new Node(id);
            nodes.Add(node);

            //remove colliders and mesh renderer
            if (removeCollider)
                GameObject.Destroy(id.GetComponent<Collider>());
            if (removeRenderer)
                GameObject.Destroy(id.GetComponent<Renderer>());
        }*/

    //public void AddEdge(GameObject fromNode, GameObject toNode)
    void AddEdge(Node from, Node to)
    {
        /*Node from = findNode(fromNode);
        Node to = findNode(toNode);*/

        if (from != null && to != null)
        {
            Edge e = new Edge(from, to);
            edges.Add(e);
            //from.edgelist.Add(e);
        }
    }

/*    Node findNode(GameObject id)
    {
        foreach (Node n in nodes)
        {
            if (n.getId() == id)
                return n;
        }
        return null;
    }


    public int getPathLength()
    {
        return pathList.Count;
    }

    public GameObject getPathPoint(int index)
    {
        return pathList[index].getId();
    }

    public void printPath()
    {
        foreach (Node n in pathList)
        {
            Debug.Log(n.getId().name);
        }
    }*/


    //public Node[] AStar(GameObject startId, GameObject endId)
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

        start.g = 0;
        start.h = distance(start, end);
        start.f = start.h;
        open.Add(start);

        while (open.Count > 0)
        {
            int i = lowestF(open);
            Node thisnode = open[i];
            if (thisnode == end)  //path found
            {
                return reconstructPath(start, end);
            }
            open.RemoveAt(i);
            closed.Add(thisnode);

            //foreach (Edge e in thisnode.edgelist)
            foreach (Node neighbour in thisnode.toNode)
            {
                //neighbour = e.endNode;
                neighbour.g = thisnode.g + distance(thisnode, neighbour);

                if (closed.IndexOf(neighbour) > -1)
                    continue;

                tentative_g_score = thisnode.g + distance(thisnode, neighbour);

                if (open.IndexOf(neighbour) == -1)
                {
                    open.Add(neighbour);
                    tentative_is_better = true;
                }
                else if (tentative_g_score < neighbour.g)
                {
                    tentative_is_better = true;
                }
                else
                    tentative_is_better = false;

                if (tentative_is_better)
                {
                    neighbour.setCameFrom(thisnode);
                    neighbour.g = tentative_g_score;
                    neighbour.h = distance(thisnode, end);
                    neighbour.f = neighbour.g + neighbour.h;
                }                
            }

        }
        return null;
    }

    public Node[] reconstructPath(Node startId, Node endId)
    {
        List<Node> pathList = new List<Node>();
        pathList.Clear();
        pathList.Add(endId);

        var p = endId.getCameFrom();
        while (p != startId && p != null)
        {
            pathList.Insert(0, p);
            p = p.getCameFrom();
        }
        pathList.Insert(0, startId);

        return pathList.ToArray();
    }

    float distance(Node a, Node b)
    {
        float dx = a.xPos - b.xPos;
        float dy = a.yPos - b.yPos;
        float dz = a.zPos - b.zPos;
        float dist = dx * dx + dy * dy + dz * dz;
        return (dist);
    }

    int lowestF(List<Node> l)
    {
        float lowestf = 0;
        int count = 0;
        int iteratorCount = 0;

        for (int i = 0; i < l.Count; i++)
        {
            if (i == 0)
            {
                lowestf = l[i].f;
                iteratorCount = count;
            }
            else if (l[i].f <= lowestf)
            {
                lowestf = l[i].f;
                iteratorCount = count;
            }
            count++;
        }
        return iteratorCount;
    }

    public void debugDraw()
    {
        //draw edges
        for (int i = 0; i < edges.Count; i++)
        {
            Debug.DrawLine(edges[i].startNode.transform.position, edges[i].endNode.transform.position, Color.red);
            Vector3 to = (edges[i].startNode.transform.position - edges[i].endNode.transform.position) * 0.05f;
            Debug.DrawRay(edges[i].endNode.transform.position, to, Color.blue);
        }
    }

}
