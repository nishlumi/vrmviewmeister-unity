using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserHandleSpace
{
    public class ownscr_HandMenuButton : MonoBehaviour
    {
        [SerializeField]
        private ownscr_HandMenu HandMenu;

        private bool IsPushingBtn1;
        private bool ToggleButtonState;
        private bool ToggleShaderCutout;

        [SerializeField]
        private GameObject imgon;
        [SerializeField]
        private GameObject imgoff;

        // Start is called before the first frame update
        void Start()
        {
            IsPushingBtn1 = false;
            ToggleButtonState = true;
            ToggleShaderCutout = false;
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter(Collider other)
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
                IsPushingBtn1 = true;
                if (
                    (gameObject.name != "hmbtn_mt_camera") &&
                    (gameObject.name != "hmbtn_mt_obj") &&
                    (gameObject.name != "hmbtn_ope_move") &&
                    (gameObject.name != "hmbtn_ope_rot") &&
                    (gameObject.name != "hmbtn_ope_size") &&
                    (gameObject.name != "HandMenuPlayPauser")
                 )
                {
                    DownEvent();
                }
                
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (IsPushingBtn1 == true)
            {
                if (
                    (gameObject.name != "hmbtn_mt_camera") &&
                    (gameObject.name != "hmbtn_mt_obj") &&
                    (gameObject.name != "hmbtn_ope_move") &&
                    (gameObject.name != "hmbtn_ope_rot") &&
                    (gameObject.name != "hmbtn_ope_size") &&
                    (gameObject.name != "HandMenuPlayPauser")
                 )
                {
                    UpEvent();
                }
                ExecuteEvent(gameObject.name);
                IsPushingBtn1 = false;
            }
        }
        public void DownEvent()
        {
            
            var bkup = transform.localPosition;
            bkup.y = 0.25f;
            transform.localPosition = bkup;
        }
        public void UpEvent()
        {
            
            var bkup = transform.localPosition;
            bkup.y = 0.5f;
            transform.localPosition = bkup;
        }
        public int ExecuteEvent(string btnname)
        {
            int ret = 0;

            if (btnname == "hmbtn_prevselobj")
            { //---select previous 3D cast
                HandMenu.TurnPreviousObject();

            }
            else if(btnname == "hmbtn_nextselobj")
            { //---select next 3D cast
                HandMenu.TurnNextObject();
                
            }
            else if (btnname == "hmbtn_showik")
            {
                ToggleButtonState = !ToggleButtonState;
                //Transform imgon = transform.Find("img_on");
                //if (imgon != null)
                {
                    if (ToggleButtonState == true)
                    {
                        //transform.Find("img_on").Translate(0, 0.6f, 0, Space.Self);
                        //imgon.transform.localPosition = new Vector3(imgon.transform.localPosition.x, 0.6f, imgon.transform.localPosition.z);
                        if (imgon != null) imgon.SetActive(true);
                        if (imgoff != null) imgoff.SetActive(false);
                    }
                    else
                    {
                        //transform.Find("img_on").Translate(0, -0.6f, 0, Space.Self);
                        //imgon.transform.localPosition = new Vector3(imgon.transform.localPosition.x, 0, imgon.transform.localPosition.z);
                        if (imgon != null) imgon.SetActive(false);
                        if (imgoff != null) imgoff.SetActive(true);

                    }
                }                
                HandMenu.ChangeIKMarkerView(ToggleButtonState);
            }
            else if (btnname == "hmbtn_oncutout")
            {
                ToggleShaderCutout = !ToggleShaderCutout;
                HandMenu.ChangeShaderCutout(ToggleShaderCutout);
            }
            else if (btnname == "hmbtn_regkeyframe")
            {
                HandMenu.RegisterKeyFrame();

            }
            else if (btnname == "hmbtn_prevkeyframe")
            {
                HandMenu.ChangePreviousKeyFrame();

            }
            else if (btnname == "hmbtn_nextkeyframe")
            {
                HandMenu.ChangeNextKeyFrame();
            }
            else if (btnname == "hmbtn_motplay")
            {
                HandMenu.PlayAnimation();
            }
            else if (btnname == "hmbtn_motstop")
            {
                HandMenu.StopAnimation();
            }
            else if (btnname == "HandMenuPlayPauser")
            {
                HandMenu.StartPauseAnimation();
            }
            else if (btnname == "hmbtn_mt_camera")
            {
                HandMenu.ChangeMoveTarget("c");
            }
            else if (btnname == "hmbtn_mt_obj")
            {
                HandMenu.ChangeMoveTarget("o");
            }
            else if (btnname == "hmbtn_ope_move")
            {
                HandMenu.ChangeMoveOperationType("translate");
            }
            else if (btnname == "hmbtn_ope_rot")
            {
                HandMenu.ChangeMoveOperationType("rotate");
            }
            else if (btnname == "hmbtn_ope_size")
            {
                HandMenu.ChangeMoveOperationType("size");
            }
            else if (btnname == "hmbtn_mt_reset_pos")
            {
                HandMenu.ResetTransformCurrentObject(true,false,false);
            }
            else if (btnname == "hmbtn_mt_reset_rot")
            {
                HandMenu.ResetTransformCurrentObject(false, true,false);
            }
            else if (btnname == "hmbtn_mt_reset_size") 
            {
                HandMenu.ResetTransformCurrentObject(false, false, true);
            }
            else if (btnname == "hmbtn_exitvrar")
            {
                HandMenu.EndVRAR();
            }

            return ret;
        }
    }

}
