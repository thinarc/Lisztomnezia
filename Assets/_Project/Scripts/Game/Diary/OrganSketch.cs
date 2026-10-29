using _Project.Scripts.Sense;
using UnityEngine;
using System.Collections.Generic;

namespace _Project.Scripts.Diary
{
    // Reshape the existing ink, preserving its material, mask and pencil style.
    internal sealed class OrganSketch
    {
        private const int Samples = 128;
        private const int StrokeCount = 20;
        private readonly ParticleSystem _splatter;
        private float _nextSplatter;
        private readonly TrailRenderer[] _trails;
        private readonly LineRenderer[] _lines;
        private readonly Vector3[] _positions = new Vector3[Samples];
        private readonly float[] _times;
        private readonly float[] _widths;
        private readonly RabbitType _type;
        private readonly Transform _page;
        private readonly Bounds _bounds;
        private float _progress;
        private readonly List<Vector3>[] _scribbles;
        private readonly Vector3[][] _captured;
        private readonly float _drawingScale;
        public bool IsForming { get; private set; }

        public OrganSketch(TrailRenderer[] trails, RabbitType type, float size, SpriteRenderer page, Material inkMaterial, Material splatterMaterial = null)
        {
            _trails = trails; _type = type;
            _drawingScale = Mathf.Clamp(size * 0.4f, 0.2f, 0.55f);
            _scribbles = new List<Vector3>[StrokeCount];
            _captured = new Vector3[StrokeCount][];
            var drawingArea = page != null ? page.transform.parent.Find("Sprite Mask")?.GetComponent<SpriteMask>() : null;
            _page = drawingArea != null ? drawingArea.transform : page != null ? page.transform : trails[0].transform.parent;
            _bounds = drawingArea != null && drawingArea.sprite != null ? drawingArea.sprite.bounds
                : page != null ? page.sprite.bounds : new Bounds(Vector3.zero, Vector3.one * size);
            _times = new float[trails.Length]; _widths = new float[StrokeCount];
            _lines = new LineRenderer[StrokeCount];
            for (var i = 0; i < trails.Length; i++) _times[i] = trails[i].time;
            for (var i = 0; i < StrokeCount; i++)
            {
                var source = trails[i % trails.Length];
                _scribbles[i] = new List<Vector3>();

                var go = new GameObject("Organ ink " + i);
                go.transform.SetParent(_page, false);
                go.layer = _page.gameObject.layer;
                var line = go.AddComponent<LineRenderer>();
                line.useWorldSpace = false; line.sharedMaterial = inkMaterial != null ? inkMaterial : source.sharedMaterial;
                line.sortingLayerID = page != null ? page.sortingLayerID : source.sortingLayerID;
                line.sortingOrder = source.sortingOrder;
                line.maskInteraction = SpriteMaskInteraction.None;
                line.widthCurve = new AnimationCurve(new Keyframe(0f,0.45f),new Keyframe(0.18f,1f),
                    new Keyframe(0.52f,0.72f),new Keyframe(0.8f,0.95f),new Keyframe(1f,0.35f));
                line.widthMultiplier = i < 4 ? 0.007f : 0.0035f; _widths[i] = line.widthMultiplier;
                line.textureMode = LineTextureMode.Tile;
                line.startColor = line.endColor = new Color(0.18f,0.18f,0.17f,0.4f); line.numCornerVertices = 2;
                line.enabled = false; _lines[i] = line;
            }
            if (splatterMaterial != null)
            {
                var go = new GameObject("Nib ink splatter"); go.transform.SetParent(_page,false);
                go.layer = _page.gameObject.layer;
                _splatter = go.AddComponent<ParticleSystem>(); _splatter.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = _splatter.main; main.loop=true; main.playOnAwake=false;
                main.simulationSpace=ParticleSystemSimulationSpace.World; main.scalingMode=ParticleSystemScalingMode.Shape;
                main.startLifetime=new ParticleSystem.MinMaxCurve(0.18f,0.45f); main.startSize=new ParticleSystem.MinMaxCurve(0.005f,0.018f);
                main.gravityModifier=0.12f; main.maxParticles=64;
                var emission=_splatter.emission; emission.enabled=false;
                var shape=_splatter.shape; shape.enabled=false;
                var renderer=go.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial=splatterMaterial;
                renderer.sortingLayerID=_lines[0].sortingLayerID; renderer.sortingOrder=155;
                var fade=_splatter.colorOverLifetime; fade.enabled=true;
                var gradient=new Gradient(); gradient.SetKeys(new[]{new GradientColorKey(Color.white,0f),new GradientColorKey(Color.white,1f)},
                    new[]{new GradientAlphaKey(0.8f,0f),new GradientAlphaKey(0.6f,0.5f),new GradientAlphaKey(0f,1f)});
                fade.color=new ParticleSystem.MinMaxGradient(gradient);
                _splatter.Play();
            }
        }

        public void Record()
        {
            var camera = Camera.main;
            if (camera == null) return;
            // The authored dot is the nib, not the animated pencil sprite's pivot.
            var ray = camera.ScreenPointToRay(camera.WorldToScreenPoint(_trails[0].transform.position));
            var plane = new Plane(_page.forward, _page.TransformPoint(_bounds.center));
            if (!plane.Raycast(ray, out var distance)) return;
            var cursorOnPage = _page.InverseTransformPoint(ray.GetPoint(distance));
            // Ignore off-paper movement instead of piling points along a clamped edge.
            if (cursorOnPage.x < _bounds.min.x || cursorOnPage.x > _bounds.max.x
                || cursorOnPage.y < _bounds.min.y || cursorOnPage.y > _bounds.max.y) return;
            var wroteInk = false;
            for (var i = 0; i < _lines.Length; i++)
            {
                // Fine parallel pencil traces, all centered on the actual cursor.
                var point = cursorOnPage + new Vector3((i % 4 - 1.5f) * _bounds.size.x * 0.002f, 0f, 0f);
                point.z = _bounds.center.z - 0.01f;
                var ink = _scribbles[i];
                if (ink.Count > 0 && (ink[ink.Count - 1] - point).sqrMagnitude < 0.0004f) continue;
                if (ink.Count >= 1024) continue;
                ink.Add(point);
                if (i == 0) wroteInk = true;
                var line = _lines[i];
                line.enabled = i < 4 && ink.Count >= 2;
                line.positionCount = ink.Count;
                line.SetPosition(ink.Count - 1, point);
            }
            if (wroteInk && _splatter != null && Time.time >= _nextSplatter)
            {
                _nextSplatter = Time.time + 0.09f;
                for (var i=0;i<2;i++)
                {
                    var angle=Random.Range(0f,Mathf.PI*2f);var speed=Random.Range(0.12f,0.5f);
                    var particle=new ParticleSystem.EmitParams();
                    particle.position=_page.TransformPoint(cursorOnPage)-camera.transform.forward*0.015f;
                    particle.velocity=(camera.transform.right*Mathf.Cos(angle)+camera.transform.up*Mathf.Sin(angle))*speed
                        -camera.transform.forward*Random.Range(0.02f,0.12f);
                    particle.startColor=new Color(0.12f,0.035f,0.025f,Random.Range(0.35f,0.65f));
                    _splatter.Emit(particle,1);
                }
            }
        }

        public void Update(float progress)
        {
            _progress = Mathf.Max(_progress, progress);
            {
                // Continue capturing and incorporating new ink throughout drawing.
                if (_progress < 0.7f || _scribbles[0].Count < 2) return;
                for (var i = 0; i < _lines.Length; i++)
                {
                    var ink = _scribbles[i].Count >= 2 ? _scribbles[i] : _scribbles[0];
                    _captured[i] ??= new Vector3[Samples];
                    var distances = new float[ink.Count];
                    for (var p = 1; p < ink.Count; p++)
                        distances[p] = distances[p - 1] + Vector3.Distance(ink[p - 1], ink[p]);
                    var segment = 0;
                    for (var p = 0; p < Samples; p++)
                    {
                        var distance = distances[ink.Count - 1] * p / (Samples - 1f);
                        while (segment < ink.Count - 2 && distances[segment + 1] < distance) segment++;
                        var t = Mathf.InverseLerp(distances[segment], distances[segment + 1], distance);
                        _captured[i][p] = Vector3.Lerp(ink[segment], ink[segment + 1], t);
                    }
                    _lines[i].positionCount = Samples;
                    _lines[i].enabled = true;
                }
                IsForming = true;
            }

            for (var i = 0; i < _lines.Length; i++)
                for (var p = 0; p < Samples; p++)
                {
                    var shape = Shape(_type, i, p / (Samples - 1f));
                    var target = _bounds.center + new Vector3(shape.x * _bounds.size.x * _drawingScale,
                        shape.y * _bounds.size.y * _drawingScale, -0.01f);
                    var start = 0.7f + 0.05f * p / (Samples - 1f);
                    var blend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, 0.98f, _progress));
                    _lines[i].SetPosition(p, Vector3.Lerp(_captured[i][p], target, blend));
                    if (i >= 4)
                    {
                        var tint = new Color(0.18f,0.17f,0.15f,0.28f * blend);
                        _lines[i].startColor = _lines[i].endColor = tint;
                    }
                }
        }

        // The ink follows the page transform; world-space offsets would move it off the paper.
        public void Offset(Vector3 offset) { }

        public void Fade(float amount)
        {
            for (var i = 0; i < _lines.Length; i++)
                if (_lines[i] != null) _lines[i].widthMultiplier = _widths[i] * amount;
        }

        public void Restore()
        {
            for (var i = 0; i < _trails.Length; i++) _trails[i].time = _times[i];
            foreach (var line in _lines) if (line != null) Object.Destroy(line.gameObject);
            if (_splatter != null) Object.Destroy(_splatter.gameObject);
        }

        private static Vector2 Detail(RabbitType type, int stroke, float t)
        {
            if (type == RabbitType.Vision)
            {
                if(stroke<2) return new Vector2(Mathf.Lerp(-0.46f,0.46f,t),
                    Mathf.Sin(t*Mathf.PI)*(stroke==0?0.31f:-0.29f));
                var angle=(stroke-2)*Mathf.PI*2f/14f;
                var radius=Mathf.Lerp(0.095f,0.205f,t);
                return new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
            }
            if(type==RabbitType.Hearing)
            {
                var side=stroke%2==0?-1f:1f;
                if(stroke<4){var y=Mathf.Lerp(-0.25f,0.32f,t);return new Vector2(side*(0.22f+(y+0.45f)*0.14f)+Mathf.Sin(t*Mathf.PI*1.5f)*0.025f,y);}
                var height=-0.3f+(stroke/2-2)*0.075f;
                return new Vector2(side*(0.22f+(height+0.45f)*0.14f)+Mathf.Lerp(-0.055f,0.045f,t),height+t*0.035f);
            }
            if(stroke<6)
            {
                var side=stroke%2==0?-1f:1f;var row=stroke/2;
                return new Vector2(side*Mathf.Lerp(0.12f,0.5f,t),-0.06f-row*0.06f+t*(0.13f-row*0.12f)+Mathf.Sin(t*Mathf.PI)*0.025f);
            }
            var x=-0.24f+(stroke-6)*0.05f;
            return new Vector2(x+t*0.025f,0.06f+Mathf.Lerp(-0.08f,0.04f,t)-Mathf.Abs(x)*0.2f);
        }

        private static Vector2 Shape(RabbitType type, int stroke, float t)
        {
            var angle = t * Mathf.PI * 2f;
            if (stroke >= 4) return Detail(type,stroke-4,t);
            switch (type)
            {
                case RabbitType.Hearing:
                    // Two long rabbit ears, each with an inner contour.
                    var side = stroke % 2 == 0 ? -1f : 1f;
                    var inner = stroke >= 2;
                    var height = inner ? 0.3f : 0.45f;
                    var y = -Mathf.Cos(angle) * height;
                    return new Vector2(side * (0.22f + (y + height) * 0.14f)
                        + Mathf.Sin(angle) * (inner ? 0.065f : 0.13f), y);
                case RabbitType.Smell:
                    if (stroke == 0)
                    {
                        // Soft triangular rabbit nose.
                        var segment = Mathf.Min(2, Mathf.FloorToInt(t * 3f));
                        var u = t * 3f - segment;
                        var a = segment == 0 ? new Vector2(-0.38f, 0.16f)
                            : segment == 1 ? new Vector2(0.38f, 0.16f) : new Vector2(0, -0.2f);
                        var b = segment == 0 ? new Vector2(0.38f, 0.16f)
                            : segment == 1 ? new Vector2(0, -0.2f) : new Vector2(-0.38f, 0.16f);
                        var control = segment == 0 ? new Vector2(0, 0.34f)
                            : segment == 1 ? new Vector2(0.24f, -0.16f) : new Vector2(-0.24f, -0.16f);
                        return (1f-u)*(1f-u)*a + 2f*(1f-u)*u*control + u*u*b;
                    }
                    if (stroke == 3) return new Vector2(0, Mathf.Lerp(-0.2f, -0.4f, t));
                    return new Vector2((stroke == 1 ? -0.19f : 0.19f) + Mathf.Cos(angle) * 0.055f,
                        0.07f + Mathf.Sin(angle) * 0.035f);
                default:
                    // Upper/lower eyelid, iris, pupil.
                    if (stroke < 2)
                        return new Vector2(Mathf.Lerp(-0.5f, 0.5f, t),
                            Mathf.Sin(t * Mathf.PI) * (stroke == 0 ? 0.25f : -0.25f));
                    var radius = stroke == 2 ? 0.22f : 0.085f;
                    return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
        }
    }
}
