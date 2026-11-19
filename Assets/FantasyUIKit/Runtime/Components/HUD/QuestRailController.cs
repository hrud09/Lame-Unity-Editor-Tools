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
    /// Exact Reference Image 1 vertical hanging chained Quests Rail with dynamic angled status ribbons
    /// (PROGRESS! purple, TIME IS UP! crimson, COLLECT NOW! emerald).
    /// </summary>
    public class QuestRailController : MonoBehaviour
    {
        [SerializeField] private Transform questItemsRoot;
        [SerializeField] private GameObject questRowTemplate;
        [SerializeField] private Sprite ribbonPurple;
        [SerializeField] private Sprite ribbonCrimson;
        [SerializeField] private Sprite ribbonEmerald;

        private readonly List<GameObject> _spawnedRows = new List<GameObject>();
        public event Action<QuestItemData> OnQuestClicked;

        public void BindReferences(Transform root, GameObject template, Sprite purple, Sprite crimson, Sprite emerald)
        {
            questItemsRoot = root;
            questRowTemplate = template;
            ribbonPurple = purple;
            ribbonCrimson = crimson;
            ribbonEmerald = emerald;
        }

        public void SetQuests(IList<QuestItemData> quests)
        {
            for (int i = 0; i < _spawnedRows.Count; i++)
                if (_spawnedRows[i] != null) Destroy(_spawnedRows[i]);
            _spawnedRows.Clear();

            if (quests == null || questRowTemplate == null || questItemsRoot == null) return;
            questRowTemplate.SetActive(false);

            for (int i = 0; i < quests.Count; i++)
            {
                var q = quests[i];
                var row = Instantiate(questRowTemplate, questItemsRoot);
                row.name = $"QuestRow_{q.id}";
                row.SetActive(true);
                _spawnedRows.Add(row);

                var icon = row.transform.Find("SlotFrame/Icon")?.GetComponent<Image>();
                if (icon != null && q.icon != null) icon.sprite = q.icon;

                var ribbonImg = row.transform.Find("StatusRibbon")?.GetComponent<Image>();
                var ribbonTxt = row.transform.Find("StatusRibbon/Label")?.GetComponent<TMP_Text>();

                if (ribbonImg != null)
                {
                    ribbonImg.sprite = q.status switch
                    {
                        QuestStatus.InProgress => ribbonPurple,
                        QuestStatus.TimeIsUp   => ribbonCrimson,
                        _                      => ribbonEmerald
                    };
                }
                if (ribbonTxt != null)
                {
                    ribbonTxt.text = q.status switch
                    {
                        QuestStatus.InProgress => "PROGRESS!",
                        QuestStatus.TimeIsUp   => "TIME IS UP!",
                        _                      => "COLLECT NOW!"
                    };
                }

                var btn = row.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    var captured = q;
                    btn.onClick.AddListener(() =>
                    {
                        UIThemeManager.Instance?.PlaySfx(captured.status == QuestStatus.CollectNow ? UISfxCue.CoinClink : UISfxCue.Click);
                        OnQuestClicked?.Invoke(captured);
                    });
                }
            }
        }
    }
}
