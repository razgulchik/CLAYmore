using UnityEngine;
using CLAYmore.ECS;

namespace CLAYmore
{
    public class AbilityContainerUI : MonoBehaviour
    {
        public Transform                   column;
        public GameObject                  iconPrefab;
        public ModifierCategoryBackground[] categoryBackgrounds;

        private void Awake()
        {
            if (column == null) return;
            foreach (Transform child in column)
                Destroy(child.gameObject);
        }

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<ModifierChosenEvent>(OnModifierChosen);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<ModifierChosenEvent>(OnModifierChosen);
        }

        private void OnModifierChosen(ModifierChosenEvent evt)
        {
            if (iconPrefab == null || column == null) return;

            foreach (Transform child in column)
            {
                var existing = child.GetComponent<AbilityIconUI>();
                if (existing != null && existing.Modifier == evt.Modifier)
                {
                    existing.SetLevel(evt.NewLevel);
                    return;
                }
            }

            var icon = Instantiate(iconPrefab, column);
            icon.GetComponent<AbilityIconUI>()?.Setup(evt.Modifier, evt.NewLevel, GetBackground(evt.Modifier.category));
        }

        private Sprite GetBackground(ModifierCategory category)
        {
            if (categoryBackgrounds == null) return null;
            foreach (var b in categoryBackgrounds)
                if (b.category == category) return b.sprite;
            return null;
        }
    }
}
