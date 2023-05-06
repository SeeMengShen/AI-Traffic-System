using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

[CanEditMultipleObjects]
[CustomEditor(typeof(Node))]
public class NodesEditor : Editor
{
    List<GameObject> nodes = new List<GameObject>();
    private const string NODE_STR = "Node";
    GUIStyle style = new GUIStyle();
    private ReorderableList list;

    protected void OnEnable()
    {
        style.normal.textColor = Color.black;
        nodes = GameObject.FindGameObjectsWithTag(NODE_STR).ToList();

        list = new ReorderableList(serializedObject, serializedObject.FindProperty("toNode"), true, true, true, true);

        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "To Node");
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
    }

    protected void OnSceneGUI()
    {
        foreach (GameObject node in nodes)
        {
            Handles.Label(node.transform.position, node.name, style);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Node selectedNode = (Node)target;

        CheckNodeType(selectedNode);
    }

    private void CheckNodeType(Node node)
    {
        if (node.type == Node.NodeType.start || node.type == Node.NodeType.normal)
        {
            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

[CustomEditor(typeof(TrafficLightsManager))]
public class TrafficLightsManagerEditor : NodesEditor
{

}

[CustomEditor(typeof(JunctionManager))]
public class JunctionsManagerEditor : NodesEditor
{

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

[CustomEditor(typeof(ConnectionManager))]
public class ConnectionManagerEditor : Editor
{
    private const string REFRESH_STR = "Refresh Nodes";
    private const string CONNECT_STR = "Generate Connections";
    private const string CLEAR_STR = "Clear Connections";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button(CLEAR_STR))
        {
            ConnectionManager.Instance.ClearConnections();
        }
        if (GUILayout.Button(REFRESH_STR))
        {
            ConnectionManager.Instance.RefreshNodes();
        }
        else if (GUILayout.Button(CONNECT_STR))
        {
            ConnectionManager.Instance.GenerateConnections();
        }
    }
}
#endif