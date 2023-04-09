using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunctionManager : MonoBehaviour
{
    public float radius;
    private float sqrRadius;
    private Vehicle[] vehicles;
    private Vector3 offset;
    private Queue<Vehicle> queue;
    private Vehicle currentMoving;
    private bool clearToMove;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Vehicle");
        for (int i = 0; i < gameObjects.Length; i++)
        {
            vehicles[i] = gameObjects[i].GetComponent<Vehicle>();
        }
        sqrRadius = radius * radius;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Vehicle v in vehicles)
        {
            if (withinRange(v.transform.position))
            {
                if(!queue.Contains(v))
                {
                    queue.Enqueue(v);
                    v.switchState(Vehicle.StateList.stop);
                }                
            }
        }

        if (queue.Count > 0)
        {
            if (currentMoving != null)
            {
                if (withinRange(currentMoving.transform.position))
                {
                    clearToMove = false;
                }
                else
                {
                    clearToMove = true;
                    currentMoving = null;
                }
            }

            if (clearToMove)
            {
                currentMoving = queue.Dequeue();
                currentMoving.switchState(Vehicle.StateList.moving);
            }
        }
    }

    bool withinRange(Vector3 other)
    {
        offset = transform.position - other;

        return offset.sqrMagnitude < sqrRadius;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gameObject.transform.position, radius);
    }
}
