using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCollision : MonoBehaviour
{
    public Transform feet;

    CapsuleCollider bodyCollider;

    // Start is called before the first frame update
    void Start()
    {
        bodyCollider = gameObject.GetComponent<CapsuleCollider>();
        Physics.IgnoreLayerCollision(0, 9); // Ignore collision of body and non-wall/floor objects
        Physics.IgnoreLayerCollision(10, 9);  // Ignore collision of body and hands
    }

    // Update is called once per frame
    void Update()
    {
        float distanceFromFloor = Vector3.Dot(feet.localPosition, Vector3.up);
        bodyCollider.height = Mathf.Max(bodyCollider.radius, distanceFromFloor);
        transform.localPosition = feet.localPosition - 0.5f * distanceFromFloor * Vector3.up;

        transform.position = new Vector3(feet.position.x, feet.transform.position.y - 0.5f * distanceFromFloor, feet.position.z);
    }
}
