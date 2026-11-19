using UnityEngine;
using UnityEngine.UI;

namespace FantasyUI.Core
{
    /// <summary>
    /// Base class for stacked popup/modal windows (Scroll Pause, Level Complete, Leather Inventory, etc.).
    /// Supports dim backdrop dismissal, Escape/Back handling, and optional game-time pausing.
    /// </summary>
    public class UIModal : UIView
    {
        [Header("Modal Behavior")]
        [SerializeField] private bool dismissOnBackdropClick = true;
        [SerializeField] private bool dismissOnEscape = true;
        [SerializeField] private bool pauseTimeScaleWhenOpen = false;
        [SerializeField] private Button backdropButton;
        [SerializeField] private Button closeButton;

        public bool DismissOnEscape => dismissOnEscape;
        public bool PauseTimeScaleWhenOpen => pauseTimeScaleWhenOpen;

        protected override void Awake()
        {
            base.Awake();
            if (backdropButton != null && dismissOnBackdropClick)
                backdropButton.onClick.AddListener(CloseSelf);
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseSelf);
        }

        public void BindModalButtons(Button backdrop, Button close)
        {
            backdropButton = backdrop;
            closeButton = close;
        }

        public virtual void CloseSelf()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.CloseModal(this);
            else
                Hide();
        }
    }
}
