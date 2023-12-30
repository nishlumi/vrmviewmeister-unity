using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserHandleSpace
{
    public class ownscr_HandMenuOpener : MonoBehaviour
    {
        [SerializeField]
        private ownscr_HandMenu Hand;

        bool IsOpenMenu;

        // Start is called before the first frame update
        void Start()
        {
            IsOpenMenu = false;
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter(Collider other)
        {
            
        }
        private void OnTriggerExit(Collider other)
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
                if (IsOpenMenu)
                {
                    Hand.ShowHandMenu(false);
                    IsOpenMenu = false;
                }
                else
                {
                    Hand.ShowHandMenu(true);
                    IsOpenMenu = true;
                }
            }
        }
    }

}
