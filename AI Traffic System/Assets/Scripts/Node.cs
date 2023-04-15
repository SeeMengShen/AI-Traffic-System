using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

[DisallowMultipleComponent]
public class Node : MonoBehaviour
{
	//public List<Edge> edgelist = new List<Edge>();
	//public Node path = null;
	//private GameObject id;
	/*	public float xPos;
		public float yPos;
		public float zPos;*/
	private float f, g, h;
	private Node cameFrom;

	[Tooltip("Set the available route for this node")]
	public Node[] toNode;

    /*public Node(GameObject i)
	{
		id = i;
		xPos = i.transform.position.x;
		yPos = i.transform.position.y;
		zPos = i.transform.position.z;
		//path = null;
	}*/

/*    void Awake()
    {
        xPos = transform.position.x;
        yPos = transform.position.y;
        zPos = transform.position.z;
    }*/

    /*public GameObject getId()
	{
		return id;
	}*/

	public float getF()
	{
		return f;
	}

	public float getG()
	{
		return g;
	}

	public float getH()
	{
		return h;
	}

	public Node GetCameFrom()
	{
		return cameFrom;
	}

	public void setF(float newF)
	{
		f = newF;
	}

	public void setG(float newG)
	{
		g = newG;
	}

	public void setH(float newH)
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