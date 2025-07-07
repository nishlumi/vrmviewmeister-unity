using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UserHandleSpace;

namespace UserUISpace
{
    public class UserRelativeUIConnect : MonoBehaviour
    {
        public MobileUIReceiver1 receiver1;
        public UserUIARWin uiwin;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (receiver1 != null)
            {
                if (receiver1.IsShow)
                {
                    uiwin.ShowRotatePopup(receiver1.IsShow, receiver1.label, receiver1.partsname);
                    receiver1.IsShow = false;
                }
            }
        }
    }

}
