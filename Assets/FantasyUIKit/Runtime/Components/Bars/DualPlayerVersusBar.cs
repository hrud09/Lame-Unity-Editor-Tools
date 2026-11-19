using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Data;

namespace FantasyUI.Components
{
    /// <summary>
    /// Exact Reference Image 1 & 2 bottom carved-stone Dual-Player Versus Dock
    /// with Crowned Skull VS emblem, left/right hero portraits, names, and animated HP/MP bars.
    /// </summary>
    public class DualPlayerVersusBar : MonoBehaviour
    {
        [SerializeField] private UIEventChannels eventChannel;

        [Header("Left Player")]
        [SerializeField] private TMP_Text leftPlayerName;
        [SerializeField] private Image leftAvatar;
        [SerializeField] private FantasyProgressBar leftHpBar;
        [SerializeField] private FantasyProgressBar leftMpBar;

        [Header("Right Player / Opponent")]
        [SerializeField] private TMP_Text rightPlayerName;
        [SerializeField] private Image rightAvatar;
        [SerializeField] private FantasyProgressBar rightHpBar;
        [SerializeField] private FantasyProgressBar rightMpBar;

        private void OnEnable()
        {
            if (eventChannel == null) return;
            eventChannel.OnPlayerHealthChanged += HandleLeftHp;
            eventChannel.OnPlayerManaChanged += HandleLeftMp;
            eventChannel.OnOpponentHealthChanged += HandleRightHp;
            eventChannel.OnOpponentManaChanged += HandleRightMp;
        }

        private void OnDisable()
        {
            if (eventChannel == null) return;
            eventChannel.OnPlayerHealthChanged -= HandleLeftHp;
            eventChannel.OnPlayerManaChanged -= HandleLeftMp;
            eventChannel.OnOpponentHealthChanged -= HandleRightHp;
            eventChannel.OnOpponentManaChanged -= HandleRightMp;
        }

        public void BindReferences(
            TMP_Text lName, Image lAvatar, FantasyProgressBar lHp, FantasyProgressBar lMp,
            TMP_Text rName, Image rAvatar, FantasyProgressBar rHp, FantasyProgressBar rMp,
            UIEventChannels channel = null)
        {
            leftPlayerName = lName; leftAvatar = lAvatar; leftHpBar = lHp; leftMpBar = lMp;
            rightPlayerName = rName; rightAvatar = rAvatar; rightHpBar = rHp; rightMpBar = rMp;
            eventChannel = channel;
        }

        public void SetPlayers(string leftName, Sprite leftIcon, string rightName, Sprite rightIcon)
        {
            if (leftPlayerName != null) leftPlayerName.text = leftName;
            if (leftAvatar != null && leftIcon != null) leftAvatar.sprite = leftIcon;
            if (rightPlayerName != null) rightPlayerName.text = rightName;
            if (rightAvatar != null && rightIcon != null) rightAvatar.sprite = rightIcon;
        }

        private void HandleLeftHp(float cur, float max) => leftHpBar?.SetValue(cur, max);
        private void HandleLeftMp(float cur, float max) => leftMpBar?.SetValue(cur, max);
        private void HandleRightHp(float cur, float max) => rightHpBar?.SetValue(cur, max);
        private void HandleRightMp(float cur, float max) => rightMpBar?.SetValue(cur, max);
    }
}
