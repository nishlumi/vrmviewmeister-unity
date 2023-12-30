using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserHandleSpace
{
    public class ownscr_CrossKey : MonoBehaviour
    {
        [SerializeField]
        GameObject cameraset;
        [SerializeField]
        GameObject handL;
        [SerializeField]
        GameObject handR;

        public ManageAnimation manim;

        public float MoveRate = 0.01f;
        public float RotateRate = 0.5f;

        private Vector3 trackcurpos = Vector3.zero;
        public string MoveTarget = "c";
        public string OperationType = "translate"; //translate, rotate

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter(Collider collider)
        {


        }
        private void OnTriggerStay(Collider other)
        {
            if (
                (other.gameObject.name == "F1c") ||
                (other.gameObject.name == "thumb_tip") ||
                (other.gameObject.name == "index_finger_tip") ||
                (other.gameObject.name == "middle_finger_tip") ||
                (other.gameObject.name == "ring_finger_tip") ||
                (other.gameObject.name == "pinky_finger_tip")
            )
            {
                
                if (MoveTarget == "o")
                {
                    NativeAnimationAvatar nav = manim.GetCastInProject(manim.VRARSelectedAvatarName);
                    if (nav != null)
                    {
                        Transform tran = null;
                        if (nav.type == AF_TARGETTYPE.VRM)
                        {
                            tran = nav.ikparent.transform;
                        }
                        else
                        {
                            tran = nav.avatar.transform;
                        }

                        
                        if (tran != null)
                        {
                            /*
                            if (gameObject.name == "tp_forward")
                            {
                                tran.Translate(0, 0, 1 * MoveRate, Space.Self);
                            }
                            else if (gameObject.name == "tp_back")
                            {
                                tran.Translate(0, 0, -1 * MoveRate, Space.Self);
                            }
                            else if (gameObject.name == "tp_right")
                            {
                                tran.Translate(1 * MoveRate, 0, 0f, Space.Self);
                            }
                            else if (gameObject.name == "tp_left")
                            {
                                tran.Translate(-1 * MoveRate, 0, 0f, Space.Self);
                            }
                            else if (gameObject.name == "tp_up")
                            {
                                tran.Translate(0f, 1 * MoveRate, 0f, Space.Self);
                            }
                            else if (gameObject.name == "tp_down")
                            {
                                tran.Translate(0f, -1 * MoveRate, 0f, Space.Self);
                            }*/

                            if (OperationType == "translate")
                            {
                                TargetTranslate(tran, gameObject.name);
                            }
                            else if (OperationType == "rotate")
                            {
                                TargetRotate(tran, gameObject.name);
                            }
                            
                        }
                    }
                    
                }
                else if (MoveTarget == "c")
                {
                    if (OperationType == "translate")
                    {
                        TargetTranslate(cameraset.transform, gameObject.name);
                    }
                    else if (OperationType == "rotate")
                    {
                        TargetRotate(cameraset.transform, gameObject.name);
                    }
                    

                /*
                    if (gameObject.name == "tp_forward")
                    {
                        cameraset.transform.Translate(0, 0, 1 * MoveRate, Space.Self);
                    }
                    else if (gameObject.name == "tp_back")
                    {
                        cameraset.transform.Translate(0, 0, -1 * MoveRate, Space.Self);
                    }
                    else if (gameObject.name == "tp_right")
                    {
                        cameraset.transform.Translate(1 * MoveRate, 0, 0f, Space.Self);
                    }
                    else if (gameObject.name == "tp_left")
                    {
                        cameraset.transform.Translate(-1 * MoveRate, 0, 0f, Space.Self);
                    }
                    else if (gameObject.name == "tp_up")
                    {
                        cameraset.transform.Translate(0f, 1 * MoveRate, 0f, Space.Self);
                    }
                    else if (gameObject.name == "tp_down")
                    {
                        cameraset.transform.Translate(0f, -1 * MoveRate, 0f, Space.Self);
                    }*/
                }
                

            }


        }

        private void OnTriggerExit(Collider collider)
        {            

        }

        /// <summary>
        /// Translate the target camera/object on pushing button
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="btnname"></param>
        public void TargetTranslate(Transform tran, string btnname)
        {
            if (btnname == "tp_forward")
            {
                
                tran.Translate(Vector3.forward * MoveRate, Space.Self);
            }
            else if (btnname == "tp_back")
            {
                tran.Translate(Vector3.back * MoveRate, Space.Self);
            }
            else if (btnname == "tp_right")
            {
                tran.Translate(Vector3.right * MoveRate, Space.Self);
            }
            else if (btnname == "tp_left")
            {
                tran.Translate(Vector3.left * MoveRate, Space.Self);
            }
            else if (btnname == "tp_up")
            {
                tran.Translate(Vector3.up * MoveRate, Space.Self);
            }
            else if (btnname == "tp_down")
            {
                tran.Translate(Vector3.down * MoveRate, Space.Self);
            }
        }

        /// <summary>
        /// Rotate the target camera/object on pushing button
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="btnname"></param>
        public void TargetRotate(Transform tran, string btnname)
        {
            if (btnname == "tp_forward") //X axis
            {
                tran.Rotate(Vector3.forward * RotateRate, Space.Self);
            }
            else if (btnname == "tp_back") //X axis
            {
                tran.Rotate(Vector3.back * RotateRate, Space.Self);
            }
            else if (btnname == "tp_right") //Z axis
            {
                tran.Rotate(Vector3.right * RotateRate, Space.Self);
            }
            else if (btnname == "tp_left") //Z axis
            {
                tran.Rotate(Vector3.left * RotateRate, Space.Self);
            }
            else if (btnname == "tp_up") //Y axis
            {
                tran.Rotate(Vector3.up * RotateRate, Space.Self);
            }
            else if (btnname == "tp_down") //Y axis
            {
                tran.Rotate(Vector3.down * RotateRate, Space.Self);
            }
        }
    }

}
