using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DemoUI : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public TextMeshProUGUI viewingText;
    public Transform[] junctions;
    private const string VEHICLE_STR = "Vehicle ";
    private const string JUNCTION_STR = "Junction ";
    private int currentVehicleIndex = 0;
    private int currentJunctionIndex = 0;
    private Vector3 vehicleCamOffset = new Vector3(0f, 2f, -3f);
    private Vector3 junctionCamOffset = new Vector3(0f, 30f, 0f);
    Vector3 obstacleOffset = new Vector3(0f, 0.5f, 0f);
    private Vector3 overviewCamOffset;

    // Start is called before the first frame update
    void Start()
    {
        overviewCamOffset = Camera.main.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 pos = Vector3.zero;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 50f))
            {
                pos = raycastHit.point;
            }
            else
            {
                return;
            }
            //Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
            Instantiate(obstaclePrefab, pos + obstacleOffset, Quaternion.identity);
        }
    }

    public void SwitchVehicle()
    {
        currentVehicleIndex++;
        currentVehicleIndex %= 30;
        Transform vehicle = VehicleManager.Instance.vehicles[currentVehicleIndex].transform;
        Camera.main.transform.SetParent(vehicle);
        Camera.main.transform.localPosition = vehicleCamOffset;
        Camera.main.transform.LookAt(vehicle);
        viewingText.text = VEHICLE_STR + currentVehicleIndex.ToString();
    }

    public void SwitchJunctions()
    {
        currentJunctionIndex++;
        currentJunctionIndex %= 14;
        Transform junction = junctions[currentJunctionIndex];
        Camera.main.transform.SetParent(junction);
        Camera.main.transform.localPosition = junctionCamOffset;
        Camera.main.transform.LookAt(junction);
        viewingText.text = JUNCTION_STR + currentJunctionIndex.ToString();
    }

    public void SwitchToOverview()
    {
        Camera.main.transform.SetParent(null);
        Camera.main.transform.localPosition = overviewCamOffset;
        Camera.main.transform.LookAt(Vector3.zero);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
