using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Collider))]
public class Vehicle : MonoBehaviour
{
    public Node[] path;
    public int currentWP = 0;
    public float rotationSpeed;
    public float speed;
    public float accuracy;
    public float narrowCollisionAngle;
    public float wideCollisionAngle;
    public Node startNode, endNode;
    public float keepDistanceSlow, keepDistanceStop;
    private float sqrKeepDistSlow, sqrKeepDistStop;
    delegate void State();
    private State state;
    RaycastHit leftStraightHit, rightStraightHit, leftNarrowHit, rightNarrowHit, leftWideHit, rightWideHit;
    Vector3 leftNarrowDir, rightNarrowDir, leftWideDir, rightWideDir;
    bool isLeftStraightHit, isRightStraightHit, isLeftNarrowHit, isRightNarrowHit, isLeftWideHit, isRightWideHit;
    int layerMask;

    private GameObject stopTarget;

    private const string NODE_STR = "Node";
    private const string VEHICLE_STR = "Vehicle";

    private float rayOffset;

    //private Collider obstacle;

    public enum StateList
    {
        stop,
        moving,
        junctionStop,
        evading
    }

    // Start is called before the first frame update
    void Start()
    {
        switchState(StateList.moving);

        path = Graph.Instance.AStar(startNode, endNode);

        layerMask = ~(1 << 6);

        sqrKeepDistSlow = keepDistanceSlow * keepDistanceSlow;
        sqrKeepDistStop = keepDistanceStop * keepDistanceStop;

        rayOffset = (GetComponent<Collider>().bounds.size.x / 2) + 0.1f;

        if (path == null)
        {
            Debug.Log("null");
            Destroy(this);
        }
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
            case StateList.stop:
                state = stop;
                break;
            case StateList.moving:
                state = moving;
                break;
            case StateList.evading:
                state = evading;
                break;
            default:
                break;
        }
    }

    void stop()
    {
        Vector3 direction = stopTarget.transform.position - transform.position;

        float distance = direction.sqrMagnitude;
        float targetSpeed;
        float sqrDistanceStop = sqrKeepDistStop;
        float sqrDistanceSlow = sqrKeepDistSlow;

        if (stopTarget.CompareTag(NODE_STR))
        {
            if(stopTarget.GetComponent<JunctionManager>() != null)
            {
                sqrDistanceStop = stopTarget.GetComponent<JunctionManager>().radius;
            }
            else if(stopTarget.GetComponent<TrafficLight>() != null)
            {
                sqrDistanceStop = stopTarget.GetComponentInChildren<TrafficLight>().radius;
            }
            
            sqrDistanceStop *= sqrDistanceStop;

            sqrDistanceSlow = sqrDistanceStop * 0.8f;
        }

        if (distance < sqrDistanceStop)
        {
            return;
        }

        if (distance > sqrDistanceSlow)
        {
            targetSpeed = speed;
            switchState(StateList.moving);
        }
        else
        {
            targetSpeed = speed * direction.magnitude / keepDistanceSlow;
        }
        transform.Translate(0.0f, 0.0f, Time.deltaTime * targetSpeed);
    }

    void moving()
    {
        isLeftStraightHit = false;
        isRightStraightHit = false;
        isLeftNarrowHit = false;
        isRightNarrowHit = false;
        isLeftWideHit = false;
        isRightWideHit = false;

        /*leftNarrowDir = Quaternion.Euler(0f, -narrowCollisionAngle / 2, 0f) * transform.forward;
        rightNarrowDir = Quaternion.Euler(0f, narrowCollisionAngle / 2, 0f) * transform.forward;
        leftWideDir = Quaternion.Euler(0f, -wideCollisionAngle / 2, 0f) * transform.forward;
        rightWideDir = Quaternion.Euler(0f, wideCollisionAngle / 2, 0f) * transform.forward;*/

        leftNarrowDir = Quaternion.AngleAxis(-narrowCollisionAngle / 2, Vector3.up) * transform.forward;
        rightNarrowDir = Quaternion.AngleAxis(narrowCollisionAngle / 2, Vector3.up) * transform.forward;
        leftWideDir = Quaternion.AngleAxis(-wideCollisionAngle / 2, Vector3.up) * transform.forward;
        rightWideDir = Quaternion.AngleAxis(wideCollisionAngle / 2, Vector3.up) * transform.forward;

        /*Debug.DrawRay(transform.position - rayOffset, transform.forward * 5.0f, Color.blue);
        Debug.DrawRay(transform.position + rayOffset, transform.forward * 5.0f, Color.blue);
        Debug.DrawRay(transform.position - rayOffset, leftNarrowDir.normalized * 5.0f, Color.red);
        Debug.DrawRay(transform.position + rayOffset, rightNarrowDir.normalized * 5.0f, Color.red);
        Debug.DrawRay(transform.position - rayOffset, leftWideDir.normalized * 5.0f, Color.green);
        Debug.DrawRay(transform.position + rayOffset, rightWideDir.normalized * 5.0f, Color.green);*/

        Vector3 offsetLeftPosition = transform.position;
        offsetLeftPosition -= transform.right * rayOffset;
        Vector3 offsetRightPosition = transform.position;
        offsetRightPosition += transform.right * rayOffset;

        if (Physics.Raycast(offsetLeftPosition, transform.forward, out leftStraightHit, 5.0f, layerMask))
        {
            isLeftStraightHit = true;
            Debug.Log("HitStraightLeft");
            Debug.Log(leftStraightHit.collider.gameObject.name);

            if (leftStraightHit.collider.CompareTag(VEHICLE_STR))
            {
                Debug.Log("Stop");
                setStopTarget(leftStraightHit.transform.gameObject);
                switchState(Vehicle.StateList.stop);
                return;
            }
        }
        if (Physics.Raycast(offsetRightPosition, transform.forward, out rightStraightHit, 5.0f, layerMask))
        {
            isRightStraightHit = true;
            Debug.Log("HitStraightRight");
            Debug.Log(rightStraightHit.collider.gameObject.name);

            if (rightStraightHit.collider.CompareTag(VEHICLE_STR))
            {
                Debug.Log("Stop");
                setStopTarget(rightStraightHit.transform.gameObject);
                switchState(Vehicle.StateList.stop);
                return;
            }
        }
        if (Physics.Raycast(offsetLeftPosition, leftNarrowDir, out leftNarrowHit, 5.0f, layerMask))
        {
            isLeftNarrowHit = true;
            Debug.Log("HitLeft");
        }
        if (Physics.Raycast(offsetRightPosition, rightNarrowDir, out rightNarrowHit, 5.0f, layerMask))
        {
            isRightNarrowHit = true;
            Debug.Log("HitRight");
        }

        if (isLeftStraightHit && isRightStraightHit)
        {
            Debug.Log("Stop");
            setStopTarget(leftStraightHit.transform.gameObject);
            switchState(Vehicle.StateList.stop);
            return;
        }

        if (isLeftNarrowHit)
        {
            if (Physics.Raycast(offsetLeftPosition, leftWideDir, out leftWideHit, 5.0f, layerMask))
            {
                isLeftWideHit = true;
                Debug.Log("HitLeftWide");
            }

        }

        if (isRightNarrowHit)
        {
            if (Physics.Raycast(offsetRightPosition, rightWideDir, out rightWideHit, 5.0f, layerMask))
            {
                isRightWideHit = true;
                Debug.Log("HitRightWide");
            }
        }

        Debug.DrawRay(offsetLeftPosition, transform.forward * 5.0f, Color.blue);
        Debug.DrawRay(offsetRightPosition, transform.forward * 5.0f, Color.blue);
        Debug.DrawRay(offsetLeftPosition, leftNarrowDir.normalized * 5.0f, Color.red);
        Debug.DrawRay(offsetRightPosition, rightNarrowDir.normalized * 5.0f, Color.red);
        Debug.DrawRay(offsetLeftPosition, leftWideDir.normalized * 5.0f, Color.green);
        Debug.DrawRay(offsetRightPosition, rightWideDir.normalized * 5.0f, Color.green);

        // if there is no path or at the end don't do anything
        if (currentWP == path.Length)
        {
            return;
        }

        /*
        // the node we are closest to at this moment
        currentNode = graph.getPathPoint(currentWP);*/

        // if we are close enough to the current waypoint, move to next
        if (Vector3.Distance(path[currentWP].transform.position, transform.position) < accuracy)
        {
            currentWP++;
        }

        // if we are not at the end of path
        if (currentWP < path.Length)
        {
            //Move
            Vector3 direction = path[currentWP].transform.position - transform.position;
            float rotateFactor = 1f;


            if (isLeftStraightHit)
            {
                direction = transform.right;
                rotateFactor = 0.5f;

                if (isLeftNarrowHit)
                {
                    rotateFactor *= 2;

                    if (isLeftWideHit)
                    {
                        rotateFactor *= 2;
                    }
                }
            }
            else if (isRightStraightHit)
            {
                direction = -transform.right;
                rotateFactor = 0.5f;

                if (isRightNarrowHit)
                {
                    rotateFactor *= 2;

                    if (isRightWideHit)
                    {
                        rotateFactor *= 2;
                    }
                }
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * rotateFactor * Time.deltaTime);
            transform.Translate(0.0f, 0.0f, Time.deltaTime * speed);
        }
    }

    void evading()
    {
        /*Debug.Log("Evading");
        Vector3 obstaclePosition = Vector3.zero;
        Vector3 toLook = Vector3.zero;

        if (isLeftNarrowHit)
        {
            obstaclePosition = leftNarrowHit.transform.position;
            toLook = transform.right;
        }
        else if (isRightNarrowHit)
        {
            obstaclePosition = rightNarrowHit.transform.position;
            toLook = -transform.right;
        }

        Vector3 direction = obstaclePosition - transform.position;
        float distance = direction.sqrMagnitude;

        if (distance < sqrKeepDistSlow/3)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toLook), rotationSpeed * Time.deltaTime);
        }
        else
        {
            switchState(StateList.moving);
        }
        transform.Translate(0.0f, 0.0f, Time.deltaTime * speed);*/
    }

    public void setStopTarget(GameObject target)
    {
        stopTarget = target;
    }
}
