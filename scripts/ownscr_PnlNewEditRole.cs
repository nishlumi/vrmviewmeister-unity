using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ownscr_PnlNewEditRole : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        /*GameObject c1 = transform.Find("lab_ObjectName").gameObject;
        GameObject c2 = transform.Find("inp_ObjectRole").gameObject;
        GameObject c3 = transform.Find("hdn_ObjectId").gameObject;

        if (c1 != null)
        {
            DestroyChildren(c1.transform);
            Destroy(c1);
        }
        if (c2 != null)
        {
            DestroyChildren(c2.transform);
            Destroy(c2);
        }
        if (c3 != null)
        {
            DestroyChildren(c3.transform);
            Destroy(c3);
        }*/
    }
    public void DestroyChildren(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public List<string> GetEditedRoleList()
    {
        GameObject c1 = transform.Find("lab_ObjectName").gameObject;
        GameObject c2 = transform.Find("inp_ObjectRole").gameObject;
        GameObject c3 = transform.Find("hdn_ObjectId").gameObject;

        List<string> ret = new List<string>();
        
        ret.Add(c3.GetComponent<Text>().text);
        ret.Add(c2.GetComponent<InputField>().text);


        return ret;
    }
    public int GetSelectedAvatarList()
    {
        int ret = -1;

        GameObject c2 = transform.Find("sel_ObjectAvatar").gameObject;
        Dropdown dp = c2.GetComponent<Dropdown>();
        ret = dp.value;

        return ret;
    }
    public void SelectAvatarList(int index)
    {
        GameObject c2 = transform.Find("sel_ObjectAvatar").gameObject;
        Dropdown dp = c2.GetComponent<Dropdown>();
        dp.value = index;
    }
}
