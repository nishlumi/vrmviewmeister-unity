using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteTransform : MonoBehaviour
{
    public Transform target;
    public bool EnableTranslate = true;
    public bool EnableRotate = true;
    public bool EnableScale = false;
    public Space targetSpace = Space.Self;

    private Vector3 oldPosition = Vector3.zero;
    private Quaternion oldRotation = Quaternion.identity;
    private Vector3 oldScale = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        if (EnableTranslate)
        {
            if (targetSpace == Space.World)
            {
                if (target.position !=  oldPosition)
                {
                    target.position = transform.position;
                }
                

                oldPosition = transform.position;
            }
            else
            {
                if (target.localPosition != oldPosition)
                {
                    target.localPosition = transform.localPosition;
                }
                

                oldPosition = transform.localPosition;
            }
        }
        if (EnableRotate)
        {
            if (targetSpace == Space.World)
            {
                if (target.rotation != oldRotation)
                {
                    target.rotation = transform.rotation;
                }
                

                oldRotation = transform.rotation;
            }
            else
            {
                if (target.localRotation != oldRotation)
                {
                    target.localRotation = transform.localRotation;
                }
                

                oldRotation = transform.localRotation;
            }
        }
        if (EnableScale) 
        {
            if (target.localScale != oldScale)
            {
                target.localScale = transform.localScale;
            }
            

            oldScale = transform.localScale;
        }

        
    }
}
