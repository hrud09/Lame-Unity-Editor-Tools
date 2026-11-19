using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FantasyUI.Components
{
    /// <summary>
    /// Universal modular fantasy button controller supporting Iron-Capped Slabs, Price Pills,
    /// Circular Iron Rings, Quick Tiles, and Bookmark '+' tags.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class FantasyButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text priceOrBadgeText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject badgeContainer;

        public Button Button => button != null ? button : (button = GetComponent<Button>());
        public event Action OnClicked;

        private void Awake()
        {
            if (Button != null)
                Button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        public void BindReferences(Button btn, TMP_Text label, Image bg, Image icon = null, TMP_Text badge = null, GameObject badgeRoot = null)
        {
            button = btn;
            labelText = label;
            backgroundImage = bg;
            iconImage = icon;
            priceOrBadgeText = badge;
            badgeContainer = badgeRoot;
        }

        public void SetLabel(string text)
        {
            if (labelText != null) labelText.text = text;
        }

        public void SetPriceOrBadge(string text, bool visible = true)
        {
            if (badgeContainer != null) badgeContainer.SetActive(visible);
            if (priceOrBadgeText != null) priceOrBadgeText.text = text;
        }

        public void SetIcon(Sprite sprite)
        {
            if (iconImage == null) return;
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        public void SetInteractable(bool interactable)
        {
            if (Button != null) Button.interactable = interactable;
        }
    }
}
