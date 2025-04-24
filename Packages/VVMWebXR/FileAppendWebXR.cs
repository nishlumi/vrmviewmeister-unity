using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;

namespace UserVRMSpace
{
    public class FileAppendWebXR : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void Test()
        {

        }
        public static GameObject AppendMouseDragComponent(GameObject target)
        {
            target.AddComponent<WebXR.Interactions.MouseDragObject>();
            return target;
        }
    }

}
