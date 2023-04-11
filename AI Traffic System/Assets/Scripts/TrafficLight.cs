using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class TrafficLight : MonoBehaviour
{
    public float radius;
    private float sqrRadius;
    private List<Vehicle> vehicles = new List<Vehicle>();
    private Vector3 offset;
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
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Vehicle");

        for (int i = 0; i < gameObjects.Length; i++)
        {
            vehicles.Add(gameObjects[i].GetComponent<Vehicle>());
        }

        sqrRadius = radius * radius;
    }

    // Update is called once per frame
    void Update()
    {
        state?.Invoke();
    }

    public void switchState(StateList stateEnum)
    {
        switch (stateEnum)
        {
            case StateList.green:
                state = green;
                break;
            case StateList.red:
                state = red;
                break;
            default:
                break;
        }
    }

    public void red()
    {
        foreach (Vehicle v in vehicles)
        {
            if (withinRange(v.transform.position))
            {
                v.setStopTarget(transform.parent.gameObject);
                v.switchState(Vehicle.StateList.stop);
            }
        }
    }

    public void green()
    {
        foreach (Vehicle v in vehicles)
        {
            if (withinRange(v.transform.position))
            {
                v.switchState(Vehicle.StateList.moving);
            }
        }
    }

    bool withinRange(Vector3 other)
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
