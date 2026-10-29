using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Effects
{
    public static class RabbitAbsorption
    {
        private sealed class Fragment
        {
            public LineRenderer Line;
            public Vector3[] Start;
            public Vector3 Bend;
            public float Delay;
            public float Width;
        }

        public static async UniTask Play(SpriteRenderer source, Camera camera, Material material,
            float fromProgress, float toProgress)
        {
            if (source == null || camera == null || material == null) return;
            var root = new GameObject("Absorbed interior energy");
            root.transform.SetParent(source.transform, false);
            var fragments = new List<Fragment>();
            var sprite = source.sprite;
            var polygons = new List<List<Vector2>>();
            for (var polygon = 0; polygon < sprite.GetPhysicsShapeCount(); polygon++)
            {
                var points = new List<Vector2>();
                sprite.GetPhysicsShape(polygon, points);
                polygons.Add(points);
            }
            var scale = Mathf.Max(source.bounds.size.x, source.bounds.size.y);
            var token = source.GetCancellationTokenOnDestroy();
            try
            {
                // Spawn at the advancing burn front inside the rabbit, never its silhouette.
                for (var attempt = 0; attempt < 24 && fragments.Count < 4; attempt++)
                {
                    var progress = Mathf.Lerp(fromProgress, toProgress, (attempt % 4 + 0.5f) / 4f);
                    var radius = Mathf.Lerp(-0.08f, 0.85f, progress);
                    if (radius <= 0f) continue;
                    var angle = attempt * 2.399963f + toProgress * 19f;
                    var normalized = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    var bounds = sprite.bounds;
                    var local = new Vector2(bounds.center.x + normalized.x * bounds.size.x,
                        bounds.center.y + normalized.y * bounds.size.y);
                    if (!InsideSprite(local, polygons)) continue;
                    if (source.flipX) local.x = -local.x;
                    if (source.flipY) local.y = -local.y;
                    var start = source.transform.TransformPoint(local);
                    var go = new GameObject("Interior energy strand");
                    go.transform.SetParent(root.transform, false);
                    var line = go.AddComponent<LineRenderer>();
                    line.sharedMaterial = material;
                    line.useWorldSpace = true;
                    line.positionCount = 7;
                    line.numCapVertices = 3;
                    line.numCornerVertices = 2;
                    line.sortingLayerID = source.sortingLayerID;
                    line.sortingOrder = source.sortingOrder + 1;
                    line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    line.receiveShadows = false;
                    var positions = new Vector3[line.positionCount];
                    for (var i = 0; i < positions.Length; i++) positions[i] = start;
                    line.SetPositions(positions);
                    fragments.Add(new Fragment
                    {
                        Line = line, Start = positions,
                        Bend = camera.transform.right * Mathf.Sin(angle) * scale * 0.22f
                            + camera.transform.up * Mathf.Cos(angle) * scale * 0.18f,
                        Delay = fragments.Count * 0.02f,
                        Width = Mathf.Clamp(scale * 0.003f, 0.005f, 0.016f)
                    });
                }
                if (fragments.Count == 0) return;
                var elapsed = 0f;
                const float travelTime = 0.65f;
                while (elapsed < travelTime + 0.25f)
                {
                    if (token.IsCancellationRequested || camera == null) return;
                    // End in front of the moving camera, away from its clipping plane.
                    var destination = camera.transform.position + camera.transform.forward * Mathf.Max(0.35f, camera.nearClipPlane + 0.12f)
                        - camera.transform.up * 0.12f;
                    foreach (var fragment in fragments)
                    {
                        var t = Mathf.Clamp01((elapsed - fragment.Delay) / travelTime);
                        var alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.1f))
                            * (1f - Mathf.SmoothStep(0.7f, 1f, t));
                        var tint = new Color(0.52f, 0.59f, 0.57f, alpha * 0.85f);
                        fragment.Line.startColor = tint;
                        fragment.Line.endColor = new Color(tint.r, tint.g, tint.b, alpha * 0.25f);
                        fragment.Line.widthMultiplier = fragment.Width * Mathf.Lerp(1f, 0.2f, t);
                        for (var j = 0; j < fragment.Start.Length; j++)
                        {
                            var u = Mathf.Clamp01((elapsed - fragment.Delay - j * 0.013f) / travelTime);
                            var eased = u * u;
                            var start = fragment.Start[j];
                            var control1 = start + fragment.Bend;
                            var control2 = destination + fragment.Bend * 0.3f - camera.transform.forward * 0.2f;
                            var v = 1f - eased;
                            var position = v*v*v*start + 3f*v*v*eased*control1
                                + 3f*v*eased*eased*control2 + eased*eased*eased*destination;
                            fragment.Line.SetPosition(j, position);
                        }
                    }
                    await UniTask.NextFrame();
                    elapsed += Time.deltaTime;
                }
            }
            finally
            {
                if (root != null) Object.Destroy(root);
            }
        }
        private static bool InsideSprite(Vector2 point, List<List<Vector2>> polygons)
        {
            foreach (var polygon in polygons)
            {
                var inside = false;
                for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
                {
                    var a = polygon[i];
                    var b = polygon[j];
                    if ((a.y > point.y) != (b.y > point.y)
                        && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                        inside = !inside;
                }
                if (inside) return true;
            }
            return false;
        }
    }
}
