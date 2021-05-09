using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class navigateMe : MonoBehaviour
{
    Rigidbody rb;
    // float speed = 0.5f; 
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey (KeyCode.Space)) {
            rb.useGravity = true;
        }
        // bool w = Input.GetKey(KeyCode.W);
        // bool a = Input.GetKey(KeyCode.A);
        // bool s = Input.GetKey(KeyCode.S);
        // bool d = Input.GetKey(KeyCode.D);
        // if (w) {
        //     Vector3 move = new Vector3(0, 0, 1) * speed *Time.deltaTime;
        //     rb.MovePosition(move);
        //     Debug.Log("Moved using w key");

        // }
        // if (s) {
        //     Vector3 move = new Vector3(0, 0, -1) * speed *Time.deltaTime;
        //     rb.MovePosition(move);
        //     Debug.Log("Moved using w key");

        // }
        // if (a) {
        //     Vector3 move = new Vector3(1, 0, 0) * speed *Time.deltaTime;
        //     rb.MovePosition(move);
        //     Debug.Log("Moved using w key");

        // }
        // if (w) {
        //     Vector3 move = new Vector3(-1, 0, 0) * speed *Time.deltaTime;
        //     rb.MovePosition(move);
        //     Debug.Log("Moved using w key");

        // }

    }
}
