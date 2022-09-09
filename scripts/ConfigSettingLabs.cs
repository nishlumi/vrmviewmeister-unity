using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;



public class ConfigSettingLabs : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ReceiveStringVal(string val);
    [DllImport("__Internal")]
    private static extern void ReceiveIntVal(int val);
    [DllImport("__Internal")]
    private static extern void ReceiveFloatVal(float val);


    //public Dictionary<string, bool> boolDataList;

    /*
     * is_move_with_vrmbody_with_ik
     * is_change_shader_mtoon_when_otherobject
     * use_animation_generic_when_otherobject
     */

    private string[] intKeys = { "is_move_with_vrmbody_with_ik", "is_change_shader_mtoon_when_otherobject", "use_animation_generic_when_otherobject",
        "enable_activateavatar_from_unity", "use_fullbody_bipedik","enable_foot_autorotate","focus_camera_onselect","recover_tomax_overframe"
    };
    public Dictionary<string, int>  intDataList;
    private string[] floatKeys = { "ikbone_adjust_leg_x", "ikbone_adjust_leg_y", "ikbone_adjust_leg_z", "distance_camera_viewpoint","camera_keymove_speed" };
    public Dictionary<string, float> floatDataList;
    private string[] strKeys = { };
    public Dictionary<string, string> strDataList;

    public bool IsMoveWithVRMbodyWithIK = true;
    public bool IsChangeShaderMToonForOtherobject = false;
    public bool UseAnimationGeneriForOtherobject = false;
    public bool enable_foot_autorotate = false;

    private void Awake()
    {
        //boolDataList = new Dictionary<string, bool>();
        intDataList = new Dictionary<string, int>();
        floatDataList = new Dictionary<string, float>();
        strDataList = new Dictionary<string, string>();
        
    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < intKeys.Length; i++)
        {
            if (PlayerPrefs.HasKey(intKeys[i])) SetIntVal(intKeys[i], PlayerPrefs.GetInt(intKeys[i], 0));
        }
        for (int i = 0; i < floatKeys.Length; i++)
        {
            if (PlayerPrefs.HasKey(floatKeys[i])) SetFloatVal(floatKeys[i], PlayerPrefs.GetFloat(floatKeys[i], 0f));
        }
        for (int i = 0; i < strKeys.Length; i++)
        {
            if (PlayerPrefs.HasKey(strKeys[i])) SetStrVal(strKeys[i], PlayerPrefs.GetString(strKeys[i], ""));
        }

        //---initialize key & value
        if (!PlayerPrefs.HasKey("is_move_with_vrmbody_with_ik")) SetIntVal("is_move_with_vrmbody_with_ik",1);
        if (!PlayerPrefs.HasKey("is_change_shader_mtoon_when_otherobject")) SetIntVal("is_change_shader_mtoon_when_otherobject",1);
        if (!PlayerPrefs.HasKey("use_animation_generic_when_otherobject")) SetIntVal("use_animation_generic_when_otherobject",0);
        if (!PlayerPrefs.HasKey("use_fullbody_bipedik")) SetIntVal("use_fullbody_bipedik", 0);

        if (!PlayerPrefs.HasKey("ikbone_adjust_leg_x")) SetFloatVal("ikbone_adjust_leg_x",1f);
        if (!PlayerPrefs.HasKey("ikbone_adjust_leg_y")) SetFloatVal("ikbone_adjust_leg_y",1f);
        if (!PlayerPrefs.HasKey("ikbone_adjust_leg_z")) SetFloatVal("ikbone_adjust_leg_z",1f);
        if (!PlayerPrefs.HasKey("distance_camera_viewpoint")) SetFloatVal("distance_camera_viewpoint", 2.5f);

        if (!PlayerPrefs.HasKey("enable_activateavatar_from_unity")) SetIntVal("enable_activateavatar_from_unity",1);

        if (!PlayerPrefs.HasKey("enable_foot_autorotate")) SetIntVal("enable_foot_autorotate", 0);
        if (!PlayerPrefs.HasKey("focus_camera_onselect")) SetIntVal("focus_camera_onselect", 0);

        if (!PlayerPrefs.HasKey("camera_keymove_speed")) SetFloatVal("camera_keymove_speed", 0.01f);
        if (!PlayerPrefs.HasKey("camera_keyrotate_speed")) SetFloatVal("camera_keyrotate_speed", 0.1f);

        if (!PlayerPrefs.HasKey("recover_tomax_overframe")) SetIntVal("recover_tomax_overframe", 1);

    }
    private void OnDestroy()
    {
        Dictionary<string, int>.Enumerator enumint = intDataList.GetEnumerator();
        while (enumint.MoveNext())
        {
            PlayerPrefs.SetInt(enumint.Current.Key, enumint.Current.Value);
        }

        Dictionary<string, float>.Enumerator enumfloat = floatDataList.GetEnumerator();
        while (enumfloat.MoveNext())
        {
            PlayerPrefs.SetFloat(enumfloat.Current.Key, enumfloat.Current.Value);
        }

        Dictionary<string, string>.Enumerator enumstr = strDataList.GetEnumerator();
        while (enumstr.MoveNext())
        {
            PlayerPrefs.SetString(enumstr.Current.Key, enumstr.Current.Value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Convert Dictionary key-value to fixed variable. (for using variable in Update(), etc... )
    /// </summary>
    /// <param name="name"></param>
    /// <param name="val"></param>
    void ContinuableVariable(string name, string val)
    {
        if (name == "enable_foot_autorotate")
        {
            int v = int.TryParse(val, out v) ? v : 0;
            if (v == 1)
            {
                enable_foot_autorotate = true;
            }
            else
            {
                enable_foot_autorotate = false;
            }
        }
    }
    /*
    public void SetBoolVal(string key, bool value)
    {
        PlayerPrefs.SetInt(key + "_bool", value == true ? 1 : 0);
    }
    public bool GetBoolVal(string key)
    {
        return PlayerPrefs.GetInt(key + "_bool") == 1 ? true : false;
    }
    */

    //---int---------------------------------------------------------------
    public void SetIntVal(string key, int value)
    {
        intDataList[key] = value;
        //PlayerPrefs.SetInt(key, value);

        ContinuableVariable(key, value.ToString());
    }
    public int GetIntVal(string key, int defaultval = 0)
    {
        return intDataList.ContainsKey(key) ? intDataList[key] : defaultval;
        //return PlayerPrefs.GetInt(key,0);
    }
    public void DelIntVal(string key)
    {
        intDataList.Remove(key);
    }
    //---float------------------------------------------------------------
    public void SetFloatVal(string key, float value)
    {
        floatDataList[key] = value;

        ContinuableVariable(key, value.ToString());
    }
    public float GetFloatVal(string key, float defaultval = 0f)
    {
        return floatDataList.ContainsKey(key) ? floatDataList[key] : defaultval;
    }
    public void DelFloatVal(string key)
    {
        floatDataList.Remove(key);
    }
    //---string----------------------------------------------------------
    public void SetStrVal(string key, string value)
    {
        strDataList[key] = value;

        ContinuableVariable(key, value);
    }
    public string GetStrVal(string key, string defaultval = "")
    {
        return strDataList.ContainsKey(key) ? strDataList[key] : defaultval;
    }
    public void DelStrVal(string key)
    {
        strDataList.Remove(key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="param">0=int,float,bool,str, 1=name, 2=value</param>
    public void SetValFromOuter(string param)
    {
        string[] prm = param.Split(',');
        string name = prm[1];

        if (prm[0] == "int")
        {
            int v = int.TryParse(prm[2], out v) ? v : 0;
            SetIntVal(name, v);
        }
        else if (prm[0] == "float")
        {
            float v = float.TryParse(prm[2], out v) ? v : 0f;
            SetFloatVal(name, v);
        }
        /*else if (prm[0] == "bool")
        {
            int v = int.TryParse(prm[2], out v) ? v : 0;
            SetIntVal(name, v == 1 ? true : false);
        }*/
        else if (prm[0] == "str")
        {
            SetStrVal(name, prm[2]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="param">0=int,float,bool,str, 1=name, 2=default value</param>
    public void GetValFromOuter(string param)
    {
        string[] prm = param.Split(',');
        string valtype = prm[0];
        string name = prm[1];
#if !UNITY_EDITOR && UNITY_WEBGL
        if (valtype == "int") 
        {
            ReceiveIntVal(GetIntVal(name));
        }
        else if (valtype == "float") 
        {
            ReceiveFloatVal(GetFloatVal(name));
        }
        /*else if (valtype == "bool") 
        {
            ReceiveIntVal(GetBoolVal(name) ? 1 : 0);
        }*/
        else if (valtype == "str") 
        {
            ReceiveStringVal(GetStrVal(name));
        }
#endif

    }
}
