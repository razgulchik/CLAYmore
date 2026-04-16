using UnityEngine;
using CLAYmore.ECS;

namespace CLAYmore
{
    /// <summary>
    /// Scroll-panel that tracks acquired modifiers.
    /// Listens to ModifierChosenEvent and spawns a card in the Content container.
    /// Assign content (ScrollView/Viewport/Content) and cardPrefab in the Inspector.
    /// </summary>
    public class ModifierListUI : MonoBehaviour
    {
        public Transform       content;
        public GameObject      cardPrefab;

        private void Start()
        {
            World.Current?.Events.Subscribe<ModifierChosenEvent>(OnModifierChosen);
        }

        private void OnDestroy()
        {
            World.Current?.Events.Unsubscribe<ModifierChosenEvent>(OnModifierChosen);
        }

        private void OnModifierChosen(ModifierChosenEvent evt)
        {
if (cardPrefab == null || content == null) return;

            // If the modifier is already in the list — just update its level.
            foreach (Transform child in content)
            {
                var existing = child.GetComponent<ModifierAcquiredCardUI>();
                if (existing != null && existing.Modifier == evt.Modifier)
                {
                    existing.SetLevel(evt.NewLevel);
                    return;
                }
            }

            // New modifier — spawn a card.
            var card = Instantiate(cardPrefab, content);
            card.GetComponent<ModifierAcquiredCardUI>()?.Setup(evt.Modifier, evt.NewLevel);
        }
    }
}
