using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace UserHandleSpace
{
    public class OperateLoadedText : OperateLoadedUImage
    {
        [DllImport("__Internal")]
        private static extern void ChangeTransformOnUpdate(string val);

        [DllImport("__Internal")]
        private static extern void ReceiveObjectVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);




        public GameObject relatedHandleParent;

        private ManageAnimation animarea;

        protected Transform Transform3D;
        protected RectTransform ChildRect;
        protected RectTransform Transform2D;
        protected TextMeshProUGUI text2d;
        protected TextMeshPro text3d;
        protected BoxCollider boxc;
        protected bool isFixMoving;

        protected int jumpNum;
        protected float jumpPower;
        protected AvatarPunchEffect effectPunch;
        protected AvatarShakeEffect effectShake;


        private void Awake()
        {
            targetType = AF_TARGETTYPE.Text;
            bti = new BasicTransformInformation();
            bti.dimension = "2d";
            animarea = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            effectPunch = new AvatarPunchEffect();
            effectShake = new AvatarShakeEffect();
            jumpNum = 0;
            jumpPower = 1f;


        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (bti.dimension == "2d")
            {
                RectTransform rect = GetRectTransform();
                if ((rect.anchoredPosition != oldPosition) || (rect.rotation != oldRotation))
                {

                }

                oldPosition = rect.anchoredPosition;
                oldRotation = rect.rotation;
            }

        }
        public void RegenerateDimension(int dimension)
        {
            if (dimension == 2)
            {
                targetType = AF_TARGETTYPE.Text;
                bti.dimension = "2d";
                text2d = GetComponent<TextMeshProUGUI>();
                Transform2D = GetRectTransform();
                Transform2D.anchoredPosition = oldPosition;
                Transform2D.rotation = oldRotation;
            }
            else if (dimension == 3)
            {
                targetType = AF_TARGETTYPE.Text3D;
                Transform child = transform.GetChild(0);

                bti.dimension = "3d";
                Transform3D = transform;
                boxc = transform.GetComponent<BoxCollider>();

                //---child
                text3d = child.GetComponent<TextMeshPro>();
                ChildRect = child.GetComponent<RectTransform>();
            }
        }
        public TMP_Text TextPro {
            get
            {
                TMP_Text ttext = null;
                if (bti.dimension == "2d")
                {
                    ttext = text2d;
                }
                else if (bti.dimension == "3d")
                {
                    ttext = text3d;
                }
                return ttext;
            }
        }
        

        public override void GetCommonTransformFromOuter()
        {
            string ret = "";
            
            if (bti.dimension == "3d")
            {
                Rigidbody rig = GetComponent<Rigidbody>();
                float drag = rig.drag;

                Vector3 pos = GetPosition3D();
                Vector3 rot = GetRotation3D();
                Vector3 sca = Transform3D.localScale;

                string[] rets =
                {
                    pos.x + "," + pos.y + "," + pos.z,
                    rot.x + "," + rot.y + "," + rot.z,
                    sca.x + "," + sca.y + "," + sca.z,
                    rig.drag.ToString() + "," + rig.angularDrag.ToString() + "," + GetEasyCollision().ToString()  + "," + GetUseGravity().ToString()
                };
                ret = String.Join('%', rets);
            }
            else
            {
                Vector3 pos = GetPosition();
                Vector3 rot = GetRotation();
                Vector3 siz = Transform2D.sizeDelta;
                Vector3 sca = Transform2D.localScale;
                string[] rets =
                {
                    pos.x + "," + pos.y + "," + pos.z,
                    rot.x + "," + rot.y + "," + rot.z,
                    siz.x + "," + siz.y + "," + siz.z,
                    sca.x + "," + sca.y + "," + sca.z
                };
                //ret = pos.x + "," + pos.y + "," + pos.z + "%" + rot.x + "," + rot.y + "," + rot.z + "%" + sca.x + "," + sca.y + "," + sca.z;
                ret = String.Join('%', rets);
            }
            

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        /// <param name="type">1 - normal light, 0 - directional light</param>
        public override void GetIndicatedPropertyFromOuter(int type)
        {
            string ret = "";
            //Text tex = transform.GetComponent<Text>();
            //string apos = GetAnchorPos();

            Vector3 size = GetTextAreaSize();

            Color[] colorGradient = GetColorGradient();
            
            //ret = tex.text + "\t" + apos + "\t" + tex.fontSize.ToString() + "\t" + ((int)tex.fontStyle).ToString() + "\t" + ColorUtility.ToHtmlStringRGBA(tex.color)
            //;
            string[] rets =
            {
                GetVVMText(),
                GetFontSize().ToString(),
                GetFontStyles(),
                ColorUtility.ToHtmlStringRGBA(GetFontColor()),
                size.x.ToString() + "," + size.y.ToString(),
                GetTextAlignment(),
                GetTextOverflow().ToString(),
                bti.dimension,
                GetIsColorGradient() ? "1" : "0",
                ColorUtility.ToHtmlStringRGBA(colorGradient[0]) + "," + ColorUtility.ToHtmlStringRGBA(colorGradient[1]) + "," +
                ColorUtility.ToHtmlStringRGBA(colorGradient[2]) + "," + ColorUtility.ToHtmlStringRGBA(colorGradient[3]),
                GetFontOutlineWidth().ToString(),
                ColorUtility.ToHtmlStringRGBA(GetFontOutlineColor())
            };
            ret = String.Join('\t', rets);

            Debug.Log(ret);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        //---Transform for manual operation--------------------------------------------=============
        public void SetDrag(float dval, float adval)
        {
            Rigidbody rig = GetComponent<Rigidbody>();
            rig.drag = dval;
            rig.angularDrag = adval;
        }
        public void SetDragFromOuter(string param)
        {
            string[] prms = param.Split(",");
            float dval = float.TryParse(prms[0], out dval) ? dval : 10f;
            float adval = float.TryParse(prms[1], out adval) ? adval : 10f;

            SetDrag(dval, adval);

        }
        public float GetDrag(int flag)
        {
            Rigidbody rig = GetComponent<Rigidbody>();
            if (flag == 1)
            {
                return rig.drag;
            }
            else if (flag == 2)
            {
                return rig.angularDrag;
            }
            return 0;
        }
        public void SetEasyCollision(int flag)
        {
            BoxCollider boxc = GetComponent<BoxCollider>();
            if (boxc != null)
            {
                boxc.isTrigger = flag == 1 ? false : true;
            }
        }
        public int GetEasyCollision()
        {
            int ret = 0;

            //---Text3D collider is itself
            BoxCollider boxc = GetComponent<BoxCollider>();
            if (boxc != null)
            {
                ret = boxc.isTrigger == true ? 0 : 1;
            }
            return ret;
        }
        public void SetUseGravity(int flag)
        {
            Rigidbody rig = GetComponent<Rigidbody>();
            rig.useGravity = flag == 1 ? true : false;
        }
        public int GetUseGravity()
        {
            int ret = 0;
            Rigidbody rig = GetComponent<Rigidbody>();
            ret = rig.useGravity ? 1 : 0;
            return ret;
        }

        //===-normal transform-----------------------------------------------------------------####
        public Vector3 GetPosition3D()
        {
            if (relatedHandleParent != null)
            {
                return relatedHandleParent.transform.position;
            }
            else
            {
                return gameObject.transform.position;
            }
        }
        public Vector3 GetPosition3DFromOuter()
        {
            Vector3 ret;
            if (relatedHandleParent != null)
            {
                ret = relatedHandleParent.transform.position;
            }
            else
            {
                ret = gameObject.transform.position;
            }

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;
        }
        public void SetPosition(Vector3 pos)
        {
            if (relatedHandleParent != null)
            {
                relatedHandleParent.transform.position = (pos);
            }
            else
            {
                gameObject.transform.position = (pos);
            }

        }
        public override void SetPositionFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;
            bool isabs = prm[3] == "1" ? true : false;

            if (bti.dimension == "3d")
            {
                SetPosition(new Vector3(x, y, z));
            }
            else
            {
                base.SetPositionFromOuter(param);
            }

            
            /*if (relatedHandleParent != null)
            {
                relatedHandleParent.transform.DOMove(new Vector3(x, y, z), 0.1f).SetRelative(!isabs);
            }
            else
            {
                gameObject.transform.DOMove(new Vector3(x, y, z), 0.1f).SetRelative(!isabs);
            }*/


        }
        public Vector3 GetRotation3D()
        {
            Vector3 ret;

            if (relatedHandleParent != null)
            {
                ret = relatedHandleParent.transform.rotation.eulerAngles;
            }
            else
            {
                ret = gameObject.transform.rotation.eulerAngles;
            }

            return ret;
        }
        public override Vector3 GetRotationFromOuter()
        {
            Vector3 ret;
            if (bti.dimension == "3d")
            {
                if (relatedHandleParent != null)
                {
                    ret = relatedHandleParent.transform.rotation.eulerAngles;
                }
                else
                {
                    ret = gameObject.transform.rotation.eulerAngles;
                }
            }
            else
            {
                ret = Transform2D.rotation.eulerAngles;
            }
            

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;

        }
        public void SetRotation(Vector3 rot)
        {
            if (relatedHandleParent != null)
            {
                relatedHandleParent.transform.rotation = Quaternion.Euler(rot);
            }
            else
            {
                gameObject.transform.rotation = Quaternion.Euler(rot);
            }

        }
        public override void SetRotationFromOuter(string param)
        {

            if (bti.dimension == "3d")
            {
                string[] prm = param.Split(',');
                float x = float.TryParse(prm[0], out x) ? x : 0f;
                float y = float.TryParse(prm[1], out y) ? y : 0f;
                float z = float.TryParse(prm[2], out z) ? z : 0f;
                bool isabs = prm[3] == "1" ? true : false;
                if (relatedHandleParent != null)
                {
                    relatedHandleParent.transform.rotation = Quaternion.Euler(new Vector3(x, y, z));
                }
                else
                {
                    gameObject.transform.rotation = Quaternion.Euler(new Vector3(x, y, z));
                }
            }
            else
            {
                base.SetRotationFromOuter(param);
            }


        }

        /// <summary>
        /// Set scale of 3D-text parent object
        /// </summary>
        /// <param name="param"></param>
        public virtual void SetScale(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;

            //---Different to OperateActiveVRM.SetScale:
            // OperateActiveVRM: base point is child object with collider. refer to transform.parent.
            // At here: transform.gameObject IS Other Object own.

            transform.DOScale(new Vector3(x, y, z), 0.1f);
            //relatedHandleParent.transform.DOScale(new Vector3(x, y, z), 0.1f);

        }
        public Vector3 GetScale()
        {
            Vector3 ret = Vector3.zero;

            ret = transform.localScale;

            return ret;
        }
        public new void GetScaleFromOuter()
        {
            Vector3 ret = Vector3.zero;

            ret = transform.localScale;

            string jstr = JsonUtility.ToJson(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(jstr);
#endif
        }
        //---fix flag----------------------------------############
        public int GetFixMoving()
        {
            return isFixMoving ? 1 : 0;
        }
        public int GetFixMovingFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(isFixMoving ? 1 : 0);
#endif
            return isFixMoving ? 1 : 0;
        }
        public virtual void SetFixMoving(bool flag)
        {
            /* Available 3D Object
             * VRM - avatar self: CapsuleCollider                                     IK : BoxCollider, SphereCollider
             * Oth - avatar self: none     avatar child(with Mesh) : BoxCollider      IK : BoxCollider
             * IMG - avatar self: none     avatar child(with Mesh) : BoxCollider      IK : BoxCollider
             * CAM - avatar self: none
             * 
             */
            isFixMoving = flag;

            transform.gameObject.GetComponent<BoxCollider>().enabled = !isFixMoving;
            if (relatedHandleParent != null) relatedHandleParent.SetActive(!isFixMoving);
                    
        }
        public void SetFixMovingFromOuter(string param)
        {
            SetFixMoving(param == "1" ? true : false);
        }

        //---effect: punch------------------------------------
        public virtual void SetPunch(AvatarPunchEffect param)
        {
            if (param != null)
            {
                effectPunch.elasiticity = param.elasiticity;
                effectPunch.isEnable = param.isEnable;
                effectPunch.punch.x = param.punch.x;
                effectPunch.punch.y = param.punch.y;
                effectPunch.punch.z = param.punch.z;
                effectPunch.translationType = param.translationType;
                effectPunch.vibrato = param.vibrato;
            }
        }
        public virtual void SetPunchFromOuter(string param)
        {
            AvatarPunchEffect ape = JsonUtility.FromJson<AvatarPunchEffect>(param);
            if (ape != null)
            {
                effectPunch = ape;
            }
        }
        public AvatarPunchEffect GetPunch()
        {
            return effectPunch;
        }
        public void GetPunchFromOuter()
        {
            string js = JsonUtility.ToJson(effectPunch);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        //---effect: shake------------------------------------
        public virtual void SetShake(AvatarShakeEffect param)
        {
            if (param != null)
            {
                effectShake.fadeOut = param.fadeOut;
                effectShake.isEnable = param.isEnable;
                effectShake.randomness = param.randomness;
                effectShake.strength = param.strength;
                effectShake.translationType = param.translationType;
                effectShake.vibrato = param.vibrato;
            }
        }
        public virtual void SetShakeFromOuter(string param)
        {
            AvatarShakeEffect ashe = JsonUtility.FromJson<AvatarShakeEffect>(param);
            if (ashe != null)
            {
                effectShake = ashe;
            }
        }
        public AvatarShakeEffect GetShake()
        {
            return effectShake;
        }
        public void GetShakeFromOuter()
        {
            string js = JsonUtility.ToJson(effectShake);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        //---effect: shake------------------------------------
        public void SetJump(string param)
        {
            string[] prm = param.Split(',');
            float power = float.TryParse(prm[0], out power) ? power : 0f;
            int num = int.TryParse(prm[1], out num) ? num : 0;

            jumpNum = num;
            jumpPower = power;
        }
        public float GetJumpPower()
        {
            return jumpPower;
        }
        public int GetJumpNum()
        {
            return jumpNum;
        }
        public void GetJumpPowerFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(jumpPower);
#endif
        }
        public void GetJumpNumFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(jumpNum);
#endif
        }

        //---method for Text UI----------------------------------------=======================
        public int GetFontSize(int withhtml = -1)
        {
            //RectTransform rect = GetRectTransform();
            //Text tex = transform.GetComponent<Text>();

            int ret = 0;
            if (bti.dimension == "2d")
            {
                ret = Mathf.RoundToInt(text2d.fontSize);
            }
            else if (bti.dimension == "3d")
            {
                ret = Mathf.RoundToInt(text3d.fontSize);
            }

#if !UNITY_EDITOR && UNITY_WEBGL
            if (withhtml > -1) ReceiveIntVal(ret);
#endif
            return ret;

        }
        public void SetFontSize(int size)
        {
            //Text tex = transform.GetComponent<Text>();
            //tex.fontSize = size;
            if (bti.dimension == "2d")
            {
                text2d.fontSize = size;
            }
            else if (bti.dimension == "3d")
            {
                text3d.fontSize = size;
            }
            
            
        }

        /// <summary>
        /// Get Text style
        /// </summary>
        /// <returns></returns>
        public int GetFontStyle()
        {
            //Text tex = transform.GetComponent<Text>();

            int ret = 0;

            if (bti.dimension == "2d")
            {
                ret = (int)text2d.fontStyle;
            }
            

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
            return ret;
        }
        public void SetFontStyle(int style)
        {
            //Text tex = transform.GetComponent<Text>();
            //tex.fontStyle = (FontStyle)style;

        }

        /// <summary>
        /// Get TextmeshPro style
        /// </summary>
        /// <returns></returns>
        public string GetFontStyles(int withhtml = -1)
        {
            //TMPro.FontStyles ret = FontStyles.Normal;
            string[] ret = { "-", "-", "-", "-", "-", "-"};

            if ((TextPro.fontStyle & FontStyles.Bold) > 0) ret[0] = "b";
            if ((TextPro.fontStyle & FontStyles.Italic) > 0) ret[1] = "i";
            if ((TextPro.fontStyle & FontStyles.Underline) > 0) ret[2] = "u";
            if ((TextPro.fontStyle & FontStyles.Strikethrough) > 0) ret[3] = "s";
            if ((TextPro.fontStyle & FontStyles.LowerCase) > 0) ret[4] = "L";
            if ((TextPro.fontStyle & FontStyles.UpperCase) > 0) ret[5] = "U";
            /*
            if (bti.dimension == "2d")
            {
                if ((text2d.fontStyle & FontStyles.Bold) > 0) ret[0] = "b";
                if ((text2d.fontStyle & FontStyles.Italic) > 0) ret[1] ="i";
                if ((text2d.fontStyle & FontStyles.Underline) > 0) ret[2] = "u";
                if ((text2d.fontStyle & FontStyles.LowerCase) > 0) ret[3] = "L";
                if ((text2d.fontStyle & FontStyles.UpperCase) > 0) ret[4] = "U";

                //ret = text2d.fontStyle;
            }
            else if (bti.dimension == "3d")
            {
                if ((text3d.fontStyle & FontStyles.Bold) > 0) ret[0] = "b";
                if ((text3d.fontStyle & FontStyles.Italic) > 0) ret[1] = "i";
                if ((text3d.fontStyle & FontStyles.Underline) > 0) ret[2] = "u";
                if ((text3d.fontStyle & FontStyles.LowerCase) > 0) ret[3] = "L";
                if ((text3d.fontStyle & FontStyles.UpperCase) > 0) ret[4] = "U";

                //ret = text3d.fontStyle;
            }*/

            string strret = String.Join(',', ret).Replace(",", "");
#if !UNITY_EDITOR && UNITY_WEBGL
            
            //---convert to style string
            //if ((ret & FontStyles.Bold) > 0) strret += "b";
            //if ((ret & FontStyles.Italic) > 0) strret += "i";
            //if ((ret & FontStyles.Underline) > 0) strret += "u";
            //if ((ret & FontStyles.LowerCase) > 0) strret += "L";
            //if ((ret & FontStyles.UpperCase) > 0) strret += "U";
            if (withhtml > -1) ReceiveStringVal(strret);
#endif
            return strret;
        }
        public void SetFontStyles(string param)
        {

            /*
            string[] prms = param.Split(",");
            string addstyle = prms[0];
            string delstyle = prms[1];
            */

            if (param[0] == 'b') TextPro.fontStyle |= FontStyles.Bold;
            if (param[1] == 'i') TextPro.fontStyle |= FontStyles.Italic;
            if (param[2] == 'u') TextPro.fontStyle |= FontStyles.Underline;
            if (param[3] == 's') TextPro.fontStyle |= FontStyles.Strikethrough;
            if (param[4] == 'L') TextPro.fontStyle |= FontStyles.LowerCase;
            if (param[5] == 'U') TextPro.fontStyle |= FontStyles.UpperCase;

            if (param[0] == '-') TextPro.fontStyle &= ~FontStyles.Bold;
            if (param[1] == '-') TextPro.fontStyle &= ~FontStyles.Italic;
            if (param[2] == '-') TextPro.fontStyle &= ~FontStyles.Underline;
            if (param[3] == '-') TextPro.fontStyle &= ~FontStyles.Strikethrough;
            if (param[4] == '-') TextPro.fontStyle &= ~FontStyles.LowerCase;
            if (param[5] == '-') TextPro.fontStyle &= ~FontStyles.UpperCase;

        }

        public string GetVVMText(int withhtml = -1)
        {
            //Text tex = transform.GetComponent<Text>();
            string ret = "";
            if (bti.dimension == "2d")
            {
                ret = text2d.text;
            }
            else if (bti.dimension == "3d")
            {
                ret = text3d.text;
            }

#if !UNITY_EDITOR && UNITY_WEBGL
            if (withhtml > -1) ReceiveStringVal(ret);
#endif
            return ret;
        }
        public void SetVVMText(string text)
        {
            //Text tex = transform.GetComponent<Text>();
            //tex.text = text;
            if (bti.dimension == "2d")
            {
                text2d.text = text;
            }
            else if (bti.dimension == "3d")
            {
                text3d.text = text;
            }
        }

        public Color GetFontColor(int withhtml = -1)
        {
            //Text tex = transform.GetComponent<Text>();
            Color ret = Color.white;

            ret = TextPro.color;

            /*if (bti.dimension == "2d")
            {
                ret = text2d.color;
            }
            else if (bti.dimension == "3d")
            {
                ret = text3d.color;
            }*/

#if !UNITY_EDITOR && UNITY_WEBGL
            if (withhtml > -1) ReceiveStringVal(ColorUtility.ToHtmlStringRGBA(ret));
#endif
            return ret;
        }
        public void SetFontColor(Color col)
        {
            /*if (bti.dimension == "2d")
            {
                text2d.color = col;
            }
            else if (bti.dimension == "3d")
            {
                text3d.color = col;
            }*/
            //TextPro.enableVertexGradient = false;
            TextPro.color = col;
        }
        public void SetFontColorFromOuter(string param)
        {
            //Text tex = transform.GetComponent<Text>();
            Color col = ColorUtility.TryParseHtmlString(param, out col) ? col : Color.black;

            SetFontColor(col);
        }
        public bool GetIsColorGradient()
        {
            return TextPro.enableVertexGradient;
        }
        public void SetEnableColorGradient(bool flag)
        {
            TextPro.enableVertexGradient = flag;
            if (flag ==  true)
            {
                TextPro.color = Color.white;
            }
        }
        public void SetEnableColorGradientFromOuter(string param)
        {
            SetEnableColorGradient(param == "1" ? true : false);
        }
        /// <summary>
        /// Get color gradient
        /// </summary>
        /// <returns>0 - topleft, 1 - topright, 2 - bottomleft, 3 - bottomright</returns>
        public Color[] GetColorGradient()
        {
            Color[] cols = new Color[4];

            TMP_Text ttext = TextPro;
            
            cols[0] = ttext.colorGradient.topLeft;
            cols[1] = ttext.colorGradient.topRight;
            cols[2] = ttext.colorGradient.bottomLeft;
            cols[3] = ttext.colorGradient.bottomRight;

            return cols;
        }
        public void SetColorGradient(string dir, Color col)
        {
            VertexGradient colorGradient = TextPro.colorGradient;
            //TMP_Text ttext = TextPro;
            
            switch (dir)
            {
                case "tl":
                    colorGradient.topLeft = col;
                    break;
                case "tr":
                    colorGradient.topRight = col;
                    break;
                case "bl":
                    colorGradient.bottomLeft = col;
                    break;
                case "br":
                    colorGradient.bottomRight = col;
                    break;
            }
            //TextPro.enableVertexGradient = true;
            TextPro.colorGradient = colorGradient;
        }

        /// <summary>
        /// Set color gradient of text 2d/3d 
        /// </summary>
        /// <param name="param">[0] direction(tl, tr, bl, br), [1] - Color HTML String(#FFFFFF)</param>
        public void SetColorGradientFromOuter(string param)
        {
            string[] prms = param.Split(",");

            Color col = Color.white;
            if (prms.Length > 1)
            {
                col = ColorUtility.TryParseHtmlString(prms[1], out col) ? col : Color.black;
            }
            SetColorGradient(prms[0], col);
        }

        /// <summary>
        /// Get outline width
        /// </summary>
        /// <returns></returns>
        public float GetFontOutlineWidth()
        {
            return TextPro.outlineWidth;
        }
        public void SetFontOutlineWidth(float width)
        {
            
            TextPro.outlineWidth = width;
        }
        public void SetFontOutlineWidthFromOuter(string param)
        {
            float val = float.TryParse(param, out val) ? val : 0;
            SetFontOutlineWidth(val);
        }

        /// <summary>
        /// Get outline color
        /// </summary>
        /// <returns></returns>
        public Color GetFontOutlineColor()
        {
            return TextPro.outlineColor;
        }
        public void SetFontOutlineColor(Color col)
        {
            TextPro.outlineColor = col;
        }
        public void SetFontOutlineColorFromOuter(string param)
        {
            Color col = ColorUtility.TryParseHtmlString(param, out col) ? col : Color.black;
            SetFontOutlineColor(col);            
        }

        /// <summary>
        /// Get text overflow type
        /// </summary>
        /// <param name="withhtml"></param>
        /// <returns></returns>
        public int GetTextOverflow(int withhtml = -1)
        {
            int ret = 0;
            if (bti.dimension == "2d")
            {
                ret = (int)text2d.overflowMode;
            }
            else if (bti.dimension == "3d")
            {
                ret = (int)text3d.overflowMode;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            if (withhtml > -1) ReceiveIntVal(ret);
#endif
            return ret;
        }
        public void SetTextOverflow(int overflow)
        {
            if (bti.dimension == "2d")
            {
                text2d.overflowMode = (TMPro.TextOverflowModes)overflow;
            }
            else if (bti.dimension == "3d")
            {
                text3d.overflowMode = (TMPro.TextOverflowModes)overflow;
            }
        }
        public void SetTextAlignment(string anchorpos)
        {
            TMP_Text text = null;
            if (bti.dimension == "2d")
            {
                text = text2d;
            }
            else if (bti.dimension == "3d")
            {
                text = text3d;
            }

            if (anchorpos == "tl")
            {
                text.alignment = TextAlignmentOptions.TopLeft;
            }
            else if (anchorpos == "ml")
            {
                text.alignment = TextAlignmentOptions.MidlineLeft;
            }
            else if (anchorpos == "bl")
            {
                text.alignment = TextAlignmentOptions.BottomLeft;
            }
            else if (anchorpos == "tm")
            {
                text.alignment = TextAlignmentOptions.Top;
            }
            else if (anchorpos == "mm")
            {
                text.alignment = TextAlignmentOptions.MidlineLeft;
            }
            else if (anchorpos == "bm")
            {
                text.alignment = TextAlignmentOptions.Bottom;
            }
            else if (anchorpos == "tr")
            {
                text.alignment = TextAlignmentOptions.TopRight;
            }
            else if (anchorpos == "mr")
            {
                text.alignment = TextAlignmentOptions.MidlineRight;
            }
            else if (anchorpos == "br")
            {
                text.alignment = TextAlignmentOptions.BottomRight;
            }
        }
        public string GetTextAlignment()
        {
            string ret = "";

            TMP_Text text = null;
            if (bti.dimension == "2d")
            {
                text = text2d;
            }
            else if (bti.dimension == "3d")
            {
                text = text3d;
            }

            if (text.alignment == TextAlignmentOptions.TopLeft) ret = "tl";
            if (text.alignment == TextAlignmentOptions.MidlineLeft) ret = "ml";
            if (text.alignment == TextAlignmentOptions.BottomLeft) ret = "bl";

            if (text.alignment == TextAlignmentOptions.Top) ret = "tm";
            if (text.alignment == TextAlignmentOptions.Midline) ret = "mm";
            if (text.alignment == TextAlignmentOptions.Bottom) ret = "bm";

            if (text.alignment == TextAlignmentOptions.TopRight) ret = "tr";
            if (text.alignment == TextAlignmentOptions.MidlineRight) ret = "mr";
            if (text.alignment == TextAlignmentOptions.BottomRight) ret = "br";

            return ret;
        }

        public Vector3 GetTextAreaSize()
        {

            if (bti.dimension == "3d")
            {
                return new Vector3(ChildRect.sizeDelta.x, ChildRect.sizeDelta.y, 1);
            }
            else
            {
                return new Vector3(Transform2D.sizeDelta.x, Transform2D.sizeDelta.y, 1);
            }
        }
        /// <summary>
        /// Set size (RectTransform and BoxCollider)
        /// </summary>
        /// <param name="size"></param>
        public void SetTextAreaSize(Vector3 size)
        {
            TMP_Text text = null;
            if (bti.dimension == "2d")
            {
                text = text2d;
                //---set TextMeshPro sizeDelta
                Transform2D.sizeDelta = new Vector2(size.x, size.y);
            }
            else if (bti.dimension == "3d")
            {
                text = text3d;
                //---set parent BoxCollider size (2, 0.5,...)
                boxc.size = new Vector3(size.x * 0.1f, size.y * 0.1f, boxc.size.z);
                //---set TextMeshPro sizeDelta (20, 5,...)
                ChildRect.sizeDelta = new Vector2(size.x, size.y);
            }
        }
        /// <summary>
        /// Set RectTransform painted area size of 3D-text child object (sizeDelta level)
        /// </summary>
        /// <param name="param"></param>
        public void SetTextAreaSizeFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 1f;
            float y = float.TryParse(prm[1], out y) ? y : 1f;
            //float z = float.TryParse(prm[2], out z) ? z : 0f;

            SetTextAreaSize(new Vector3(x, y, 1));

        }
        public void SetFontAsset(string param)
        {
            TMP_FontAsset.CreateInstance(param);
        }
        //=== For an animation tween====================================================================##########
        public void TextAnimationTween(Sequence seq, AnimationTargetParts movedata, AnimationParsingOptions options, float duration)
        {
            if (options.isExecuteForDOTween != 1) return;

            TMP_Text text = null;
            //RectTransform rect = GetRectTransform();
            //BoxCollider bcol = GetComponent<BoxCollider>();
            if (bti.dimension == "2d")
            {
                text = text2d;
            }
            else if (bti.dimension == "3d")
            {
                text = text3d;
            }

            if (movedata.animationType == AF_MOVETYPE.Scale)
            {
                if (bti.dimension == "3d")
                {
                    seq.Join(ChildRect.DOSizeDelta(movedata.scale, duration));
                    seq.Join(DOVirtual.DelayedCall(duration, () =>
                    {
                        boxc.size = new Vector3(movedata.scale.x * 0.1f, movedata.scale.y * 0.1f, movedata.scale.z);
                    }, false));
                }
                else
                {
                    seq.Join(Transform2D.DOSizeDelta(movedata.scale, duration));
                }
                
                
            }
            else if (movedata.animationType == AF_MOVETYPE.Text)
            {
                seq.Join(DOVirtual.DelayedCall(duration, () =>
                {
                    SetVVMText(movedata.text);
                }, false));

            }
            else if (movedata.animationType == AF_MOVETYPE.TextProperty)
            {
                seq.Join(DOTween.To(() => text.fontSize, x => text.fontSize = x, movedata.fontSize, duration));

                if (!movedata.IsGradient)
                {
                    seq.Join(text.DOColor(movedata.color, duration));
                }
                
                seq.Join(DOTween.To(() => text.outlineWidth, x => text.outlineWidth = x, movedata.fontOutlineWidth, duration));
                seq.Join(DOVirtual.DelayedCall(duration, () =>
                {
                    SetFontStyles(movedata.fontStyles);
                    SetTextAlignment(movedata.textAlignmentOptions);
                    SetTextOverflow(movedata.textOverflow);

                    if (movedata.IsGradient)
                    {
                        SetFontColor(Color.white);
                        string[] gradientDirs = { "tl", "tr", "bl", "br" };
                        for (int i = 0; i < movedata.gradients.Length; i++)
                        {
                            SetColorGradient(gradientDirs[i], movedata.gradients[i]);
                        }
                    }
                    
                    
                    SetFontOutlineColor(movedata.fontOutlineColor);
                }, false));



            }
        }
    }

}
