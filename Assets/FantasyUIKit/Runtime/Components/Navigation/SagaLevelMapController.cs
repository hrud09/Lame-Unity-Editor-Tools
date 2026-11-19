using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Data;
using FantasyUI.Theme;

namespace FantasyUI.Components
{
    /// <summary>
    /// Exact Reference Image 2 Interactive Saga / Level Progression Map
    /// with winding stepping-dot path, Completed (3-Star), Active (Glowing Purple), and Locked (Padlock) nodes,
    /// plus a crisp parchment Tooltip Popover ("INFORMATION" / "WELCOME!").
    /// </summary>
    public class SagaLevelMapController : MonoBehaviour
    {
        [SerializeField] private RectTransform dotsRoot;
        [SerializeField] private RectTransform nodesRoot;
        [SerializeField] private GameObject nodeTemplate;
        [SerializeField] private Sprite dotSprite;
        [SerializeField] private Sprite nodeCompleteSprite;
        [SerializeField] private Sprite nodeActiveSprite;
        [SerializeField] private Sprite nodeLockedSprite;
        [SerializeField] private Sprite padlockSprite;
        [SerializeField] private Sprite goldStarSprite;

        [Header("Parchment Tooltip Popover")]
        [SerializeField] private RectTransform tooltipRoot;
        [SerializeField] private TMP_Text tooltipTitle;
        [SerializeField] private TMP_Text tooltipBody;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        public event Action<LevelNodeData> OnLevelNodeSelected;

        public void BindReferences(
            RectTransform dots, RectTransform nodes, GameObject nodeTpl,
            Sprite dotSpr, Sprite compSpr, Sprite actSpr, Sprite lockSpr, Sprite padlockSpr, Sprite starSpr,
            RectTransform tipRoot, TMP_Text tipTitle, TMP_Text tipBody)
        {
            dotsRoot = dots; nodesRoot = nodes; nodeTemplate = nodeTpl;
            dotSprite = dotSpr; nodeCompleteSprite = compSpr; nodeActiveSprite = actSpr;
            nodeLockedSprite = lockSpr; padlockSprite = padlockSpr; goldStarSprite = starSpr;
            tooltipRoot = tipRoot; tooltipTitle = tipTitle; tooltipBody = tipBody;
        }

        public void BuildPath(IList<LevelNodeData> levels, IList<Vector2> positions)
        {
            for (int i = 0; i < _spawned.Count; i++)
                if (_spawned[i] != null) Destroy(_spawned[i]);
            _spawned.Clear();

            if (levels == null || positions == null || nodeTemplate == null || nodesRoot == null) return;
            nodeTemplate.SetActive(false);

            int count = Mathf.Min(levels.Count, positions.Count);

            // Spawn winding stepping dots between nodes
            if (dotsRoot != null && dotSprite != null)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    Vector2 a = positions[i];
                    Vector2 b = positions[i + 1];
                    Vector2 mid = (a + b) * 0.5f + new Vector2(0f, (i % 2 == 0 ? 22f : -22f));
                    const int steps = 4;
                    for (int s = 1; s < steps; s++)
                    {
                        float t = s / (float)steps;
                        Vector2 p = (1 - t) * (1 - t) * a + 2 * (1 - t) * t * mid + t * t * b;
                        var dotGo = new GameObject($"Dot_{i}_{s}", typeof(RectTransform), typeof(Image));
                        dotGo.transform.SetParent(dotsRoot, false);
                        var img = dotGo.GetComponent<Image>();
                        img.sprite = dotSprite;
                        img.raycastTarget = false;
                        img.rectTransform.sizeDelta = new Vector2(14f, 14f);
                        img.rectTransform.anchoredPosition = p;
                        _spawned.Add(dotGo);
                    }
                }
            }

            // Spawn level nodes
            for (int i = 0; i < count; i++)
            {
                var lvl = levels[i];
                var go = Instantiate(nodeTemplate, nodesRoot);
                go.name = $"LevelNode_{lvl.levelNumber}";
                go.SetActive(true);
                ((RectTransform)go.transform).anchoredPosition = positions[i];
                _spawned.Add(go);

                var bg = go.GetComponent<Image>();
                var numTxt = go.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
                var lockImg = go.transform.Find("LockIcon")?.GetComponent<Image>();
                var starsRow = go.transform.Find("StarsRow");

                if (bg != null)
                {
                    bg.sprite = lvl.state switch
                    {
                        LevelNodeState.Completed => nodeCompleteSprite,
                        LevelNodeState.Active    => nodeActiveSprite,
                        _                        => nodeLockedSprite
                    };
                }
                if (numTxt != null) numTxt.text = lvl.levelNumber.ToString();
                if (lockImg != null)
                {
                    lockImg.sprite = padlockSprite;
                    lockImg.gameObject.SetActive(lvl.state == LevelNodeState.Locked);
                }
                if (starsRow != null)
                {
                    starsRow.gameObject.SetActive(lvl.state == LevelNodeState.Completed);
                    for (int s = 0; s < starsRow.childCount; s++)
                    {
                        var stImg = starsRow.GetChild(s).GetComponent<Image>();
                        if (stImg != null)
                        {
                            stImg.sprite = goldStarSprite;
                            stImg.color = s < lvl.starsEarned ? Color.white : new Color(0.25f, 0.22f, 0.28f, 0.8f);
                        }
                    }
                }

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    var captured = lvl;
                    var pos = positions[i];
                    btn.onClick.AddListener(() =>
                    {
                        UIThemeManager.Instance?.PlaySfx(UISfxCue.Click);
                        ShowTooltip(captured.levelTitle, captured.goalDescription, pos + new Vector2(0f, 84f));
                        OnLevelNodeSelected?.Invoke(captured);
                    });
                }
            }
        }

        public void ShowTooltip(string title, string body, Vector2 anchoredPos)
        {
            if (tooltipRoot == null) return;
            tooltipRoot.gameObject.SetActive(true);
            tooltipRoot.anchoredPosition = anchoredPos;
            if (tooltipTitle != null) tooltipTitle.text = title;
            if (tooltipBody != null) tooltipBody.text = body;
        }
    }
}
