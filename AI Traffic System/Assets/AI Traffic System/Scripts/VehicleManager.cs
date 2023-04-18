using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager Instance;
    private List<Vehicle> vehicles = new List<Vehicle>();

    [Tooltip("Layer index for node")]
    public int nodeLayer;

    [Tooltip("Layer index for vehicle")]
    public int vehicleLayer;

    [Tooltip("The tag of nodes")]
    public string nodeTag;

    [Tooltip("The tag of vehicles")]
    public string vehicleTag;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(vehicleTag);

        for (int i = 0; i < gameObjects.Length; i++)
        {
            vehicles.Add(gameObjects[i].GetComponent<Vehicle>());
        }
    }

    public List<Vehicle> GetVehicles()
    {
        return vehicles;
    }

    public void RemoveVehicle(Vehicle vehicle)
    {
        vehicles.Remove(vehicle);
        Destroy(vehicle.gameObject);
    }

    public void AddVehicle(Vehicle vehicle)
    {
        vehicles.Add(vehicle);
    }
}
