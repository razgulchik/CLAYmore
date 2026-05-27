using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CLAYmore
{
    public class SkipUI : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onClick;

        public void OnPointerClick(PointerEventData _) => onClick.Invoke();
    }
}
