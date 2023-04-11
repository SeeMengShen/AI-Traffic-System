using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (Node))]
public class NodesLabel : Editor
{
    List<GameObject> nodes = new List<GameObject>();
    private const string NODE_STR = "Node";
    GUIStyle style = new GUIStyle();

    private void OnEnable()
    {
        style.normal.textColor = Color.black;
        nodes = GameObject.FindGameObjectsWithTag(NODE_STR).ToList();
    }

    private void OnSceneGUI()
    {
        foreach(GameObject node in nodes)
        {
            Handles.Label(node.transform.position, node.name, style);
        }       
    }
}

[CustomEditor(typeof(TrafficLightsManager))]
public class TrafficLightsManagerLabel : Editor
{
    List<GameObject> nodes = new List<GameObject>();
    private const string NODE_STR = "Node";
    GUIStyle style = new GUIStyle();

    private void OnEnable()
    {
        style.normal.textColor = Color.black;
        nodes = GameObject.FindGameObjectsWithTag(NODE_STR).ToList();
    }

    private void OnSceneGUI()
    {
        foreach (GameObject node in nodes)
        {
            Handles.Label(node.transform.position, node.name, style);
        }
    }
}

[CustomEditor(typeof(JunctionManager))]
public class JunctionsLabel : Editor
{
    List<GameObject> nodes = new List<GameObject>();
    private const string NODE_STR = "Node";
    GUIStyle style = new GUIStyle();

    private void OnEnable()
    {
        style.normal.textColor = Color.black;
        nodes = GameObject.FindGameObjectsWithTag(NODE_STR).ToList();
    }

    private void OnSceneGUI()
    {
        foreach (GameObject node in nodes)
        {
            Handles.Label(node.transform.position, node.name, style);
        }
    }
}

[CustomEditor(typeof(TrafficLight))]
public class TrafficLightsLabel : Editor
{
    List<GameObject> trafficLights = new List<GameObject>();
    private const string NODE_STR = "TrafficLight";
    GUIStyle style = new GUIStyle();

    private void OnEnable()
    {
        style.normal.textColor = Color.black;
        trafficLights = GameObject.FindGameObjectsWithTag(NODE_STR).ToList();
    }

    private void OnSceneGUI()
    {
        foreach (GameObject trafficLight in trafficLights)
        {
            Handles.Label(trafficLight.transform.position, trafficLight.name, style);
        }
    }
}