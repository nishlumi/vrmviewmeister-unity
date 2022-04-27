using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleOperation : MonoBehaviour
{
    public Texture areaTex;
    public GUIStyle style;
    public Vector3 pos = Vector3.zero;
    public Quaternion rot = Quaternion.identity;
    public Vector3 scale = Vector3.one;
    public Transform[] points;
    public Vector3 lookAtPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DataUpdate()
    {
        transform.rotation = rot;
        transform.position = pos;
        transform.localScale = scale;
    }
    public void LookAtUpdate()
    {
        transform.LookAt(lookAtPoint);
    }
}
