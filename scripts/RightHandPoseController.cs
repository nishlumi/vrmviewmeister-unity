using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHandPoseController : HandPoseController
{
    override protected void Awake()
    {
        base.Awake();
    }

    override protected void Start()
    {
        base.Start();
        switchLeft = false;
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