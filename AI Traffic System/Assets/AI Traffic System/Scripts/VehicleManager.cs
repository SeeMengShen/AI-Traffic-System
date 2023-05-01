using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager Instance;
    public GameObject vehiclePrefab;
    public int quantity;
    public bool recycleVehicle;
    public List<Vehicle> vehicles = new List<Vehicle>();

    [Tooltip("The random value difference of vehicle speed")]
    [Range(0f, 1.5f)]
    public float randomSpeed;

    [Tooltip("Layer index for node")]
    public int nodeLayer;

    [Tooltip("Layer index for vehicle")]
    public int vehicleLayer;

    [Tooltip("The tag of nodes")]
    public string nodeTag;

    [Tooltip("The tag of vehicles")]
    public string vehicleTag;

    private WaitForSeconds wait = new WaitForSeconds(0.5f);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // Get vehicles on scene
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(vehicleTag);

        for (int i = 0; i < gameObjects.Length; i++)
        {
            vehicles.Add(gameObjects[i].GetComponent<Vehicle>());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelaySpawn());
    }

    public List<Vehicle> GetVehicles()
    {
        return vehicles;
    }

    public void AddVehicle()
    {
        // Get random start and end node
        Node startNode = StartEndManager.Instance.RandomStartNode();
        Node endNode = StartEndManager.Instance.RandomEndNode();

        // Instantiate the new vehicle under this script's object
        GameObject newVehicle = Instantiate(vehiclePrefab, startNode.transform.position, Quaternion.identity, transform);

        // Assign the random start and end node
        Vehicle vehicle = newVehicle.GetComponent<Vehicle>();
        vehicle.startNode = startNode;
        vehicle.endNode = endNode;

        // Add the newly created vehicle into the list
        vehicles.Add(vehicle);
    }

    // Remove the vehicle from the scene
    public void RemoveVehicle(Vehicle vehicle)
    {
        vehicles.Remove(vehicle);
        Destroy(vehicle.gameObject);
        quantity = vehicles.Count;
    }

    public void ReuseVehicle(Vehicle vehicle)
    {
        // Get random start and end node
        Node startNode = StartEndManager.Instance.RandomStartNode();
        Node endNode = StartEndManager.Instance.RandomEndNode();

        vehicle.transform.position = startNode.transform.position;
        vehicle.transform.rotation = Quaternion.identity;

        // Assign the random start and end node
        vehicle.startNode = startNode;
        vehicle.endNode = endNode;

        vehicle.Start();
    }

    IEnumerator DelaySpawn()
    {
        // Instantiate vehicles on runtime
        for (int i = 0; i < quantity; i++)
        {
            yield return wait;
            AddVehicle();
        }        
    }
}
