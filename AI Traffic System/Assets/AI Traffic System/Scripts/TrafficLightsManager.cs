using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightsManager : Node
{
    [Tooltip("Contains the traffic light of this junction")]
    public TrafficLight[] trafficLights;

    [Tooltip("Set the green light duration in second")]
    public float greenLightDurationInSecond;

    private float currentTimeLeft;
    private int currentIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize every traffic light to Red State
        foreach (TrafficLight t in trafficLights)
        {
            t.SwitchState(TrafficLight.StateList.red);
        }

        NextTrafficLight();
    }

    // Update is called once per frame
    void Update()
    {
        // Tracking the time of green light
        currentTimeLeft -= Time.deltaTime;

        //Debug.Log(trafficLights[currentIndex].gameObject.name + ": " + currentTimeLeft);

        // If time is out
        if (currentTimeLeft <= 0f)
        {
            // Switch the current traffic light to Red State
            trafficLights[currentIndex].SwitchState(TrafficLight.StateList.red);

            NextTrafficLight();
        }
    }

    // Switch next traffic light to green
    void NextTrafficLight()
    {
        // Increment the index
        currentIndex++;

        // Adjust if it exceeds the array size
        currentIndex %= trafficLights.Length;

        trafficLights[currentIndex].SwitchState(TrafficLight.StateList.green);

        //Debug.Log(trafficLights[currentIndex].name);

        // Set the green light duration
        currentTimeLeft = greenLightDurationInSecond;
    }
}
