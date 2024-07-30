using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

using UserHandleSpace;

namespace UserUISpace
{
    public class UserUIARWin : MonoBehaviour
    {
        [SerializeField]
        GameObject cameraset;

        [SerializeField]
        protected ManageAnimation manim;

        [SerializeField]
        CameraManagerXR cameraManager;

        [SerializeField]
        Camera arcam;

        [SerializeField]
        ScreenShot sshot;

        private UIDocument MainUI;
        private VisualElement rootElement;
        private VisualElement LeftPanel;
        private VisualElement RightPanel;
        private VisualElement MiniPanel;
        private VisualElement BonePanel;
        private Label arlab_selectavatar;
        private Label arlab_keynumber;

        private ARMenuFunction armf;

        private bool showik_state;
        private bool cutout_state;
        private bool is_minimize = false;
        private bool pushed_tp_right = false;
        private bool pushed_tp_left = false;
        private bool pushed_tp_up = false;
        private bool pushed_tp_down = false;
        private bool pushed_tp_forward = false;
        private bool pushed_tp_back = false;

        // Start is called before the first frame update
        void Start()
        {
            MainUI = GetComponent<UIDocument>();
            rootElement = MainUI.rootVisualElement;


            armf = new ARMenuFunction(manim, manim.ikArea.GetComponent<OperateActiveVRM>(), cameraManager);
            armf.cameraset = cameraset;
            GenerateUI();

            showik_state = true;
            cutout_state = false;

            ShowUI(false);
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void FixedUpdate()
        {
            if (pushed_tp_right) armf.TargetOperationBody("tp_right");
            if (pushed_tp_left) armf.TargetOperationBody("tp_left");
            if (pushed_tp_up) armf.TargetOperationBody("tp_up");
            if (pushed_tp_down) armf.TargetOperationBody("tp_down");
            if (pushed_tp_forward) armf.TargetOperationBody("tp_forward");
            if (pushed_tp_back) armf.TargetOperationBody("tp_back");

            if (cameraManager.isActiveNormal() == true)
            {
                ShowButtonOtherThanVRM(false);
            }

        }

        void GenerateUI()
        {
            LeftPanel = rootElement.Q<VisualElement>("panel_operate");
            RightPanel = rootElement.Q<VisualElement>("panel_transform");
            MiniPanel = rootElement.Q<VisualElement>("panel_minimize");
            BonePanel = rootElement.Q<VisualElement>("panel_vrm");
            arlab_selectavatar = LeftPanel.Q<Label>("arlab_selectavatar");
            arlab_keynumber = LeftPanel.Q<Label>("arlab_keynumber");

            //---MiniPanel
            MiniPanel.Q<Button>("arbtn_folding").clicked += arbtn_folding_OnClick;
            //MiniPanel.Q<Button>("arbtn_motplay2").clicked += arbtn_motplay_OnClick;
            //MiniPanel.Q<Button>("arbtn_motstop2").clicked += arbtn_motstop_OnClick;
            MiniPanel.Q<Button>("arbtn_leftpanel").clicked += arbtn_leftpanel_OnClick;
            MiniPanel.Q<Button>("arbtn_rightpanel").clicked += arbtn_rightpanel_OnClick;

            //---Left: operate panel
            LeftPanel.Q<Button>("arbtn_prevselobj").clicked += arbtn_prevselobj_OnClick;
            LeftPanel.Q<Button>("arbtn_nextselobj").clicked += arbtn_nextselobj_OnClick;
            LeftPanel.Q<Button>("arbtn_showik").clicked += arbtn_showik_OnClick;
            LeftPanel.Q<Button>("arbtn_oncutout").clicked += arbtn_oncutout_OnClick;
            LeftPanel.Q<Button>("arbtn_globallocal").clicked += arbtn_globallocal_OnClick;
            LeftPanel.Q<Button>("arbtn_prevkeyframe").clicked += arbtn_prevkeyframe_OnClick;
            LeftPanel.Q<Button>("arbtn_nextkeyframe").clicked += arbtn_nextkeyframe_OnClick;
            LeftPanel.Q<Button>("arbtn_regkeyframe").clicked += regkeyframe_OnClick;
            LeftPanel.Q<Button>("arbtn_motplay").clicked += arbtn_motplay_OnClick;
            LeftPanel.Q<Button>("arbtn_motstop").clicked += arbtn_motstop_OnClick;

            //---Right: transform panel
            RightPanel.Q<Button>("arbtn_mt_camera").clicked += arbtn_mt_camera_OnClick;
            RightPanel.Q<Button>("arbtn_mt_obj").clicked += arbtn_mt_obj_OnClick;
            RightPanel.Q<Button>("arbtn_mt_vrmbone").clicked += arbtn_mt_vrmbone_OnClick;
            RightPanel.Q<Button>("arbtn_obj_move").clicked += arbtn_obj_move_OnClick;
            RightPanel.Q<Button>("arbtn_obj_rot").clicked += arbtn_obj_rot_OnClick;
            RightPanel.Q<Button>("arbtn_obj_size").clicked += arbtn_obj_size_OnClick;
            RightPanel.Q<Button>("arbtn_reset_move").clicked += arbtn_reset_move_OnClick;
            RightPanel.Q<Button>("arbtn_reset_rot").clicked += arbtn_reset_rot_OnClick;
            RightPanel.Q<Button>("arbtn_reset_size").clicked += arbtn_reset_size_OnClick;

            //---X+
            RightPanel.Q<Button>("tpbt_xplus").RegisterCallback<PointerUpEvent>(tp_right_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_xplus").RegisterCallback<PointerDownEvent>(tp_right_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_xplus").RegisterCallback<PointerUpEvent>(tp_right_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_xplus").RegisterCallback<PointerDownEvent>(tp_right_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_xplus").RegisterCallback<PointerUpEvent>(tp_right_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_xplus").RegisterCallback<PointerDownEvent>(tp_right_PointerDownEvent, TrickleDown.TrickleDown);
            //---X-
            RightPanel.Q<Button>("tpbt_xmin").RegisterCallback<PointerUpEvent>(tp_left_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_xmin").RegisterCallback<PointerDownEvent>(tp_left_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_xmin").RegisterCallback<PointerUpEvent>(tp_left_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_xmin").RegisterCallback<PointerDownEvent>(tp_left_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_xmin").RegisterCallback<PointerUpEvent>(tp_left_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_xmin").RegisterCallback<PointerDownEvent>(tp_left_PointerDownEvent, TrickleDown.TrickleDown);
            //---Y+
            RightPanel.Q<Button>("tpbt_yplus").RegisterCallback<PointerUpEvent>(tp_up_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_yplus").RegisterCallback<PointerDownEvent>(tp_up_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_yplus").RegisterCallback<PointerUpEvent>(tp_up_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_yplus").RegisterCallback<PointerDownEvent>(tp_up_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_yplus").RegisterCallback<PointerUpEvent>(tp_up_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_yplus").RegisterCallback<PointerDownEvent>(tp_up_PointerDownEvent, TrickleDown.TrickleDown);
            //---Y-
            RightPanel.Q<Button>("tpbt_ymin").RegisterCallback<PointerUpEvent>(tp_down_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_ymin").RegisterCallback<PointerDownEvent>(tp_down_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_ymin").RegisterCallback<PointerUpEvent>(tp_down_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_ymin").RegisterCallback<PointerDownEvent>(tp_down_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_ymin").RegisterCallback<PointerUpEvent>(tp_down_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_ymin").RegisterCallback<PointerDownEvent>(tp_down_PointerDownEvent, TrickleDown.TrickleDown);
            //---Z+
            RightPanel.Q<Button>("tpbt_zplus").RegisterCallback<PointerUpEvent>(tp_forward_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_zplus").RegisterCallback<PointerDownEvent>(tp_forward_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_zplus").RegisterCallback<PointerUpEvent>(tp_forward_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_zplus").RegisterCallback<PointerDownEvent>(tp_forward_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_zplus").RegisterCallback<PointerUpEvent>(tp_forward_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_zplus").RegisterCallback<PointerDownEvent>(tp_forward_PointerDownEvent, TrickleDown.TrickleDown);
            //---Z-
            RightPanel.Q<Button>("tpbt_zmin").RegisterCallback<PointerUpEvent>(tp_back_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_zmin").RegisterCallback<PointerDownEvent>(tp_back_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_zmin").RegisterCallback<PointerUpEvent>(tp_back_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_zmin").RegisterCallback<PointerDownEvent>(tp_back_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_zmin").RegisterCallback<PointerUpEvent>(tp_back_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_zmin").RegisterCallback<PointerDownEvent>(tp_back_PointerDownEvent, TrickleDown.TrickleDown);

            //---BonePanel
            //BonePanel.Q<Button>("arbtn_bn_ikparent").clicked += arbtn_bn_ikparent_OnClick;
            BonePanel.Q<Button>("arbtn_bn_head").clicked += arbtn_bn_head_OnClick;
            BonePanel.Q<Button>("arbtn_bn_eyeviewhandle").clicked += arbtn_bn_eyeviewhandle_OnClick;
            BonePanel.Q<Button>("arbtn_bn_lookat").clicked += arbtn_bn_lookat_OnClick;
            BonePanel.Q<Button>("arbtn_bn_chest").clicked += arbtn_bn_chest_OnClick;
            BonePanel.Q<Button>("arbtn_bn_aim").clicked += arbtn_bn_aim_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightshoulder").clicked += arbtn_bn_rightshoulder_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftshoulder").clicked += arbtn_bn_leftshoulder_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightlowerarm").clicked += arbtn_bn_rightlowerarm_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftlowerarm").clicked += arbtn_bn_leftlowerarm_OnClick;
            BonePanel.Q<Button>("arbtn_bn_pelvis").clicked += arbtn_bn_pelvis_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftarm").clicked += arbtn_bn_lefthand_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightarm").clicked += arbtn_bn_righthand_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftlowerleg").clicked += arbtn_bn_leftlowerleg_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightlowerleg").clicked += arbtn_bn_rightlowerleg_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftleg").clicked += arbtn_bn_leftleg_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightleg").clicked += arbtn_bn_rightleg_OnClick;
        }

        public void ShowUI(bool flag)
        {
            //rootElement.visible = flag;

            {
                LeftPanel.visible = flag;
                RightPanel.visible = flag;
                MiniPanel.visible = flag;
                if (flag == false)
                {
                    BonePanel.visible = flag;
                }
            }

            //MiniPanel.Q<Button>("arbtn_motplay2").visible = flag;
            //MiniPanel.Q<Button>("arbtn_motstop2").visible = flag;
            MiniPanel.Q<Button>("arbtn_folding").visible = flag;
            is_minimize = false;
        }
        public void ShowVRMBonePanel(bool flag)
        {
            BonePanel.visible = flag;
        }
        public void ShowVRMButton(bool flag)
        {
            RightPanel.Q<Button>("arbtn_mt_vrmbone").visible = flag;
        }
        public void ShowButtonOtherThanVRM(bool flag)
        {
            if (flag == true)
            {
                RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Visible;
                RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Visible;
            }
            else
            {
                RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
                RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;
            }
        }
        public void SetObjectTitle(string title)
        {
            arlab_selectavatar.text = title;
        }
        public void SetObjectInformation(string title, AF_TARGETTYPE type)
        {
            arlab_selectavatar.text = title;
            armf.SelectedAvatarType = type;
        }
        //########################################################################################################
        void arbtn_folding_OnClick()
        {
            is_minimize = !is_minimize;

            if (is_minimize == true)
            {
                //LeftPanel.visible = false;
                //RightPanel.visible = false;
                //MiniPanel.Q<Button>("arbtn_motplay2").visible = true;
                //MiniPanel.Q<Button>("arbtn_motstop2").visible = true;
            }
            else
            {
                //LeftPanel.visible = true;
                //RightPanel.visible = true;
                //MiniPanel.Q<Button>("arbtn_motplay2").visible = false;
                //MiniPanel.Q<Button>("arbtn_motstop2").visible = false;
            }
            armf.StartPauseAnimation();
        }
        void arbtn_leftpanel_OnClick()
        {
            if (LeftPanel.visible == true)
            {
                LeftPanel.visible = false;
            }
            else
            {
                LeftPanel.visible = true;
            }
        }
        void arbtn_rightpanel_OnClick()
        {
            if (RightPanel.visible == true)
            { //---to hide
                RightPanel.visible = false;
                if (BonePanel.visible == true)
                {
                    ShowVRMBonePanel(false);
                }
                if (armf.SelectedAvatarType == AF_TARGETTYPE.VRM)
                {
                    ShowVRMButton(false);
                }
                
                RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
                RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;

            }
            else
            { //---to show
                RightPanel.visible = true;
                if (armf.SelectedAvatarType == AF_TARGETTYPE.VRM)
                {
                    if (armf.GetMoveTarget() == "b")
                    { //-- oldly select BonePanel, recover selected states.
                        ShowVRMBonePanel(true);
                    }
                    ShowVRMButton(true);
                }
                else
                {
                    ShowVRMBonePanel(false);
                }

                if (armf.GetMoveTarget() == "o")
                {
                    RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Visible;
                    RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Visible;

                }
                else
                {
                    RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
                    RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;

                }
            }
        }

        //########################################################################################################
        void arbtn_prevselobj_OnClick()
        {
            arlab_selectavatar.text = armf.TurnPreviousObject(arlab_selectavatar.text);
            if (armf.SelectedAvatarType == AF_TARGETTYPE.VRM)
            {
                ShowVRMButton(true);
            }
            else
            {
                ShowVRMButton(false);
            }
        }
        void arbtn_nextselobj_OnClick()
        {
            arlab_selectavatar.text = armf.TurnNextObject(arlab_selectavatar.text);
            if (armf.SelectedAvatarType == AF_TARGETTYPE.VRM)
            {
                ShowVRMButton(true);
            }
            else
            {
                ShowVRMButton(false);
            }
        }
        void arbtn_showik_OnClick()
        {
            showik_state = !showik_state;
            armf.ChangeIKMarkerView(showik_state);
        }
        void arbtn_oncutout_OnClick()
        {
            cutout_state = !cutout_state;
            armf.ChangeShaderCutout(cutout_state);
        }
        void arbtn_globallocal_OnClick()
        {
            if (LeftPanel.Q<Button>("arbtn_globallocal").text == "L")
            {
                armf.ChangeSpaceType(Space.World);
                LeftPanel.Q<Button>("arbtn_globallocal").text = "G";
            }
            else if (LeftPanel.Q<Button>("arbtn_globallocal").text == "G")
            {
                armf.ChangeSpaceType(Space.Self);
                LeftPanel.Q<Button>("arbtn_globallocal").text = "L";
            }
        }
        public void LabelWriteKeyFrame(int index)
        {
            arlab_keynumber.text = index.ToString();
        }
        void arbtn_prevkeyframe_OnClick()
        {
            int val = armf.ChangePreviousKeyFrame();
            arlab_keynumber.text = val.ToString();
        }
        void arbtn_nextkeyframe_OnClick()
        {
            int val = armf.ChangeNextKeyFrame();
            arlab_keynumber.text = val.ToString();
        }
        void regkeyframe_OnClick()
        {
            armf.RegisterKeyFrame();
        }
        void arbtn_motplay_OnClick()
        {
            armf.StartPauseAnimation();
        }
        void arbtn_motstop_OnClick()
        {
            armf.StopAnimation();
        }
        //#######################################################################################################3
        void arbtn_mt_camera_OnClick()
        {
            armf.ChangeMoveTarget("c");
            ChangeStyleBoneBtn("", true);
            RightPanel.Q<Button>("arbtn_mt_camera").style.backgroundColor = new Color(41, 123, 214, 255);
            RightPanel.Q<Button>("arbtn_mt_obj").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_mt_vrmbone").style.backgroundColor = new Color(41, 123, 214, 0);

            ShowVRMBonePanel(false);
            RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
            RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;
        }
        void arbtn_mt_obj_OnClick()
        {
            ChangeStyleBoneBtn("", true);
            armf.ChangeMoveTarget("o");
            RightPanel.Q<Button>("arbtn_mt_camera").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_mt_obj").style.backgroundColor = new Color(41, 123, 214, 255);
            RightPanel.Q<Button>("arbtn_mt_vrmbone").style.backgroundColor = new Color(41, 123, 214, 0);

            ShowVRMBonePanel(false);
            RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Visible;
            RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Visible;
        }
        void arbtn_mt_vrmbone_OnClick()
        {
            armf.ChangeMoveTarget("b");
            ChangeStyleBoneBtn("", true);
            RightPanel.Q<Button>("arbtn_mt_vrmbone").style.backgroundColor = new Color(41, 123, 214, 255);

            ShowVRMBonePanel(true);

            //--- bone ik marker, not show size buttons          
            RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
            RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;            
           
        }
        void arbtn_obj_move_OnClick()
        {
            armf.ChangeMoveOperationType("translate");
            RightPanel.Q<Button>("arbtn_obj_move").style.backgroundColor = new Color(41, 123, 214, 255);
            RightPanel.Q<Button>("arbtn_obj_rot").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_obj_size").style.backgroundColor = new Color(41, 123, 214, 0);

            rootElement.Q<VisualElement>("tpbox_tran1").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_tran2").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_rot1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_rot2").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_size1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_size2").style.display = DisplayStyle.None;
        }
        void arbtn_obj_rot_OnClick()
        {
            armf.ChangeMoveOperationType("rotate");
            RightPanel.Q<Button>("arbtn_obj_move").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_obj_rot").style.backgroundColor = new Color(41, 123, 214, 255);
            RightPanel.Q<Button>("arbtn_obj_size").style.backgroundColor = new Color(41, 123, 214, 0);

            rootElement.Q<VisualElement>("tpbox_tran1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_tran2").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_rot1").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_rot2").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_size1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_size2").style.display = DisplayStyle.None;

        }
        void arbtn_obj_size_OnClick()
        {
            armf.ChangeMoveOperationType("size");
            RightPanel.Q<Button>("arbtn_obj_move").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_obj_rot").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_obj_size").style.backgroundColor = new Color(41, 123, 214, 255);

            rootElement.Q<VisualElement>("tpbox_tran1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_tran2").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_rot1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_rot2").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_size1").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_size2").style.display = DisplayStyle.Flex;
        }
        void arbtn_reset_move_OnClick()
        {
            armf.ResetTransformCurrentObject(true, false, false);
        }
        void arbtn_reset_rot_OnClick()
        {
            armf.ResetTransformCurrentObject(false, true, false);
        }
        void arbtn_reset_size_OnClick()
        {
            armf.ResetTransformCurrentObject(false, false, true);
        }
        //---X+
        void tp_right_OnClick()
        { 
            armf.TargetOperationBody("tp_right");
        }
        void tp_right_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_right");
            pushed_tp_right = true;
            evt.StopPropagation();
        }
        void tp_right_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_right = false;
            evt.StopPropagation();
        }
        //---X-
        void tp_left_OnClick()
        {
            armf.TargetOperationBody("tp_left");
        }
        void tp_left_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_left");
            pushed_tp_left = true;
            evt.StopPropagation();
        }
        void tp_left_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_left = false;
            evt.StopPropagation();
        }
        //---Y+
        void tp_up_OnClick()
        {
            armf.TargetOperationBody("tp_up");
        }
        void tp_up_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_up");
            pushed_tp_up = true;
            evt.StopPropagation();
        }
        void tp_up_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_up = false;
            evt.StopPropagation();
        }
        //---Y-
        void tp_down_OnClick()
        {
            armf.TargetOperationBody("tp_down");
        }
        void tp_down_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_down");
            pushed_tp_down = true;
            evt.StopPropagation();
        }
        void tp_down_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_down = false;
            evt.StopPropagation();
        }
        //---Z+
        void tp_forward_OnClick()
        {
            armf.TargetOperationBody("tp_forward");
        }
        void tp_forward_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_forward");
            pushed_tp_forward = true;
            evt.StopPropagation();
        }
        void tp_forward_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_forward = false;
            evt.StopPropagation();
        }
        //---Z-
        void tp_back_OnClick()
        {
            armf.TargetOperationBody("tp_back");
        }
        void tp_back_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_back");
            pushed_tp_back = true;
            evt.StopPropagation();
        }
        void tp_back_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_back = false;
            evt.StopPropagation();
        }
        //#######################################################################################################
        void ChangeStyleBoneBtn(string targetname, bool alldisable = false)
        {
            Color sel = new Color(41, 123, 214, 255);
            Color non = new Color(41, 123, 214, 0);

            //BonePanel.Q<Button>("arbtn_bn_ikparent").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_head").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_lookat").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_eyeviewhandle").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_chest").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_aim").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightshoulder").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftshoulder").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightlowerarm").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftlowerarm").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_pelvis").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftarm").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightarm").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftlowerleg").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightlowerleg").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftleg").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightleg").style.backgroundColor = non;

            RightPanel.Q<Button>("arbtn_mt_camera").style.backgroundColor = non;
            RightPanel.Q<Button>("arbtn_mt_obj").style.backgroundColor = non;


            //---select
            if (alldisable != true)
            {
                BonePanel.Q<Button>("arbtn_bn_" + targetname).style.backgroundColor = sel;
            }
            
        }

        void arbtn_bn_head_OnClick()
        {
            string partsname = "head";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_eyeviewhandle_OnClick()
        {
            string partsname = "eyeviewhandle";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_lookat_OnClick()
        {
            string partsname = "lookat";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_chest_OnClick()
        {
            string partsname = "chest";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_aim_OnClick()
        {
            string partsname = "aim";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_rightshoulder_OnClick()
        {
            string partsname = "rightshoulder";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_leftshoulder_OnClick()
        {
            string partsname = "leftshoulder";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_rightlowerarm_OnClick()
        {
            string partsname = "rightlowerarm";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_leftlowerarm_OnClick()
        {
            string partsname = "leftlowerarm";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_pelvis_OnClick()
        {
            string partsname = "pelvis";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_lefthand_OnClick()
        {
            string partsname = "leftarm";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_righthand_OnClick()
        {
            string partsname = "rightarm";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_leftlowerleg_OnClick()
        {
            string partsname = "leftlowerleg";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_rightlowerleg_OnClick()
        {
            string partsname = "rightlowerleg";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_leftleg_OnClick()
        {
            string partsname = "leftleg";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
        void arbtn_bn_rightleg_OnClick()
        {
            string partsname = "rightleg";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
        }
    }

}
