using UnityEngine;
using TMPro;

namespace FantasyUI.Animation
{
    /// <summary>
    /// Curves a TMP_Text mesh along a smooth fantasy arch (for Ribbon Titles & VICTORY!/DEFEATED!/AMAZING!/AWESOME! banners).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class UIArchedText : MonoBehaviour
    {
        [SerializeField] private float archHeight = 14f;
        [SerializeField] private bool rotateCharacters = true;

        private TMP_Text _tmp;
        private string _lastText;
        private float _lastArch;

        private void OnEnable()
        {
            _tmp = GetComponent<TMP_Text>();
            ApplyCurve();
        }

        private void LateUpdate()
        {
            if (_tmp == null) return;
            if (_lastText != _tmp.text || !Mathf.Approximately(_lastArch, archHeight))
                ApplyCurve();
        }

        public void SetArch(float height)
        {
            archHeight = height;
            ApplyCurve();
        }

        public void ApplyCurve()
        {
            if (_tmp == null) _tmp = GetComponent<TMP_Text>();
            if (_tmp == null) return;

            _lastText = _tmp.text;
            _lastArch = archHeight;

            _tmp.ForceMeshUpdate();
            var textInfo = _tmp.textInfo;
            if (textInfo == null || textInfo.characterCount == 0) return;

            float minX = _tmp.bounds.min.x;
            float maxX = _tmp.bounds.max.x;
            float width = maxX - minX;
            if (width <= 0.001f) return;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int matIdx = charInfo.materialReferenceIndex;
                int vertIdx = charInfo.vertexIndex;
                var verts = textInfo.meshInfo[matIdx].vertices;

                Vector3 mid = (verts[vertIdx] + verts[vertIdx + 2]) * 0.5f;
                float normX = ((mid.x - minX) / width) * 2f - 1f; // -1 .. +1
                float yOffset = (1f - normX * normX) * archHeight;
                float slope = -2f * normX * archHeight / (width * 0.5f);
                float angle = rotateCharacters ? Mathf.Atan(slope) * Mathf.Rad2Deg * 0.65f : 0f;

                Quaternion rot = Quaternion.Euler(0f, 0f, angle);
                for (int v = 0; v < 4; v++)
                {
                    Vector3 local = verts[vertIdx + v] - mid;
                    local = rot * local;
                    verts[vertIdx + v] = mid + local + new Vector3(0f, yOffset, 0f);
                }
            }

            for (int m = 0; m < textInfo.meshInfo.Length; m++)
            {
                textInfo.meshInfo[m].mesh.vertices = textInfo.meshInfo[m].vertices;
                _tmp.UpdateGeometry(textInfo.meshInfo[m].mesh, m);
            }
        }
    }
}
