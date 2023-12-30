using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHandPoseController : HandPoseController
{

    override protected void  Awake()
    {
        base.Awake();
        switchLeft = true;
    }

    override protected void Start()
    {
        base.Start();
    }

    override protected void Update()
    {
        base.Update();

    }

    override protected void LateUpdate()
    {
        base.LateUpdate();
    }

}