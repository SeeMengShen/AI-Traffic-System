using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightsManager : Node
{
    public TrafficLight[] trafficLights;
    public float greenLightDurationInSecond;
    private float currentTimeLeft;
    private int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        foreach (TrafficLight t in trafficLights)
        {
            t.switchState(TrafficLight.StateList.red);
        }

        nextTrafficLight();
    }

    // Update is called once per frame
    void Update()
    {
        currentTimeLeft -= Time.deltaTime;

        Debug.Log(trafficLights[currentIndex].gameObject.name + ": " + currentTimeLeft);

        if (currentTimeLeft <= 0f)
        {
            trafficLights[currentIndex].switchState(TrafficLight.StateList.red);

            currentIndex++;
            currentIndex %= trafficLights.Length;

            nextTrafficLight();
        }
    }

    void nextTrafficLight()
    {
        trafficLights[currentIndex].switchState(TrafficLight.StateList.green);
        Debug.Log(trafficLights[currentIndex].name);
        currentTimeLeft = greenLightDurationInSecond;
    }
}
