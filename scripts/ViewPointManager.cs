using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UserHandleSpace;

namespace UserHandleSpace
{
    public class ViewPointManager : MonoBehaviour
    {
        Vector3 oldposition;
        public TMPro.TextMeshProUGUI DistCam2ObjView;

        OperateActiveVRM oavrm;
        [SerializeField]
        ManageAnimation manim;
        // Start is called before the first frame update
        void Start()
        {
            oldposition = Vector3.zero;
            GameObject ikparent = GameObject.Find("IKHandleParent");
            oavrm = ikparent.GetComponent<OperateActiveVRM>();
        }

        // Update is called once per frame
        void Update()
        {
            if (manim.IsVRAR()) return;

            if (oldposition != transform.position)
            {
                if (oavrm.ActiveAvatar != null)
                {
                    //Debug.Log(oavrm.ActiveAvatar.gameObject.name);
                    //float dist = (transform.position - oavrm.ActiveAvatar.transform.position).sqrMagnitude;
                    float dist2 = Vector3.Distance(transform.position, oavrm.ActiveAvatar.transform.position);

                    DistCam2ObjView.text = dist2.ToString("N3");
                    //Debug.Log("distance=" + dist.ToString() + " / " + dist2.ToString());
                }
            }
            

            oldposition = transform.position;
        }
        
    }
}

