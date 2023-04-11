using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class JunctionManager : MonoBehaviour
{
    public float radius;
    private float sqrRadius;
    private List<Vehicle> vehicles = new List<Vehicle>();
    private Vector3 offset;
    public Queue<Vehicle> queue = new Queue<Vehicle>();
    private Vehicle currentMoving;
    private bool clearToMove;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Vehicle");

        for (int i = 0; i < gameObjects.Length; i++)
        {
            vehicles.Add(gameObjects[i].GetComponent<Vehicle>());
        }
        sqrRadius = radius * radius;

        clearToMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Vehicle v in vehicles)
        {
            if (v.IsUnityNull())
            {
                vehicles.Remove(v);
                Destroy(v.gameObject);
            }

            if (withinRange(v.transform.position))
            {
                if(!queue.Contains(v))
                {
                    Debug.Log(-1);
                    queue.Enqueue(v);
                    v.setStopTarget(gameObject);
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
                    queue.Dequeue();
                    clearToMove = true;
                    currentMoving = null;
                }
            }
            else
            {
                if (clearToMove)
                {
                    currentMoving = queue.Peek();
                    currentMoving.switchState(Vehicle.StateList.moving);
                }
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
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }
}
