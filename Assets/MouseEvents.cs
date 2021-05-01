using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEvents : MonoBehaviour
{
    private bool mouseDown = false;
    void OnMouseDown(){
        Debug.Log("Mouse Down");
        mouseDown = true;
    }
    void OnMouseUp(){
        Debug.Log("Mouse Up");
        mouseDown = false;
    }
    void OnMouseEnter(){
        Debug.Log("Mouse Enter");
    }
    void OnMouseExit(){
        Debug.Log("Mouse leaving");
    }
}
