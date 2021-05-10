using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeFormRotation : MonoBehaviour
{
    float initRotY;
    float rotSpeed = 20;
    float totalXRot = 0f;
    GameObject figure;
    void OnMouseDrag()
    {
        float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
        float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;
        totalXRot += rotX;
    }

    // Start is called before the first frame update
    void Start()
    {
        figure = transform.GetChild(0).gameObject;
        initRotY = transform.rotation.y;
    }

    // Update is called once per frame
    void Update()
    {
    }
    // doing late update after animation has been applied, to enable rotation
    void LateUpdate() {
        figure.transform.RotateAround(Vector3.back, -totalXRot);
    }
}
