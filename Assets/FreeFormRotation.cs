using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeFormRotation : MonoBehaviour
{
    float initRotY;
    float rotSpeed = 20;
    float totalYRot = 0f;
    void OnMouseDrag()
    {
        float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
        float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;
        Debug.Log("ROT X : " + rotX);
        transform.RotateAround(Vector3.back, -rotX);
    }

    // Start is called before the first frame update
    void Start()
    {
        initRotY = transform.rotation.y;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
