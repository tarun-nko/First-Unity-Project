using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTransition : MonoBehaviour
{
    public GameObject disk;
    public GameObject figure;
    public Camera camera;
    Animator cameraAnimator;
    Animator diskAnimator;
    Animator figureAnimator;
    Rigidbody figureRb;
    int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        cameraAnimator = camera.GetComponent<Animator>();
        diskAnimator = disk.GetComponentInChildren<Animator> ();
        figureAnimator = figure.GetComponentInChildren<Animator> ();
        figureRb = figure.GetComponent<Rigidbody> ();
    }

    // Update is called once per frame
    void Update()
    {
        AnimatorTransitionInfo currentTransition = diskAnimator.GetAnimatorTransitionInfo(0);
        AnimatorTransitionInfo currentTransition1 = figureAnimator.GetAnimatorTransitionInfo(0);

        if(Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log(++count);
            cameraAnimator.SetTrigger("action");
            diskAnimator.SetTrigger("change");
            figureAnimator.SetTrigger("change");
            figure.transform.rotation = Quaternion.Euler(0, 0, 0);
            figure.transform.position = new Vector3(0f, 1.603f, 2f);
            if (figureRb.useGravity)
                figureRb.useGravity = false;
            if (figureRb.constraints == (RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY) || 
                figureRb.constraints == RigidbodyConstraints.None) {
                figureRb.constraints = RigidbodyConstraints.FreezePositionX |  RigidbodyConstraints.FreezePositionY |  RigidbodyConstraints.FreezePositionZ |
                 RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            if (figureAnimator.GetCurrentAnimatorStateInfo(0).IsName("New Animation")) {
                Debug.Log("I am here");
                cameraAnimator.SetTrigger("action");
                figureRb.useGravity = true;
                figureRb.constraints =  RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            }            
        }
    }
}
