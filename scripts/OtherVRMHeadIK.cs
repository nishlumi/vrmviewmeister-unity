using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherVRMHeadIK : MonoBehaviour
{
    public Transform TargetHead;
    public Transform TargetNeck;

    public GameObject GoalObject;

    [SerializeField] private Vector3 _forward = Vector3.forward;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = GoalObject.transform.position = TargetNeck.position;

        Quaternion lookAtRotation = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion offsetRotation = Quaternion.FromToRotation(_forward, Vector3.forward);

        TargetNeck.rotation = lookAtRotation * offsetRotation;

    }
}
