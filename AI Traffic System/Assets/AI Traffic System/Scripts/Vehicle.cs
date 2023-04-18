using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Vehicle : MonoBehaviour
{
    [Tooltip("For you to check the path this vehicle is taking")]
    [SerializeField] private Node[] path;

    [Tooltip("For you to check the current waypoint the vehicle on")]
    [SerializeField] private int currentWP = 0;

    [Tooltip("The rotation speed of the vehicle (1 is suggested)")]
    public float rotationSpeed;

    [Tooltip("The moving speed of the vehicle (2 is suggested)")]
    public float speed;

    [Tooltip("The distance for the vehicle needed to detect the current node for moving on to the next node")]
    public float accuracy;
    private float sqrAccuracy;

    [Tooltip("The narrow angle of the vehicle obstacle detection")]
    public float narrowCollisionAngle;

    [Tooltip("The wide angle of the vehicle obstacle detection")]
    public float wideCollisionAngle;

    [Tooltip("The starting point of the vehicle")]
    public Node startNode;

    [Tooltip("The goal of the vehicle")]
    public Node endNode;

    [Tooltip("The distance for the vehicle to slow down when an obstacle detected infront")]
    public float keepDistanceSlow;

    [Tooltip("The distance for the vehicle to stop when an obstacle detected infront")]
    public float keepDistanceStop;
    private float sqrKeepDistSlow, sqrKeepDistStop;

    // State
    delegate void State();
    private State state;
    public enum StateList
    {
        stop,
        moving
    }

    private StateList currentState;

    // Obstacle hit information
    RaycastHit leftNarrowStraightHit, rightNarrowStraightHit, leftWideStraightHit, rightWideStraightHit;
    Vector3 leftNarrowDir, rightNarrowDir, leftWideDir, rightWideDir;
    bool isLeftNarrowStraightHit, isRightNarrowStraightHit, isLeftWideStraightHit, isRightWideStraightHit, isLeftNarrowHit, isRightNarrowHit, isLeftWideHit, isRightWideHit;

    private int layerMask;
    private int ignoreVehicleLayerMask;

    public bool inJunction;

    [Tooltip("For you to check which obstacle the vehicle is detecting")]
    [SerializeField] private GameObject stopTarget;

    private float rayXOffset;
    private float rayYOffset;

    // Start is called before the first frame update
    void Start()
    {
        // Set the initial state to Moving State
        SwitchState(StateList.moving);

        // Compute path using A*
        path = Graph.Instance.AStar(startNode, endNode);

        // Convert the int of the layer to layerMask
        layerMask = ~(1 << VehicleManager.Instance.nodeLayer);
        ignoreVehicleLayerMask = ~((1 << VehicleManager.Instance.nodeLayer) | (1 << VehicleManager.Instance.vehicleLayer));

        // Compute squared value of distance comparison
        sqrKeepDistSlow = keepDistanceSlow * keepDistanceSlow;
        sqrKeepDistStop = keepDistanceStop * keepDistanceStop;
        sqrAccuracy = accuracy * accuracy;

        // Get the boundary and set offset for the ray position
        rayXOffset = (GetComponent<Collider>().bounds.size.x / 2) + 0.1f;
        rayYOffset = (GetComponent<Collider>().bounds.size.z / 2) + 0.1f;

        // If there is no path found, destroy itself
        if (path == null)
        {
            Debug.Log(gameObject.name + " could not find a path, so it chooses to suicide :(");
            VehicleManager.Instance.RemoveVehicle(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Reset the hit information boolean
        ResetHitBool();

        // If it reaches the goal node, destroy itself
        if (currentWP == path.Length)
        {
            VehicleManager.Instance.RemoveVehicle(this);
            return;
        }

        // If close enough to the current waypoint, move to next
        CheckCurrentWayPoint();

        // If not at the end of path
        if (currentWP < path.Length)
        {
            state?.Invoke();
        }
    }

    // Call this to switch state
    public void SwitchState(StateList stateEnum)
    {
        switch (stateEnum)
        {
            case StateList.stop:
                state = Stop;
                currentState = StateList.stop;
                break;
            case StateList.moving:
                state = Moving;
                currentState = StateList.moving;
                break;
            default:
                break;
        }
    }

    /*void stop()
    {
        // Check current waypoint even when in Stop State
        checkCurrentWayPoint();

        // Get a vector between the stop target and itself
        Vector3 direction = stopTarget.transform.position - transform.position;

        // Target speed used for controlling moving speed in Stop State
        float targetSpeed;

        // Get the squared distance between the stop target and itself
        float sqrDistance = direction.sqrMagnitude;

        // Make a copy of the value to avoid mutating it
        float sqrDistanceStop = sqrKeepDistStop;
        float sqrDistanceSlow = sqrKeepDistSlow;

        // Create a larger squared distance for slowing down for exiting the Stop State
        float largerSqrDistanceSlow = keepDistanceSlow * 1.3f;
        largerSqrDistanceSlow *= largerSqrDistanceSlow;

        // Reassign the direction to the actual moving target (Way points)
        direction = path[currentWP].transform.position - transform.position;

        // If the stop target is a type of node (is a junction or a traffic light triggered the Stop State)
        if (stopTarget.CompareTag(NODE_STR))
        {
            // Get the radius from the respective Component and assign as slowing down distance
            if (stopTarget.GetComponent<JunctionManager>() != null)
            {
                sqrDistanceSlow = stopTarget.GetComponent<JunctionManager>().radius;
            }
            else if (stopTarget.GetComponent<TrafficLight>() != null)
            {
                sqrDistanceSlow = stopTarget.GetComponentInChildren<TrafficLight>().radius;
            }

            // Copy the value from slowing distance and discount to 80% of it for stopping distance
            sqrDistanceStop = sqrDistanceSlow;
            sqrDistanceStop *= 0.8f;

            // Square the value
            sqrDistanceStop *= sqrDistanceStop;
            sqrDistanceSlow *= sqrDistanceSlow;
        }

        // Do nothing if it has reached the stopping distance
        if (sqrDistance < sqrDistanceStop)
        {
            return;
        }

        // Move with normal speed if it exits the slowing distance
        if (sqrDistance > sqrDistanceSlow)
        {
            Debug.DrawLine(transform.position, stopTarget.transform.position, Color.green);
            targetSpeed = speed;

            // Exit the Stop State when it has a big distance with the stop target
            if (sqrDistance > largerSqrDistanceSlow)
            {
                switchState(StateList.moving);
                return;
            }
        }
        else // Gradually slow down according to the ratio of the slowing down distance and their distance (Arrival behaviour)
        {
            Debug.DrawLine(transform.position, stopTarget.transform.position, Color.magenta);
            targetSpeed = speed * (sqrDistance / sqrDistanceSlow);
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

        transform.Translate(0.0f, 0.0f, Time.deltaTime * targetSpeed);
    }*/

    void Stop()
    {
        // Check obstacle infront
        RayCollisionCheck();

        // If state switched, return
        if (currentState != StateList.stop)
        {
            return;
        }

        // Check stop target, if it is null (probably destroyed), switch to moving state
        if (stopTarget == null)
        {
            SwitchState(StateList.moving);
            return;
        }

        // Get a vector between the stop target and itself
        Vector3 direction = stopTarget.transform.position - transform.position;

        // Target speed used for controlling moving speed in Stop State
        float targetSpeed;

        // Get the squared distance between the stop target and itself
        float sqrDistance = direction.sqrMagnitude;

        // Make a copy of the value to avoid mutating it
        float sqrDistanceStop = sqrKeepDistStop;
        float sqrDistanceSlow = sqrKeepDistSlow;

        // Create a larger squared distance for slowing down for exiting the Stop State
        float largerSqrDistanceSlow = keepDistanceSlow * 1.3f;
        largerSqrDistanceSlow *= largerSqrDistanceSlow;

        // Reassign the direction to the actual moving target (Way points)
        direction = path[currentWP].transform.position - transform.position;

        // If the stop target is a type of node (is a junction or a traffic light triggered the Stop State)
        if (stopTarget.CompareTag(VehicleManager.Instance.nodeTag))
        {
            // Get the radius from the respective Component and assign as slowing down distance
            if (stopTarget.GetComponent<JunctionManager>() != null)
            {
                sqrDistanceSlow = stopTarget.GetComponent<JunctionManager>().radius;
            }
            else if (stopTarget.GetComponent<TrafficLight>() != null)
            {
                sqrDistanceSlow = stopTarget.GetComponentInChildren<TrafficLight>().radius;
            }

            // Copy the value from slowing distance and discount to 80% of it for stopping distance
            sqrDistanceStop = sqrDistanceSlow;
            sqrDistanceStop *= 0.9f;

            // Square the value
            sqrDistanceStop *= sqrDistanceStop;
            sqrDistanceSlow *= sqrDistanceSlow;
        }

        // Do nothing if it has reached the stopping distance
        if (sqrDistance < sqrDistanceStop)
        {
            return;
        }

        // Move with normal speed if it exits the slowing distance
        if (sqrDistance > sqrDistanceSlow)
        {
            Debug.DrawLine(transform.position, stopTarget.transform.position, Color.green);
            targetSpeed = speed;

            // Exit the Stop State when it has a big distance with the stop target
            if (sqrDistance > largerSqrDistanceSlow)
            {
                SwitchState(StateList.moving);
                return;
            }
        }
        else // Gradually slow down according to the ratio of the slowing down distance and their distance (Arrival behaviour)
        {
            Debug.DrawLine(transform.position, stopTarget.transform.position, Color.magenta);
            targetSpeed = speed * (sqrDistance / sqrDistanceSlow);
        }

        float rotateFactor = 1f;

        // If the stop target is not vehicle
        if (!stopTarget.CompareTag(VehicleManager.Instance.vehicleTag))
        {
            // Perform avoidance if obstacle detected
            if (isLeftNarrowStraightHit || isLeftWideStraightHit)
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
            else if (isRightNarrowStraightHit || isRightWideStraightHit)
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
        }

        // Rotate and move by target speed
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * rotateFactor * Time.deltaTime);
        transform.Translate(0.0f, 0.0f, Time.deltaTime * targetSpeed);
    }

    /*void moving()
    {
        resetHitBool();

        rayCollisionCheck();

        // if there is no path or at the end don't do anything
        if (currentWP == path.Length)
        {
            return;
        }

        // if we are close enough to the current waypoint, move to next
        checkCurrentWayPoint();

        // if we are not at the end of path
        if (currentWP < path.Length)
        {
            //Move
            Vector3 direction = path[currentWP].transform.position - transform.position;
            float rotateFactor = 1f;

            if (isLeftNarrowStraightHit || isLeftWideStraightHit)
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
            else if (isRightNarrowStraightHit || isRightWideStraightHit)
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
    }*/

    void Moving()
    {
        // Check obstacle infront
        RayCollisionCheck();

        // If state switched, return
        if (currentState != StateList.moving)
        {
            return;
        }

        Vector3 direction = path[currentWP].transform.position - transform.position;

        // Perform avoidance if obstacle detected
        float rotateFactor = 1f;

        if (isLeftNarrowStraightHit || isLeftWideStraightHit)
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
        else if (isRightNarrowStraightHit || isRightWideStraightHit)
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

        // Rotate and move by vehicle speed
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * rotateFactor * Time.deltaTime);
        transform.Translate(0.0f, 0.0f, Time.deltaTime * speed);
    }

    private void RayCollisionCheck()
    {
        

        LayerMask mask;

        if (inJunction)
        {
            mask = ignoreVehicleLayerMask;
        }
        else
        {
            mask = layerMask;
        }

        // Uncomment here for chaotic roundabout 1
        //mask = layerMask;

        // Set the offset angle of the branching out rays
        leftNarrowDir = Quaternion.AngleAxis(-narrowCollisionAngle / 2, Vector3.up) * transform.forward;
        rightNarrowDir = Quaternion.AngleAxis(narrowCollisionAngle / 2, Vector3.up) * transform.forward;
        leftWideDir = Quaternion.AngleAxis(-wideCollisionAngle / 2, Vector3.up) * transform.forward;
        rightWideDir = Quaternion.AngleAxis(wideCollisionAngle / 2, Vector3.up) * transform.forward;

        // Set the position offset of the straight narrow rays
        Vector3 narrowOffsetLeftPosition = transform.position;
        narrowOffsetLeftPosition -= transform.right * rayXOffset / 3;
        narrowOffsetLeftPosition += transform.forward * rayYOffset;
        Vector3 narrowOffsetRightPosition = transform.position;
        narrowOffsetRightPosition += transform.right * rayXOffset / 3;
        narrowOffsetRightPosition += transform.forward * rayYOffset;

        // Set the position offset of the remaining rays
        Vector3 offsetLeftPosition = transform.position;
        offsetLeftPosition -= transform.right * rayXOffset;
        Vector3 offsetRightPosition = transform.position;
        offsetRightPosition += transform.right * rayXOffset;

        // Visualize the rays
        Debug.DrawRay(narrowOffsetLeftPosition, transform.forward * (keepDistanceSlow - rayYOffset), Color.magenta);
        Debug.DrawRay(narrowOffsetRightPosition, transform.forward * (keepDistanceSlow - rayYOffset), Color.magenta);
        Debug.DrawRay(offsetLeftPosition, transform.forward * keepDistanceSlow, Color.blue);
        Debug.DrawRay(offsetRightPosition, transform.forward * keepDistanceSlow, Color.blue);
        Debug.DrawRay(offsetLeftPosition, leftNarrowDir.normalized * keepDistanceSlow, Color.red);
        Debug.DrawRay(offsetRightPosition, rightNarrowDir.normalized * keepDistanceSlow, Color.red);
        Debug.DrawRay(offsetLeftPosition, leftWideDir.normalized * keepDistanceSlow, Color.green);
        Debug.DrawRay(offsetRightPosition, rightWideDir.normalized * keepDistanceSlow, Color.green);

        if (Physics.Raycast(narrowOffsetLeftPosition, transform.forward, out leftNarrowStraightHit, keepDistanceSlow - rayYOffset, mask))
        {
            isLeftNarrowStraightHit = true;

            // If the collided game object is a vehicle
            if (leftNarrowStraightHit.collider.CompareTag(VehicleManager.Instance.vehicleTag))
            {
                // and if it is not himself
                if (leftNarrowStraightHit.collider.gameObject != gameObject)
                {
                    // switch to Stop State
                    SetStop(leftNarrowStraightHit.transform.gameObject);
                    return;
                }
            }
        }
        if (Physics.Raycast(narrowOffsetRightPosition, transform.forward, out rightNarrowStraightHit, keepDistanceSlow - rayYOffset, mask))
        {
            isRightNarrowStraightHit = true;

            // If the collided game object is a vehicle
            if (rightNarrowStraightHit.collider.CompareTag(VehicleManager.Instance.vehicleTag))
            {
                // and if it is not himself
                if (rightNarrowStraightHit.collider.gameObject != gameObject)
                {
                    // switch to Stop State
                    SetStop(rightNarrowStraightHit.transform.gameObject);
                    return;
                }
            }
        }
        if (Physics.Raycast(offsetLeftPosition, transform.forward, out leftWideStraightHit, keepDistanceSlow, mask))
        {
            isLeftWideStraightHit = true;

            // If the collided game object is a vehicle
            if (leftWideStraightHit.collider.CompareTag(VehicleManager.Instance.vehicleTag))
            {
                // and if it is not himself
                if (leftWideStraightHit.collider.gameObject != gameObject)
                {
                    // switch to Stop State
                    SetStop(leftWideStraightHit.transform.gameObject);
                    return;
                }
            }
        }
        if (Physics.Raycast(offsetRightPosition, transform.forward, out rightWideStraightHit, keepDistanceSlow, mask))
        {
            isRightWideStraightHit = true;

            // If the collided game object is a vehicle
            if (rightWideStraightHit.collider.CompareTag(VehicleManager.Instance.vehicleTag))
            {
                // and if it is not himself
                if (rightWideStraightHit.collider.gameObject != gameObject)
                {
                    // switch to Stop State
                    SetStop(rightWideStraightHit.transform.gameObject);
                    return;
                }
            }
        }
        if (Physics.Raycast(offsetLeftPosition, leftNarrowDir, keepDistanceSlow, mask))
        {
            isLeftNarrowHit = true;
        }
        if (Physics.Raycast(offsetRightPosition, rightNarrowDir, keepDistanceSlow, mask))
        {
            isRightNarrowHit = true;
        }

        // If both narrow front ray collided, switch to Stop State 
        if (isLeftNarrowStraightHit && isRightNarrowStraightHit)
        {
            SetStop(leftNarrowStraightHit.transform.gameObject);
            return;
        }

        // Only check for wider rays collision if the narrow rays collided
        if (isLeftNarrowHit)
        {
            if (Physics.Raycast(offsetLeftPosition, leftWideDir, keepDistanceSlow, mask))
            {
                isLeftWideHit = true;
            }

        }
        if (isRightNarrowHit)
        {
            if (Physics.Raycast(offsetRightPosition, rightWideDir, keepDistanceSlow, mask))
            {
                isRightWideHit = true;
            }
        }

        // Uncomment here for chaotic roundabout 2
        /*if (stopTarget != null)
        {
            if (stopTarget.CompareTag(VehicleManager.Instance.vehicleTag))
            {
                if (!(isLeftNarrowStraightHit || isRightNarrowStraightHit || isLeftWideStraightHit || isRightWideStraightHit))
                {
                    SwitchState(StateList.moving);
                }
            }
        }*/
    }

    /*    private void arrivalRayCollisionCheck()
        {
            leftNarrowDir = Quaternion.AngleAxis(-narrowCollisionAngle / 2, Vector3.up) * transform.forward;
            rightNarrowDir = Quaternion.AngleAxis(narrowCollisionAngle / 2, Vector3.up) * transform.forward;
            leftWideDir = Quaternion.AngleAxis(-wideCollisionAngle / 2, Vector3.up) * transform.forward;
            rightWideDir = Quaternion.AngleAxis(wideCollisionAngle / 2, Vector3.up) * transform.forward;

            Vector3 narrowOffsetLeftPosition = transform.position;
            narrowOffsetLeftPosition -= transform.right * rayXOffset / 3;
            narrowOffsetLeftPosition += transform.forward * rayYOffset;
            Vector3 narrowOffsetRightPosition = transform.position;
            narrowOffsetRightPosition += transform.right * rayXOffset / 3;
            narrowOffsetRightPosition += transform.forward * rayYOffset;

            Vector3 offsetLeftPosition = transform.position;
            offsetLeftPosition -= transform.right * rayXOffset;
            Vector3 offsetRightPosition = transform.position;
            offsetRightPosition += transform.right * rayXOffset;

            if (Physics.Raycast(narrowOffsetLeftPosition, transform.forward, out leftNarrowStraightHit, keepDistanceSlow - rayYOffset, arrivalLayerMask))
            {
                isLeftNarrowStraightHit = true;

                if (leftNarrowStraightHit.collider.CompareTag(VEHICLE_STR))
                {
                    if (leftNarrowStraightHit.collider.gameObject != gameObject)
                    {
                        setStop(leftNarrowStraightHit.transform.gameObject);
                        return;
                    }
                }
            }
            if (Physics.Raycast(narrowOffsetRightPosition, transform.forward, out rightNarrowStraightHit, keepDistanceSlow - rayYOffset, arrivalLayerMask))
            {
                isRightNarrowStraightHit = true;

                if (rightNarrowStraightHit.collider.CompareTag(VEHICLE_STR))
                {
                    if (rightNarrowStraightHit.collider.gameObject != gameObject)
                    {
                        setStop(rightNarrowStraightHit.transform.gameObject);
                        return;
                    }
                }
            }
            if (Physics.Raycast(offsetLeftPosition, transform.forward, out leftWideStraightHit, keepDistanceSlow, arrivalLayerMask))
            {
                isLeftWideStraightHit = true;

                if (leftWideStraightHit.collider.CompareTag(VEHICLE_STR))
                {
                    if (leftWideStraightHit.collider.gameObject != gameObject)
                    {
                        setStop(leftWideStraightHit.transform.gameObject);
                        return;
                    }
                }
            }
            if (Physics.Raycast(offsetRightPosition, transform.forward, out rightWideStraightHit, keepDistanceSlow, arrivalLayerMask))
            {
                isRightWideStraightHit = true;

                if (rightWideStraightHit.collider.CompareTag(VEHICLE_STR))
                {
                    if (rightWideStraightHit.collider.gameObject != gameObject)
                    {
                        setStop(rightWideStraightHit.transform.gameObject);
                        return;
                    }
                }
            }
            if (Physics.Raycast(offsetLeftPosition, leftNarrowDir, keepDistanceSlow, arrivalLayerMask))
            {
                isLeftNarrowHit = true;
            }
            if (Physics.Raycast(offsetRightPosition, rightNarrowDir, keepDistanceSlow, arrivalLayerMask))
            {
                isRightNarrowHit = true;
            }
            if (isLeftNarrowStraightHit && isRightNarrowStraightHit)
            {
                setStop(leftNarrowStraightHit.transform.gameObject);
                return;
            }
            if (isLeftNarrowHit)
            {
                if (Physics.Raycast(offsetLeftPosition, leftWideDir, keepDistanceSlow, arrivalLayerMask))
                {
                    isLeftWideHit = true;
                }

            }
            if (isRightNarrowHit)
            {
                if (Physics.Raycast(offsetRightPosition, rightWideDir, keepDistanceSlow, arrivalLayerMask))
                {
                    isRightWideHit = true;
                }
            }

            Debug.DrawRay(narrowOffsetLeftPosition, transform.forward * (keepDistanceSlow - rayYOffset), Color.black);
            Debug.DrawRay(narrowOffsetRightPosition, transform.forward * (keepDistanceSlow - rayYOffset), Color.black);
            Debug.DrawRay(offsetLeftPosition, transform.forward * keepDistanceSlow, Color.black);
            Debug.DrawRay(offsetRightPosition, transform.forward * keepDistanceSlow, Color.black);
            Debug.DrawRay(offsetLeftPosition, leftNarrowDir.normalized * keepDistanceSlow, Color.black);
            Debug.DrawRay(offsetRightPosition, rightNarrowDir.normalized * keepDistanceSlow, Color.black);
            Debug.DrawRay(offsetLeftPosition, leftWideDir.normalized * keepDistanceSlow, Color.black);
            Debug.DrawRay(offsetRightPosition, rightWideDir.normalized * keepDistanceSlow, Color.black);
        }*/

    /*    void evading()
        {
            Debug.Log("Evading");
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

            if (distance < sqrKeepDistSlow / 3)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toLook), rotationSpeed * Time.deltaTime);
            }
            else
            {
                switchState(StateList.moving);
            }
            transform.Translate(0.0f, 0.0f, Time.deltaTime * speed);
        }*/

    // Call this when switch to Stop State
    public void SetStop(GameObject target)
    {
        stopTarget = target;
        SwitchState(StateList.stop);
    }

    // Check the distance between itself and target waypoint, increase the index if it is close enough
    private void CheckCurrentWayPoint()
    {
        Vector3 direction = path[currentWP].transform.position - transform.position;
        float sqrDistance = direction.sqrMagnitude;

        if (sqrDistance < sqrAccuracy)
        {
            currentWP++;
        }
    }

    // Reset the hit information boolean
    private void ResetHitBool()
    {
        isLeftNarrowStraightHit = false;
        isRightNarrowStraightHit = false;
        isLeftWideStraightHit = false;
        isRightWideStraightHit = false;
        isLeftNarrowHit = false;
        isRightNarrowHit = false;
        isLeftWideHit = false;
        isRightWideHit = false;
    }
}
