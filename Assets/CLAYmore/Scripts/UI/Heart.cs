using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CLAYmore
{
    [RequireComponent(typeof(Image))]
    public class Heart : MonoBehaviour
    {
        public Sprite fullSprite;
        public Sprite emptySprite;

        [Header("Damage Animation")]
        public float popScale    = 1.4f;
        public float popDuration = 0.08f;
        public float popReturn   = 0.15f;

        private Image _img;
        private Coroutine _anim;

        private void Awake() => _img = GetComponent<Image>();

        public void SetAlive(bool alive)
        {
            _img.sprite = alive ? fullSprite : emptySprite;
            if (!alive) PlayPop();
        }

        private void PlayPop()
        {
            if (_anim != null) StopCoroutine(_anim);
            _anim = StartCoroutine(PopRoutine());
        }

        private IEnumerator PopRoutine()
        {
            for (float t = 0f; t < popDuration; t += Time.deltaTime)
            {
                transform.localScale = Vector3.one * Mathf.Lerp(1f, popScale, t / popDuration);
                yield return null;
            }
            for (float t = 0f; t < popReturn; t += Time.deltaTime)
            {
                transform.localScale = Vector3.one * Mathf.Lerp(popScale, 1f, t / popReturn);
                yield return null;
            }
            transform.localScale = Vector3.one;
            _anim = null;
        }
    }
}
