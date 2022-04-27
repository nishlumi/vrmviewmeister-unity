using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ownscr_BtnOperateParts : MonoBehaviour
{
    public GameObject infoObject;
    private bool showInfo;
    private Vector3 infoPos;

    // Start is called before the first frame update
    void Start()
    {
        showInfo = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerClick()
    {
        RectTransform rpos = infoObject.GetComponent<RectTransform>();
        showInfo = !showInfo;
        showUIObject(showInfo);
    }
    void showUIObject(bool flag)
    {
        RectTransform rpos = infoObject.GetComponent<RectTransform>();
        if (flag)
        {
            rpos.anchoredPosition = new Vector2(165.1f, 60f);
        }
        else
        {
            rpos.anchoredPosition = new Vector2(0 - rpos.sizeDelta.x, 60f);
        }
    }
}
