using UnityEngine;
using UnityEngine.UI;

namespace CLAYmore
{
    public class PauseButtonContainer : MonoBehaviour
    {
        private void Start()
        {
            var btn = GetComponentInChildren<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => PauseManager.Instance.ToggleUserPause());
        }
    }
}
