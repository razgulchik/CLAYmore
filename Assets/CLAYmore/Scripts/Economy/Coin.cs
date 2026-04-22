using DG.Tweening;
using UnityEngine;

namespace CLAYmore
{
    // Visual coin prefab.
    // Economy.Add() is called by Pot via DOTween.OnComplete after the arc animation lands.
    // Expand this component when coins become player-collectible pickups.
    [RequireComponent(typeof(SpriteRenderer))]
    public class Coin : MonoBehaviour
    {
        private void OnDisable() => transform.DOKill();
    }
}
