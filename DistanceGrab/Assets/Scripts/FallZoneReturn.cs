using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallZoneReturn : MonoBehaviour
{
    Vector3 startPos;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Calls are received from FallZone script
    void OnFall()
    {
        Debug.Log("Out of bounds: " + gameObject.name);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPos;
    }
}
