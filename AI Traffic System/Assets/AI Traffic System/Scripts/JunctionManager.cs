using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class JunctionManager : Node
{
    [Tooltip("The radius of the junction, it is recommended to cover 1 node from each road")]
    public float radius;
    private float sqrRadius;

    private Vector3 offset;

    [Tooltip("For you to check the queue of the junction")]
    [SerializeField] public Queue<Vehicle> queue = new Queue<Vehicle>();

    public Vehicle currentMoving;
    private bool clearToMove;

    // Start is called before the first frame update
    void Start()
    {
        // Compute the squared radius for distance comaprison
        sqrRadius = radius * radius;

        clearToMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for each vehicle in the scene
        foreach (Vehicle v in VehicleManager.Instance.GetVehicles())
        {
            // If they are within the radius range
            if (WithinRange(v.transform.position))
            {
                // and does not exists in the queue
                if (!queue.Contains(v))
                {
                    // Add them into the queue
                    queue.Enqueue(v);

                    // Set the current moving inJunction value to true
                    v.inJunction = true;

                    // Switch their state to Stop State
                    v.SetStop(gameObject);
                }
            }
        }

        // If there are vehicles in the queue
        if (queue.Count > 0)
        {
            // If the current moving vehicle exists
            if (currentMoving != null)
            {
                // and it is still moving within the junction radius range
                if (WithinRange(currentMoving.transform.position))
                {
                    // The junction is not clear to move
                    clearToMove = false;
                }
                else // the current moving vehicle is out of the junction radius range
                {
                    // Remove it from the queue
                    queue.Dequeue();

                    // The junction is clear to move now
                    clearToMove = true;

                    // Set the current moving inJunction value to false
                    currentMoving.inJunction = false;

                    // Set the current moving to null
                    currentMoving = null;
                }
            }
            else // There is no current moving vehicle
            {
                // If the junction is clear to move
                if (clearToMove)
                {
                    // Take the first vehicle from the queue and set it as current moving vehicle
                    currentMoving = queue.Peek();

                    // Switch its state to Moving State
                    currentMoving.SwitchState(Vehicle.StateList.moving);
                }
            }
        }
    }

    // Check whether the vehicle is within the range of radius
    bool WithinRange(Vector3 other)
    {
        offset = transform.position - other;

        return offset.sqrMagnitude < sqrRadius;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }
}
