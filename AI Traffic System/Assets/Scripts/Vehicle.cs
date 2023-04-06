using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public Node[] path;
    public int currentWP = 0;
    public float rotationSpeed;
    public float speed;
    public float accuracy;
    public Node startNode, endNode;

    // Start is called before the first frame update
    void Start()
    {
        path = Graph.Instance.AStar(startNode, endNode);

        if(path == null)
        {
            Debug.Log("null");
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if there is no path or at the end don't do anything
        if (currentWP == path.Length)
        {
            Debug.Log("End");
            Destroy(this);
            return;
        }

        /*
        // the node we are closest to at this moment
        currentNode = graph.getPathPoint(currentWP);*/

        // if we are close enough to the current waypoint, move to next
        if (Vector3.Distance(path[currentWP].transform.position, transform.position) < accuracy)
        {
            currentWP++;
        }

        // if we are not at the end of path
        if (currentWP < path.Length)
        {
            //Move
            Vector3 direction = path[currentWP].transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

            transform.Translate(0.0f, 0.0f, Time.deltaTime * speed);
        }        
    }
}
