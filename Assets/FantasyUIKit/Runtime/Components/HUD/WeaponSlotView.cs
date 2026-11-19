using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Animation;
using FantasyUI.Data;
using FantasyUI.Theme;

namespace FantasyUI.Components
{
    /// <summary>
    /// Circular iron-rimmed Weapon / Ability slot with radial cooldown overlay, numeric timer badge, and punch feedback.
    /// Replaces GameplayScene's Weapon UI prefab in full Dark-Fantasy style.
    /// </summary>
    public class WeaponSlotView : MonoBehaviour
    {
        [SerializeField] private Button slotButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private GameObject inactiveBadge;
        [SerializeField] private TMP_Text timerLabel;
        [SerializeField] private TMP_Text nameLabel;

        private WeaponSlotData _data;
        private float _cooldownRemaining;
        private float _cooldownTotal;

        public bool IsOnCooldown => _cooldownRemaining > 0f;
        public WeaponSlotData Data => _data;
        public event Action<WeaponSlotView> OnWeaponTriggered;

        private void Awake()
        {
            if (slotButton == null) slotButton = GetComponent<Button>();
            if (slotButton != null) slotButton.onClick.AddListener(TryTrigger);
            UpdateCooldownVisuals();
        }

        private void Update()
        {
            if (_cooldownRemaining <= 0f) return;
            _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - Time.deltaTime);
            UpdateCooldownVisuals();
        }

        public void BindReferences(Button btn, Image icon, Image cdFill, GameObject badge, TMP_Text timer, TMP_Text nameTxt)
        {
            slotButton = btn;
            iconImage = icon;
            cooldownFillImage = cdFill;
            inactiveBadge = badge;
            timerLabel = timer;
            nameLabel = nameTxt;
        }

        public void Configure(WeaponSlotData data)
        {
            _data = data;
            if (data == null) return;
            if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;
            if (nameLabel != null) nameLabel.text = data.weaponName;
            _cooldownTotal = Mathf.Max(0.1f, data.cooldownDuration);
            _cooldownRemaining = 0f;
            UpdateCooldownVisuals();
        }

        public void TryTrigger()
        {
            if (IsOnCooldown) return;
            StartCooldown(_data != null ? _data.cooldownDuration : 2.5f);
            UIThemeManager.Instance?.PlaySfx(UISfxCue.Click);
            OnWeaponTriggered?.Invoke(this);

            UITweener.Run(this, 0.22f, EaseType.OutBack, t =>
            {
                float s = 1f - Mathf.Sin(t * Mathf.PI) * 0.14f;
                transform.localScale = new Vector3(s, s, 1f);
            }, () => transform.localScale = Vector3.one);
        }

        public void StartCooldown(float seconds)
        {
            _cooldownTotal = Mathf.Max(0.1f, seconds);
            _cooldownRemaining = _cooldownTotal;
            UpdateCooldownVisuals();
        }

        private void UpdateCooldownVisuals()
        {
            bool cooling = _cooldownRemaining > 0.01f;
            float norm = cooling ? _cooldownRemaining / Mathf.Max(0.1f, _cooldownTotal) : 0f;

            if (cooldownFillImage != null)
            {
                cooldownFillImage.enabled = cooling;
                cooldownFillImage.fillAmount = norm;
            }
            if (inactiveBadge != null) inactiveBadge.SetActive(cooling);
            if (timerLabel != null && cooling) timerLabel.text = _cooldownRemaining.ToString("0.0");
            if (slotButton != null) slotButton.interactable = !cooling;
        }
    }
}
