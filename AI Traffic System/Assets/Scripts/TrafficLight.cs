using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class TrafficLight : MonoBehaviour
{
    [Tooltip("The radius of the node with traffic light")]
    public float radius;
    private float sqrRadius;

    private Vector3 offset;

    // State
    delegate void TrafficLightState();
    private TrafficLightState state;
    public enum StateList
    {
        green,
        red
    }

    // Start is called before the first frame update
    void Start()
    {
        // Compute the squared value for distance computation
        sqrRadius = radius * radius;
    }

    // Update is called once per frame
    void Update()
    {
        state?.Invoke();
    }

    // Call this to switch state
    public void SwitchState(StateList stateEnum)
    {
        switch (stateEnum)
        {
            case StateList.green:
                state = Green;
                break;
            case StateList.red:
                state = Red;
                break;
            default:
                break;
        }
    }

    // When the traffic light is in Red State
    public void Red()
    {
        // Check every vehicle
        foreach (Vehicle v in VehicleManager.Instance.GetVehicles())
        {
            // If they are within the range of node with traffic light
            if (WithinRange(v.transform.position))
            {
                // Switch to Stop State
                v.SetStop(transform.parent.gameObject);
            }
        }
    }

    // When the traffic light is in Red State
    public void Green()
    {
        // Check every vehicle
        foreach (Vehicle v in VehicleManager.Instance.GetVehicles())
        {
            // If they are within the range of node with traffic light
            if (WithinRange(v.transform.position))
            {
                // Switch to Moving State
                v.SwitchState(Vehicle.StateList.moving);
            }
        }
    }

    // Check whether the vehicle is within the node with traffic light radius range
    bool WithinRange(Vector3 other)
    {
        offset = transform.parent.position - other;

        return offset.sqrMagnitude < sqrRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.parent.position, radius);
    }
}
