using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FantasyUI.Data;

namespace FantasyUI.Components
{
    /// <summary>
    /// Dynamic Circular Dark-Fantasy Minimap with rotating player arrow, compass N indicator,
    /// pooled typed map markers, and clamped screen-edge indicators.
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] private RectTransform mapMarkersContainer;
        [SerializeField] private RectTransform edgeMarkersContainer;
        [SerializeField] private RectTransform playerArrow;
        [SerializeField] private float mapRadius = 86f;

        [Header("Marker Sprites")]
        [SerializeField] private Sprite friendlyTowerSprite;
        [SerializeField] private Sprite enemyTowerSprite;
        [SerializeField] private Sprite shieldSprite;
        [SerializeField] private Sprite bossSkullSprite;
        [SerializeField] private Sprite goldCrystalSprite;
        [SerializeField] private Sprite blipSprite;

        private readonly Dictionary<string, Image> _markers = new Dictionary<string, Image>();

        public void BindReferences(RectTransform markersRoot, RectTransform edgeRoot, RectTransform arrow, float radius,
            Sprite friendly, Sprite enemy, Sprite shield, Sprite boss, Sprite crystal, Sprite blip)
        {
            mapMarkersContainer = markersRoot;
            edgeMarkersContainer = edgeRoot;
            playerArrow = arrow;
            mapRadius = radius;
            friendlyTowerSprite = friendly;
            enemyTowerSprite = enemy;
            shieldSprite = shield;
            bossSkullSprite = boss;
            goldCrystalSprite = crystal;
            blipSprite = blip;
        }

        public void SetPlayerHeading(float degreesClockwise)
        {
            if (playerArrow != null)
                playerArrow.localRotation = Quaternion.Euler(0f, 0f, -degreesClockwise);
        }

        public void UpsertMarker(MinimapMarkerData data)
        {
            if (data == null || mapMarkersContainer == null) return;
            if (!_markers.TryGetValue(data.id, out var img) || img == null)
            {
                var go = new GameObject($"Marker_{data.id}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(data.clampToEdge && edgeMarkersContainer != null ? edgeMarkersContainer : mapMarkersContainer, false);
                img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.rectTransform.sizeDelta = new Vector2(22f, 22f);
                _markers[data.id] = img;
            }

            img.sprite = ResolveSprite(data.kind);
            img.color = ResolveTint(data.kind);

            Vector2 pos = data.normalizedMapPos;
            if (pos.magnitude > 1f) pos = pos.normalized;
            img.rectTransform.anchoredPosition = pos * mapRadius;
            img.gameObject.SetActive(true);
        }

        public void RemoveMarker(string id)
        {
            if (_markers.TryGetValue(id, out var img) && img != null)
            {
                Destroy(img.gameObject);
                _markers.Remove(id);
            }
        }

        public void ClearAllMarkers()
        {
            foreach (var kv in _markers)
            {
                if (kv.Value != null) Destroy(kv.Value.gameObject);
            }
            _markers.Clear();
        }

        private Sprite ResolveSprite(MinimapMarkerKind kind)
        {
            return kind switch
            {
                MinimapMarkerKind.FriendlyTower  => friendlyTowerSprite != null ? friendlyTowerSprite : shieldSprite,
                MinimapMarkerKind.EnemyTower     => enemyTowerSprite != null ? enemyTowerSprite : bossSkullSprite,
                MinimapMarkerKind.Shield         => shieldSprite,
                MinimapMarkerKind.BossSkull      => bossSkullSprite,
                MinimapMarkerKind.GoldCrystal    => goldCrystalSprite,
                _                                => blipSprite != null ? blipSprite : goldCrystalSprite
            };
        }

        private Color ResolveTint(MinimapMarkerKind kind)
        {
            return kind switch
            {
                MinimapMarkerKind.FriendlyTower  => new Color32(66, 214, 164, 255),
                MinimapMarkerKind.FriendlyAvatar => new Color32(66, 214, 164, 255),
                MinimapMarkerKind.EnemyTower     => new Color32(236, 78, 90, 255),
                MinimapMarkerKind.EnemyAvatar    => new Color32(236, 78, 90, 255),
                MinimapMarkerKind.BossSkull      => new Color32(255, 96, 106, 255),
                MinimapMarkerKind.GoldCrystal    => new Color32(255, 218, 68, 255),
                _                                => Color.white
            };
        }
    }
}
