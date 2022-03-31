using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

[RequireComponent(typeof(Hand))]
public class DistanceGrabHand : MonoBehaviour
{
    // Direction pointer for the distance grab
    public Transform pointer;

    // Mask for grabbable objects
    public LayerMask grabbableObjectsMask;

    // Speed of object going to the hand when it gets grabbed from distance
    public float pullSpeed;

    // Maximum distance from the object for the distance grab
    public float maxGrabDistance;

    // Radius of the sphere to search for distance grabbable objects
    public float searchSphereRadius;

    // Maximum distance for the object to stop lerping to the hand
    public float stopLerpDistance;

    // Lerp data
    float startTime;
    float journeyLength;

    Hand hand;
    bool isAttached = false;
    GameObject grabbableObject = null;
    Vector3 pullStartPos;
    DistanceGrabbableObject grabbableObjectScript;
    DistanceLinearDrive distanceLinearDrive;

    void Start()
    {
        hand = GetComponent<Hand>();
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        //Debug.DrawRay(pointer.position, pointer.forward, Color.red);

        // If an object is attached
        if (isAttached)
        {
            // If hand is still in holding state
            GrabTypes endingGrabType = hand.GetGrabEnding();
            if (endingGrabType == GrabTypes.None)
            {
                if (!grabbableObjectScript.usesLinearMapping) // If not linear mapped object
                {
                    // Lerping the object
                    journeyLength = Vector3.Distance(pullStartPos, transform.position);
                    float distCovered = (Time.time - startTime) * pullSpeed;
                    float fractionOfJourney = distCovered / journeyLength;
                    grabbableObject.transform.position = Vector3.Lerp(grabbableObject.transform.position, transform.position, fractionOfJourney);

                    // If the object is close enough to the hand - reattach it to standard grab script
                    if (Vector3.Distance(grabbableObject.transform.position, transform.position) < stopLerpDistance)
                    {
                        hand.AttachObject(grabbableObject, GrabTypes.Grip);
                        grabbableObject = null;
                        isAttached = false;
                    }
                }
                else // If linear mapped object, update it
                {
                    distanceLinearDrive.DistanceHandUpdate(hand);
                }
            }
            else
            {
                // Detaching the object
                grabbableObject = null;
                isAttached = false;

                if(grabbableObjectScript.usesLinearMapping)
                {
                    distanceLinearDrive.OnDetachment(hand);
                }
            }
        }
        else if (hand.currentAttachedObject == null) // Searches for new objects only if hand doesn't hold any objects
        {
            // Uses sphere cast to find an object to grab
            // If an object found and it is distance grabbable
            if (Physics.SphereCast(pointer.position, searchSphereRadius, pointer.forward, out hit, maxGrabDistance, grabbableObjectsMask) &&
                    hit.collider.GetComponent<DistanceGrabbableObject>().isDistGrabbable)
            {
                grabbableObject = hit.collider.gameObject;
                GrabTypes startingGrabType = hand.GetGrabStarting();

                if (grabbableObject.GetComponent<Interactable>() != null && startingGrabType != GrabTypes.None)
                {
                    // Attaching the object
                    isAttached = true;

                    grabbableObjectScript = grabbableObject.GetComponent<DistanceGrabbableObject>();

                    // Check if the object uses linear mapping
                    if (!grabbableObjectScript.usesLinearMapping)
                    {
                        // Starting Lerp
                        startTime = Time.time;
                        pullStartPos = grabbableObject.transform.position;
                        journeyLength = Vector3.Distance(pullStartPos, transform.position);
                    }
                    else
                    {
                        // Get Linear Drive script of the object and begin linear mapping
                        distanceLinearDrive = grabbableObject.GetComponent<DistanceLinearDrive>();
                        distanceLinearDrive.OnAttachment(hand);
                    }

                }

                // Highlight the object
                hit.collider.GetComponent<DistanceGrabbableObject>().HighlightObject();
            }
        }
    }
}