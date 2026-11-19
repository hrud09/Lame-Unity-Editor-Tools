using System;
using UnityEngine;

namespace FantasyUI.Data
{
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum InventoryCategory
    {
        All,
        FishBag,
        PotionBag,
        Weapons,
        Hardware
    }

    public enum QuestStatus
    {
        InProgress,
        TimeIsUp,
        CollectNow
    }

    public enum LevelNodeState
    {
        Locked,
        Active,
        Completed
    }

    public enum CelebrationType
    {
        Victory,
        Defeated,
        Amazing,
        Awesome
    }

    public enum MinimapMarkerKind
    {
        FriendlyTower,
        EnemyTower,
        Shield,
        BossSkull,
        GoldCrystal,
        FriendlyAvatar,
        EnemyAvatar,
        Blip
    }

    [Serializable]
    public class InventoryItemData
    {
        public string id;
        public string displayName;
        public string description;
        public InventoryCategory category;
        public ItemRarity rarity;
        public Sprite icon;
        public int count = 1;
        public int sellValue = 100;
        public string actionLabel = "USE";
    }

    [Serializable]
    public class WeaponSlotData
    {
        public string weaponId;
        public string weaponName;
        public Sprite icon;
        public float cooldownDuration = 3f;
        public bool isActive = true;
    }

    [Serializable]
    public class PowerupCardData
    {
        public string id;
        public string title;
        public string description;
        public Sprite icon;
        public int cost;
        public string buttonLabel = "EQUIP";
    }

    [Serializable]
    public class QuestItemData
    {
        public string id;
        public string title;
        public Sprite icon;
        public QuestStatus status;
        public int rewardGold = 500;
    }

    [Serializable]
    public class LeaderboardEntryData
    {
        public int rank;
        public string playerName;
        public long score;
        public Sprite avatar;
    }

    [Serializable]
    public class StatEntryData
    {
        public string statName;
        public string valueText;
        public float normalizedFill;
        public int upgradeCost;
        public Sprite icon;
    }

    [Serializable]
    public class LevelNodeData
    {
        public int levelNumber;
        public string levelTitle;
        public string goalDescription;
        public LevelNodeState state;
        public int starsEarned; // 0..3
    }

    [Serializable]
    public class MinimapMarkerData
    {
        public string id;
        public Vector2 normalizedMapPos; // (-1..1, -1..1)
        public MinimapMarkerKind kind;
        public bool clampToEdge;
    }
}
