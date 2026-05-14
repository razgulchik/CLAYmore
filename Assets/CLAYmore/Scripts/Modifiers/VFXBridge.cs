using UnityEngine;

namespace CLAYmore
{
    public class VFXBridge : MonoBehaviour
    {
        public System.Action OnImpact;
        public System.Action OnComplete;

        // Called by Animation Event
        public void Impact()   => OnImpact?.Invoke();

        // Called by Animation Event (last frame)
        public void Complete() => OnComplete?.Invoke();

        private void OnDisable()
        {
            OnImpact   = null;
            OnComplete = null;
        }
    }
}
