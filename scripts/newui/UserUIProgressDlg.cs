using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

using UserHandleSpace;

namespace UserUISpace
{
    public class UserUIProgressDlg : MonoBehaviour
    {
        private UIDocument MainUI;
        private VisualElement rootElement;
        private ProgressBar progress;

        // Start is called before the first frame update
        void Start()
        {
            MainUI = GetComponent<UIDocument>();
            rootElement = MainUI.rootVisualElement;

            progress = rootElement.Q<ProgressBar>("progress");
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void EnableDlg(bool flag)
        {
            if (flag == true)
            {
                rootElement.style.display = DisplayStyle.Flex;
            }
            else
            {
                rootElement.style.display = DisplayStyle.None;
            }
            
        }
        public void SetProgressValue(float currentValue)
        {
            progress.value = currentValue;
        }
    }

}
