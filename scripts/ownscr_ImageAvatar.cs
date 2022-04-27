using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ownscr_ImageAvatar : MonoBehaviour
{
    public GameObject infoObject;
    public float objectTop;
    public float objectLeft;
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
        /*if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.collider.gameObject.name);
                //if (hit.collider.gameObject.name == this.name)
                //{
                Debug.Log(hit.collider.gameObject.name);

                OnPointerClick();

            }
        }*/
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
            rpos.anchoredPosition = new Vector2(301.4f, -62.2f);
        }
        else
        {
            rpos.anchoredPosition = new Vector2(0 - rpos.sizeDelta.x, -62.2f);
        }
    }
}
