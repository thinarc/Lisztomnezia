using UnityEngine;

namespace _Project.Scripts.HUD
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class HoldStartIndicator : UnityEngine.UI.MaskableGraphic
    {
        public float Progress { get; private set; }
        public void SetProgress(float value) { Progress = Mathf.Clamp01(value); SetVerticesDirty(); }
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear();
            var radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.42f;
            Ring(vh, radius, 1.2f, 1f, new Color(0.7f, 0.76f, 0.7f, 0.2f));
            Ring(vh, radius + 6f, 0.6f, 1f, new Color(0.7f, 0.76f, 0.7f, 0.08f));
            Ring(vh, radius, 2.5f, Progress, new Color(0.87f, 0.91f, 0.78f, 0.95f));
            Ring(vh, 3f + Progress * 2f, 2f, 1f, new Color(0.87f, 0.91f, 0.78f, 0.35f + Progress * 0.5f));
        }
        private static void Ring(UnityEngine.UI.VertexHelper vh, float radius, float width, float fill, Color tint)
        {
            if (fill <= 0f) return;
            var count = Mathf.CeilToInt(128 * fill);
            for (var i = 0; i < count; i++)
            {
                var a = Mathf.PI * 0.5f - Mathf.PI * 2f * fill * i / count;
                var b = Mathf.PI * 0.5f - Mathf.PI * 2f * fill * (i + 1) / count;
                var start = vh.currentVertCount;
                vh.AddVert(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (radius - width * 0.5f), tint, Vector2.zero);
                vh.AddVert(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (radius + width * 0.5f), tint, Vector2.zero);
                vh.AddVert(new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * (radius + width * 0.5f), tint, Vector2.zero);
                vh.AddVert(new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * (radius - width * 0.5f), tint, Vector2.zero);
                vh.AddTriangle(start, start+1, start+2); vh.AddTriangle(start, start+2, start+3);
            }
        }
    }
}
