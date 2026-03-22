using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace PinePie.AnimPath
{
    [AddComponentMenu("PinePie/AnimPath")]
    [System.Serializable]
    public class MotionPath : MonoBehaviour
    {
        public AnimationClip clip;
        public Animator animator;
        public AnimationWindow animationWindow;

        public Color pathColor = Color.yellow; // Color of the path

        public Color keyframePointColor = Color.white; // Color of the path
        public Color FrameDotColor = Color.yellow; // Color of the path

        public float pointSize = 0.03f;

        public bool showKeyframePoints = true;
        public bool ShowLines = true;
        public bool ShowFrameDots = false;



        public List<Vector3> points = new();
        public List<Vector3> valueEachTime = new();
        public AnimationCurve xCurve = new();
        public AnimationCurve yCurve = new();
        public AnimationCurve zCurve = new();


        public bool deselected = false;
        public bool alwaysShow = false;
        public bool keepUpdating = false;
        public Vector3 lastTransformPos;
        [HideInInspector] public bool initialized;


        void OnValidate()
        {
            if (!initialized)
            {
                AnimationWindow animationWindow = EditorWindow.GetWindow<AnimationWindow>();
                clip = animationWindow.animationClip;

                UpdateAnimationPath();

                initialized = true;
            }
        }

        public void OnDrawGizmos()
        {
            if (alwaysShow && deselected)
            {
                if (animationWindow == null) animationWindow = EditorWindow.GetWindow<AnimationWindow>();

                if (keepUpdating && animationWindow.hasFocus) UpdateAnimationPath();

                DrawLines();
                DrawFrameDotPoints();
                DrawKeyframePoints();
            }

        }

        // updating motion paths
        public void UpdateAnimationPath()
        {
            if (clip != null)
            {
                if (animator == null) animator = transform.GetComponent<Animator>();

                // get curves
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                {
                    if ((animator != null) ? binding.path == "" : binding.path.EndsWith(transform.name))
                    {

                        if (binding.propertyName == "m_LocalPosition.x")
                        {
                            xCurve = AnimationUtility.GetEditorCurve(clip, binding);
                        }
                        if (binding.propertyName == "m_LocalPosition.y")
                        {
                            yCurve = AnimationUtility.GetEditorCurve(clip, binding);
                        }
                        if (binding.propertyName == "m_LocalPosition.z")
                        {
                            zCurve = AnimationUtility.GetEditorCurve(clip, binding);
                        }
                    }
                }

                // Sample points every second
                int sampleCount = Mathf.FloorToInt(clip.length * 60f) + 1;
                Vector3[] positionSamples = new Vector3[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    float time = i * (1 / 60f);
                    float myxValue = xCurve != null ? xCurve.Evaluate(time) : 0f;
                    float myyValue = yCurve != null ? yCurve.Evaluate(time) : 0f;
                    float myzValue = zCurve != null ? zCurve.Evaluate(time) : 0f;

                    positionSamples[i] = new Vector3(myxValue, myyValue, myzValue);

                }
                valueEachTime = positionSamples.ToList();



                if (xCurve.keys.Length > 0 && yCurve.keys.Length > 0 && zCurve.keys.Length > 0)
                {
                    SortedSet<float> keyTimes = new();

                    // Add keyframe times from both curves
                    foreach (var keyframe in xCurve.keys)
                    {
                        keyTimes.Add(keyframe.time);
                    }
                    foreach (var keyframe in yCurve.keys)
                    {
                        keyTimes.Add(keyframe.time);
                    }
                    foreach (var keyframe in zCurve.keys)
                    {
                        keyTimes.Add(keyframe.time);
                    }

                    List<Vector3> pointArr = new();

                    // Iterate through all the unique keyframe times in order
                    foreach (float time in keyTimes)
                    {
                        float xValue = GetValueFromCurveOrPosition(xCurve, time, transform, "x");

                        float yValue = GetValueFromCurveOrPosition(yCurve, time, transform, "y");

                        float zValue = GetValueFromCurveOrPosition(zCurve, time, transform, "z");

                        Vector3 pos = new(xValue, yValue, zValue);

                        Transform currentTransform = transform;
                        // find the topmost parent (root)
                        while (currentTransform.parent != null)
                        {
                            if (animator != null)
                            {
                                break;
                            }
                            else
                            {
                                currentTransform = currentTransform.parent;
                            }
                        }

                        // someone other is parent
                        if (currentTransform != transform)
                        {
                            pos = currentTransform.TransformPoint(pos); // local scaling
                        }

                        // Add the resulting Vector2 to the list
                        pointArr.Add(pos);
                    }

                    points = pointArr;
                }

                if (xCurve.keys.Length < 1 && yCurve.keys.Length < 1)
                {
                    ResetMotionPath();
                }

                SceneView.RepaintAll();

            }

        }

        private float GetValueFromCurveOrPosition(AnimationCurve curve, float time, Transform target, string axis)
        {
            // Check if the curve has keyframes and the time is within range
            if (curve.length > 0 && curve.keys[0].time <= time && curve.keys[curve.length - 1].time >= time)
            {
                // Optional: Add actions you want to perform when the curve is evaluated
                return curve.Evaluate(time);  // Return the evaluated curve value
            }
            else
            {
                // Optional: Add actions you want to perform when the curve is not used

                if (axis == "x")
                {
                    return target.position.x;
                }
                else if (axis == "y")
                {
                    return target.position.x;
                }
                else if (axis == "z")
                {
                    return target.position.x;
                }
                else
                {
                    return 0;
                }
            }
        }


        public void DrawLines()
        {
            if (clip != null)
            {
                if (valueEachTime.Count < 2) return;

                if (ShowLines)
                {
                    Handles.color = pathColor;

                    // Draw the path by connecting the points
                    for (int i = 0; i < valueEachTime.Count - 1; i++)
                    {
                        Handles.DrawLine(valueEachTime[i], valueEachTime[i + 1]);
                    }
                }
            }
        }


        public void DrawKeyframePoints()
        {
            if (!showKeyframePoints) return;

            DrawPoints(points, keyframePointColor, pointSize);
        }

        public void DrawFrameDotPoints()
        {
            if (!ShowFrameDots) return;

            DrawPoints(valueEachTime, FrameDotColor, pointSize * 0.7f);
        }

        void DrawPoints(List<Vector3> pts, Color color, float size)
        {
            if (clip == null || pts.Count == 0) return;

            Handles.color = color;

            bool is2D = SceneView.currentDrawingSceneView != null &&
                        SceneView.currentDrawingSceneView.in2DMode;

            float baseSize = is2D ? HandleUtility.GetHandleSize(Vector3.zero) : 1f;

            foreach (var p in pts)
                Handles.DrawSolidDisc(p, Vector3.forward, baseSize * size);
        }


        public void ResetMotionPath()
        {
            // check if, is there points
            if (points != null)
            {
                // Clear the points lists 
                points.Clear();
                xCurve = new AnimationCurve();
                yCurve = new AnimationCurve();
                SceneView.RepaintAll();

            }
        }

    }


}

#endif
