using UnityEditor;
using UnityEngine;

namespace PinePie.AnimPath.EditorTools
{

    [CustomEditor(typeof(MotionPath))]
    public class MotionPathEditor : Editor
    {
        MotionPath animPath;

        bool showDisplayOption = true;

        public override void OnInspectorGUI()
        {
            animPath = (MotionPath)target;

            EditorGUILayout.BeginHorizontal();
            animPath.clip = (AnimationClip)EditorGUILayout.ObjectField(new GUIContent("Animation Clip", "Put associated Animation clip here."), animPath.clip, typeof(AnimationClip), true);
            if (GUILayout.Button("Get"))
            {
                AnimationWindow animationWindow = EditorWindow.GetWindow<AnimationWindow>();
                AnimationClip selectedClip = animationWindow.animationClip;

                if (selectedClip != null) animPath.clip = selectedClip;
            }
            EditorGUILayout.EndHorizontal();

            if (animPath.clip != null)
            {
                animPath.alwaysShow = EditorGUILayout.Toggle(new GUIContent("Lock view", "Keep path showing even after deselecting."), animPath.alwaysShow);
                animPath.keepUpdating = EditorGUILayout.Toggle(new GUIContent("Keep Path Updating", "Keep path updating."), animPath.keepUpdating);

                if (GUILayout.Button("Update Path"))
                {
                    animPath.UpdateAnimationPath();
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Reset Path"))
                {
                    animPath.ResetMotionPath();
                    SceneView.RepaintAll();
                }

                EditorGUILayout.Space();

                showDisplayOption = EditorGUILayout.Foldout(showDisplayOption, "Display Options");
                if (showDisplayOption)
                {
                    if (animPath.clip != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Point Size");
                        animPath.pointSize = EditorGUILayout.Slider(animPath.pointSize, 0.03f, 0.2f);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        animPath.pathColor = EditorGUILayout.ColorField("Path Color", animPath.pathColor);
                        animPath.keyframePointColor = EditorGUILayout.ColorField("Keyframe Color", animPath.keyframePointColor);
                        animPath.FrameDotColor = EditorGUILayout.ColorField("Frame dot Color", animPath.FrameDotColor);

                        EditorGUILayout.Space();

                        animPath.ShowLines = EditorGUILayout.Toggle("Show Lines", animPath.ShowLines);
                        animPath.showKeyframePoints = EditorGUILayout.Toggle("Show Points", animPath.showKeyframePoints);
                        animPath.ShowFrameDots = EditorGUILayout.Toggle("Show Path Points", animPath.ShowFrameDots);

                    }
                }

                Repaint();
                SceneView.RepaintAll();

            }


            if (animPath.clip == null)
            {
                EditorGUILayout.LabelField("No Active Animation Clip.");
            }
        }

        private void OnSceneGUI()
        {
            animPath = (MotionPath)target;

            if (animPath.animationWindow == null) animPath.animationWindow = EditorWindow.GetWindow<AnimationWindow>();

            if (!animPath.keepUpdating && animPath.animationWindow.recording && animPath.lastTransformPos != animPath.transform.position)
                animPath.UpdateAnimationPath();

            if (animPath.keepUpdating && animPath.animationWindow.hasFocus)
                animPath.UpdateAnimationPath();

            animPath.DrawLines();
            animPath.DrawFrameDotPoints();
            animPath.DrawKeyframePoints();
        }


        public void OnEnable()
        {
            animPath = (MotionPath)target;

            animPath.deselected = false;
        }

        public void OnDisable()
        {
            animPath.deselected = true;
        }

    }
}