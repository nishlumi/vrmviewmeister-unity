using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UserHandleSpace;
using DG.Tweening;

/// <summary>
/// Attach for IK Object of Other object(FBX, OBJ, etc...)
/// </summary>
public class OtherObjectDummyIK : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ChangeTransformOnUpdate(string val);
    [DllImport("__Internal")]
    private static extern void ChangeDirectionalLightTransformOnUpdate(string val);


    [SerializeField]
    protected bool IKUsePosition;
    [SerializeField]
    protected bool IKUseRotation;

    public GameObject relatedAvatar;
    public AF_TARGETTYPE relatedType;

    public GameObject equippedAvatar;
    public bool isEquipping;

    private Vector3 oldikposition;
    private Quaternion oldikrotation;
    protected Vector3 defaultPosition;
    protected Quaternion defaultRotation;


    private BasicTransformInformation bti;
    private ManageAnimation animarea;

    private OperateActiveVRM oavrm;
    private AvatarKeyOperator akeyo;

    private bool HasRectTransform;
    RectTransform rectTransform;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        SaveDefaultTransform(true, true);

        isEquipping = false;
        bti = new BasicTransformInformation();
        bti.dimension = "3d";
        animarea = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        oavrm = ikhp.GetComponent<OperateActiveVRM>();
        akeyo = new AvatarKeyOperator(animarea.cfg_keymove_speed_rot, animarea.cfg_keymove_speed_trans);

        HasRectTransform = false;
        rectTransform = null;
    }

    // Update is called once per frame
    void Update()
    {
        /*if (
            (!animarea.IsPlaying) &&
            (animarea.IsVRAR()) && 
            (gameObject.name != "DLightIK")
        )
        {
            // If VR/AR mode, IK-marker for OtherObject/Image don't move.
            return;
        }*/
        if (relatedAvatar != null)
        {
            if ((transform.position != oldikposition) || (transform.rotation != oldikrotation))
            {
                if (IKUsePosition)
                {
                    if (HasRectTransform)
                    {
                        rectTransform.position = transform.position;
                    }
                    else
                    {
                        relatedAvatar.transform.position = transform.position;
                    }
                    
                }
                if (IKUseRotation)
                {
                    if (HasRectTransform)
                    {
                        rectTransform.rotation = transform.rotation;
                    }
                    else
                    {
                        relatedAvatar.transform.rotation = transform.rotation;
                    }
                    
                }

                oldikposition = transform.position;
                oldikrotation = transform.rotation;

                
                bti.id = relatedAvatar.name;
                bti.position = transform.position;
                bti.rotation = transform.rotation.eulerAngles;
                bti.scale = relatedAvatar.transform.localScale;

#if !UNITY_EDITOR && UNITY_WEBGL
                if (!animarea.IsPlaying) {
                    if (relatedAvatar.name == "Directional Light")
                    {
                        ChangeDirectionalLightTransformOnUpdate(JsonUtility.ToJson(bti));
                    }
                    else
                    {
                        //Debug.Log("ChangeTransformOnUpdate");
                        ChangeTransformOnUpdate(JsonUtility.ToJson(bti));
                    }
                }
#endif
            }
            //---key operation for current selected avatar translation
            /*
            if (animarea.keyOperationMode == KeyOperationMode.MoveAvatar) 
            { //this avatar is active ?
                if (oavrm.ActiveAvatar.GetInstanceID() == relatedAvatar.GetInstanceID())
                {
                    akeyo.SetSpeed(animarea.cfg_keymove_speed_rot, animarea.cfg_keymove_speed_trans);
                    akeyo.CallKeyOperation(gameObject);
                }
            }
            */
        }
    }
    private void OnDestroy()
    {
        
    }
    public void SaveDefaultTransform(bool ispos, bool isrotate)
    {
        if (ispos) defaultPosition = transform.position;
        if (isrotate) defaultRotation = transform.rotation;
    }
    public void LoadDefaultTransform(bool ispos, bool isrotate)
    {
        if (ispos) transform.position = defaultPosition;
        if (isrotate) transform.rotation = defaultRotation;
    }
    public void SetIKFlag(bool use_move, bool use_rotate)
    {
        IKUsePosition = use_move;
        IKUseRotation = use_rotate;
    }
    public void SetRelateAvatar(GameObject avatar, bool IsRectTransform = false)
    {
        relatedAvatar = avatar;
        if (IsRectTransform)
        {
            HasRectTransform = true;
            rectTransform = avatar.GetComponent<RectTransform>();
        }
    }
}
