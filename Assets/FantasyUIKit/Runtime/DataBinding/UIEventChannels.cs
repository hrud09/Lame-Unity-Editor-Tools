using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyUI.Data
{
    /// <summary>
    /// Decoupled ScriptableObject Event Channel connecting any gameplay system to the Fantasy UI HUD & Modals.
    /// Drop the same asset onto Gameplay scripts and UI views — zero direct script coupling required.
    /// </summary>
    [CreateAssetMenu(menuName = "Fantasy UI/Game UI Event Channel", fileName = "GameUIEventChannel")]
    public class UIEventChannels : ScriptableObject
    {
        public event Action<long> OnGoldChanged;
        public event Action<long> OnGemsChanged;
        public event Action<float, float> OnPlayerHealthChanged;
        public event Action<float, float> OnPlayerManaChanged;
        public event Action<float, float> OnOpponentHealthChanged;
        public event Action<float, float> OnOpponentManaChanged;
        public event Action<int, int> OnInventoryCapacityChanged;
        public event Action<string, QuestStatus> OnToastRequested;
        public event Action<CelebrationType> OnCelebrationRequested;
        public event Action<List<PowerupCardData>> OnPowerupSelectionRequested;

        public void RaiseGold(long gold) => OnGoldChanged?.Invoke(gold);
        public void RaiseGems(long gems) => OnGemsChanged?.Invoke(gems);
        public void RaisePlayerHealth(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);
        public void RaisePlayerMana(float current, float max) => OnPlayerManaChanged?.Invoke(current, max);
        public void RaiseOpponentHealth(float current, float max) => OnOpponentHealthChanged?.Invoke(current, max);
        public void RaiseOpponentMana(float current, float max) => OnOpponentManaChanged?.Invoke(current, max);
        public void RaiseInventoryCapacity(int used, int max) => OnInventoryCapacityChanged?.Invoke(used, max);
        public void RaiseToast(string message, QuestStatus style = QuestStatus.CollectNow) => OnToastRequested?.Invoke(message, style);
        public void RaiseCelebration(CelebrationType type) => OnCelebrationRequested?.Invoke(type);
        public void RaisePowerupSelection(List<PowerupCardData> cards) => OnPowerupSelectionRequested?.Invoke(cards);
    }
}
