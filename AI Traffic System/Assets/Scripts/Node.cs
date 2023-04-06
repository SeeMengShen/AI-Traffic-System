using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

public class Node : MonoBehaviour
{
    //public List<Edge> edgelist = new List<Edge>();
    //public Node path = null;
	private GameObject id;
	public float xPos;
	public float yPos;
	public float zPos;
	public float f, g, h;
	private Node cameFrom;
	public Node[] toNode;

    /*public Node(GameObject i)
	{
		id = i;
		xPos = i.transform.position.x;
		yPos = i.transform.position.y;
		zPos = i.transform.position.z;
		//path = null;
	}*/

    void Awake()
    {
		id = gameObject;
        xPos = transform.position.x;
        yPos = transform.position.y;
        zPos = transform.position.z;
    }

    public GameObject getId()
	{
		return id;
	}

	public Node getCameFrom()
	{
		return cameFrom;
	}

	public void setCameFrom(Node cF)
	{
		this.cameFrom = cF;
	}

    void OnDrawGizmosSelected()
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
