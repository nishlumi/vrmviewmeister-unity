using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;

namespace UserHandleSpace
{
    public class CameraMoverXR : MonoBehaviour
    {
        [SerializeField]
        WebXRManager xman;
        [SerializeField]
        GameObject cameras;
        [SerializeField]
        WebXRController conleft;
        [SerializeField]
        WebXRController conright;

        public float moverate = 0.01f;
        public float rotaterate = 0.5f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            CamMoving();
            CamRotating();

        }
        void CamMoving()
        {
            float realmoverate = moverate;
            Vector2 v2 = conleft.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
            if (conright.GetButton(WebXRController.ButtonTypes.ButtonA))
            {
                //transform.Translate(v2.x * moverate, v2.y * moverate, 0, Space.Self);
                realmoverate = moverate * 2.0f;
            }


            {
                transform.Translate(v2.x * realmoverate, 0, v2.y * realmoverate, Space.Self);
            }

        }
        void CamRotating()
        {
            Vector2 v2 = conright.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
            transform.Rotate(0, v2.x, 0);
            transform.Translate(0, v2.y * rotaterate, 0, Space.Self);
        }
    }
}

