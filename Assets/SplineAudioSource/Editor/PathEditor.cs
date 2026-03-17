using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace SplineAS
{

    [CustomEditor(typeof(SplineAudioSource))]
    public class PathEditor : Editor
    {
        SplineAudioSource splineAudioSource;
        int selectedHandleIndex = -1;
        float snapToGroundOffset = 1;

        bool closePath = false;
        
        const bool DEBUG_MODE = false;

        private bool _showAdvancedSettings = false;
        
        Path path
        {
            get
            {
                return splineAudioSource?.path;
            }
        }

        public override void OnInspectorGUI()
        {
            splineAudioSource = (SplineAudioSource)target;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            Color backgroundColor = new Color(0.2f, 0.5f, 0.8f);
            Rect titleRect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(titleRect, backgroundColor);
            EditorGUI.LabelField(titleRect, "Spline Audio Source Editor", titleStyle);
            
            #region Issues and Warnings
            if (splineAudioSource.GetComponentInChildren<AudioSource>() == null)
            {
                EditorGUILayout.HelpBox("No children detected.\nPress button for solve", MessageType.Error);
                if (GUILayout.Button("Solve Issue"))
                {
                    if(splineAudioSource.GetComponent<AudioSource>())
                        Destroy(splineAudioSource.GetComponent<AudioSource>());
                    
                    GameObject childGameObject = new GameObject("Audio Source (Spline Audio Source)");
                    childGameObject.transform.parent = splineAudioSource.transform;
                    
                    childGameObject.AddComponent<AudioSource>();
                    childGameObject.GetComponent<AudioSource>().loop = true;
                    childGameObject.GetComponent<AudioSource>().spatialBlend = 1f;
                    childGameObject.GetComponent<AudioSource>().minDistance = 5f;
                    childGameObject.GetComponent<AudioSource>().maxDistance = 100f;
                    childGameObject.GetComponent<AudioSource>().dopplerLevel = .5f;
                }

                return;
            }
            
            if (splineAudioSource.GetComponentInChildren<AudioSource>().clip == null)
            {
                AudioSource audioSourceTarget = splineAudioSource.GetComponentInChildren<AudioSource>();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Audioclip missing on AudioSource", MessageType.Error);
                GUILayout.Space(5);

                GUI.backgroundColor = Color.red;
                audioSourceTarget.clip = (AudioClip)EditorGUILayout.ObjectField("Assign Audio Clip", audioSourceTarget.clip, typeof(AudioClip), false);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
            }
            #endregion
            
            
            DrawDefaultInspector();

            if (splineAudioSource.GetComponentInChildren<AudioSource>().clip != null)
            {
                AudioSource audioSourceTarget = splineAudioSource.GetComponentInChildren<AudioSource>();
                audioSourceTarget.clip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", audioSourceTarget.clip, typeof(AudioClip), false);
            }
            
            if (splineAudioSource.target == null)
            {
                EditorGUILayout.HelpBox("Target is not assigned.", MessageType.Error);
                GUI.backgroundColor = Color.red;
            }
            splineAudioSource.target = (Transform)EditorGUILayout.ObjectField("Target", splineAudioSource.target, typeof(Transform), true);
            GUI.backgroundColor = Color.white;

            if (path == null || path.GetPathPoints() == null)
            {
                splineAudioSource.CreatePath(splineAudioSource.transform.position + Vector3.forward);
            }

            /*
             * Settings
             */
            GUILayout.Label("Settings");

            splineAudioSource.closePath = GUILayout.Toggle(splineAudioSource.closePath, "Close Path");
            closePath = splineAudioSource.closePath;
            
            // When path is not closed, followTargetInside is disabled.
            if (splineAudioSource.closePath == false)
            {
                GUI.enabled = false;
                splineAudioSource.followTargetInside = false;
            }
            splineAudioSource.followTargetInside = GUILayout.Toggle(splineAudioSource.followTargetInside, "Follow Target Inside Shape (XZ)");
            GUI.enabled = true;

            
            if (closePath && path.GetPathPoints().Length < 3)
                EditorGUILayout.HelpBox("Close path only work with 3 or more points", MessageType.Warning);

            /*
             * Control Buttons
             */

            if (path == null || path.GetPathPoints() == null)
            {
                EditorGUILayout.HelpBox("Restart needed. An error has occurred.", MessageType.Error);
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("(+) Add new Point"))
            {
                if (path.GetPathPoints().Length % 2 == 0)
                {
                    AddNewPoint();
                }
                else
                {
                    AddNewPoint();
                    AddNewPoint();
                }
            }

            if(selectedHandleIndex < 0)
                GUI.enabled = false;
            if (GUILayout.Button("(-) Remove Selected Point"))
            {
                if (selectedHandleIndex >= 0)
                {
                    splineAudioSource.path.RemovePointAtIndex(selectedHandleIndex);
                }
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            
            if (GUILayout.Button("Reset Path"))
            {
                splineAudioSource.CreatePath(Vector3.zero);
            }
            
            GUILayout.Space(10);

            GUILayout.Label("Snap Settings");
            splineAudioSource.snapOffset = EditorGUILayout.FloatField("Snap Offset", splineAudioSource.snapOffset);

            if (GUILayout.Button("Snap path to ground"))
            {
                Undo.RecordObject(splineAudioSource, "Snap points to ground");
                SnapPointsToGround(snapToGroundOffset);
                EditorUtility.SetDirty(splineAudioSource);
            }

            EditorGUILayout.HelpBox("" +
            "Create new Point: (Shift + Click)\n" +
            "Delete selected Point (Del)",
            MessageType.Info);
            _showAdvancedSettings = GUILayout.Toggle(_showAdvancedSettings, "Show Advanced Settings");
            if (_showAdvancedSettings)
            {
                splineAudioSource.lineThickness = EditorGUILayout.FloatField("Line Thickness", splineAudioSource.lineThickness);
                splineAudioSource.handleSize = EditorGUILayout.FloatField("Handle Size", splineAudioSource.handleSize);
                GUILayout.Label("Line Color");
                splineAudioSource.lineColor = EditorGUILayout.ColorField(splineAudioSource.lineColor);
                GUILayout.Label("Handle Color");
                splineAudioSource.handleColor = EditorGUILayout.ColorField(splineAudioSource.handleColor);
                GUILayout.Label("Control Handle Color");
                splineAudioSource.controlColor = EditorGUILayout.ColorField(splineAudioSource.controlColor);
                GUILayout.Label("Selected Handle Color");
                splineAudioSource.selectedHandleColor = EditorGUILayout.ColorField(splineAudioSource.selectedHandleColor);        
                EditorUtility.SetDirty(splineAudioSource);

                if(!splineAudioSource.useWorldPosition)
                    GUI.enabled = false;
                if (GUILayout.Button("Convert spline from World position to Local position"))
                {
                    Vector3[] pathPoints = splineAudioSource.path.GetPathPoints();
                    for (int i = 0; i < pathPoints.Length; i++)
                    {
                        pathPoints[i] -= splineAudioSource.transform.position;
                    }
                    splineAudioSource.path.SetPathPoints(pathPoints);
                    
                    splineAudioSource.useWorldPosition = true;
                    EditorUtility.SetDirty(splineAudioSource);
                }
                GUI.enabled = true;
                
                if(splineAudioSource.useWorldPosition)
                    GUI.enabled = false;
                if (GUILayout.Button("Convert spline from Local position to World position"))
                {
                    Vector3[] pathPoints = splineAudioSource.path.GetPathPoints();
                    for (int i = 0; i < pathPoints.Length; i++)
                    {
                        pathPoints[i] += splineAudioSource.transform.position;
                    }
                    splineAudioSource.path.SetPathPoints(pathPoints);

                    splineAudioSource.useWorldPosition = false;
                    EditorUtility.SetDirty(splineAudioSource);
                }
                GUI.enabled = true;

            }
            
            serializedObject.Update();
        }

        public void OnSceneGUI()
        {
            if (splineAudioSource == null || path == null || path.GetPathPoints() == null)
                return;

            if (Selection.objects.Length > 1)
            {
                EditorGUILayout.HelpBox("Multiple object path selection not supported.", MessageType.Warning);
                return;
            }

            try
            {
                Draw();
                Input();
            }
            catch (System.Exception error)
            {
                Debug.LogWarning("[SplineAudioSource] - " + error);
                return;
            }

            serializedObject.ApplyModifiedProperties();
        }

        void Draw()
        {
            Vector3[] points = path.GetPathPoints();
            if (!splineAudioSource.useWorldPosition)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 scale = splineAudioSource.transform.localScale;

                    points[i] = (splineAudioSource.transform.right * (points[i].x * scale.x)) +
                                (splineAudioSource.transform.up * (points[i].y * scale.y)) +
                                (splineAudioSource.transform.forward * (points[i].z * scale.z));

                    points[i] += splineAudioSource.transform.position;
                }
            }

            if (points == null || points.Length < 2)
                return;

            for (int i = 0; i < points.Length; i++)
            {
                Handles.color = i == selectedHandleIndex ? splineAudioSource.selectedHandleColor : splineAudioSource.handleColor;
                
                if (splineAudioSource.splineType == SplineType.Quadratic)
                {
                    if (i % 2 == 1)
                    {
                        Handles.color = splineAudioSource.controlColor;
                    }
                }
                
                if (Handles.Button(points[i], Quaternion.identity, splineAudioSource.handleSize, 0.4f, (i % 2 == 1 && splineAudioSource.splineType == SplineType.Quadratic) ? Handles.SphereHandleCap : Handles.CubeHandleCap))
                {
                    selectedHandleIndex = i;
                }

                if (Physics.Raycast(points[i], Vector3.down, out RaycastHit hit, 50))
                {
                    Handles.DrawWireDisc(hit.point, Vector3.up, 0.25f);
                    Handles.DrawLine(points[i], hit.point,splineAudioSource.lineThickness);
                }
            }
            
            Handles.color = splineAudioSource.lineColor;
            
            switch (splineAudioSource.splineType)
            {
                case SplineType.Quadratic:

                    for (int i = 0; i < points.Length - 1; i++)
                    {
                        Vector3 p0 = points[i];

                        Vector3 p1 = (i + 1 < points.Length) ? points[i + 1] : points[i];
                        Vector3 p2 = (i + 2 < points.Length) ? points[i + 2] : p1;

                        if (i % 2 == 0)
                        {
                            if (i + 1 < points.Length)
                            {
                                DrawQuadraticBezier(p0, p1, p2);
                            }
                            else
                            {
                                Handles.DrawLine(p0, p1,splineAudioSource.lineThickness);
                            }
                        }
                        else
                        {

                        }
                    }

                    break;
                case SplineType.Line:
                    for (int i = 0; i < points.Length; i++)
                    {
                        if (i < points.Length - 1)
                        {
                            Handles.DrawLine(points[i], points[i + 1],splineAudioSource.lineThickness);
                        }
                    }
                    break;
            }

            if (points.Length > 2 && closePath)
            {
                Handles.DrawLine(points[0], points[points.Length - 1],splineAudioSource.lineThickness);
            }
        }

    void Input()
    {
        Vector3[] points = path.GetPathPoints();

        if (!splineAudioSource.useWorldPosition)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 scale = splineAudioSource.transform.localScale;
                points[i] = (points[i].x * splineAudioSource.transform.right * scale.x) +
                            (points[i].y * splineAudioSource.transform.up * scale.y) +
                            (points[i].z * splineAudioSource.transform.forward * scale.z);
                points[i] += splineAudioSource.transform.position;            
            }
        }

        if (points == null || selectedHandleIndex >= points.Length || selectedHandleIndex < 0)
        {
            selectedHandleIndex = -1;
            return;
        }

        if (selectedHandleIndex >= 0)
        {
            EditorGUI.BeginChangeCheck();

            Event guiEvent = Event.current;

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                if (path.GetPathPoints().Length % 2 == 0 || splineAudioSource.splineType == SplineType.Line)
                {
                    AddNewPoint();
                }
                else
                {
                    AddNewPoint();
                    AddNewPoint();
                }
                guiEvent.Use();
                return;
            }

            if (guiEvent.type == EventType.KeyDown && guiEvent.keyCode == KeyCode.Delete && selectedHandleIndex >= 0)
            {
                Undo.RecordObject(splineAudioSource, "Remove Point");
                splineAudioSource.path.RemovePointAtIndex(selectedHandleIndex);
                EditorUtility.SetDirty(splineAudioSource);

                selectedHandleIndex -= 1;

                guiEvent.Use();
            }

            Vector3 newPosition = Handles.PositionHandle(points[selectedHandleIndex], Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                points[selectedHandleIndex] = newPosition;

                if (!splineAudioSource.useWorldPosition)
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] -= splineAudioSource.transform.position;

                        Vector3 scale = splineAudioSource.transform.localScale;
                        points[i] = new Vector3(
                            Vector3.Dot(points[i], splineAudioSource.transform.right) / scale.x,
                            Vector3.Dot(points[i], splineAudioSource.transform.up) / scale.y,
                            Vector3.Dot(points[i], splineAudioSource.transform.forward) / scale.z
                        );
                    }

                }
                
                Undo.RecordObject(splineAudioSource, "Move Point");
                splineAudioSource.path.SetPathPoints(points);
                EditorUtility.SetDirty(splineAudioSource);
            }
        }
    }

        void DrawQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            int steps = 10;
            for (int j = 0; j < steps; j++)
            {
                float t1 = (float)j / steps;
                float t2 = (float)(j + 1) / steps;

                Vector3 point1 = CalculateQuadraticBezierPoint(t1, p0, p1, p2);
                Vector3 point2 = CalculateQuadraticBezierPoint(t2, p0, p1, p2);

                Handles.DrawLine(point1, point2, splineAudioSource.lineThickness);
            }
        }

        Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float oneMinusT = 1 - t;
            return oneMinusT * oneMinusT * p0 + 2 * oneMinusT * t * p1 + t * t * p2;
        }

        void SnapPointsToGround(float offset)
        {
            Vector3[] points = splineAudioSource.path.GetPathPoints();
            for (int i = 0; i < points.Length; i++)
            {
                if (Physics.Raycast(points[i], Vector3.down, out RaycastHit hit, 100))
                {
                    points[i] = hit.point + (Vector3.up * offset);
                }
            }

            splineAudioSource.path.SetPathPoints(points);
        }

        void AddNewPoint()
        {
            Vector3[] paths = path.GetPathPoints()?.ToArray();

            if (paths == null || paths.Length == 0)
                return;

            // Registra el estado antes de realizar cambios
            Undo.RecordObject(splineAudioSource, "Add Point");

            if (selectedHandleIndex == 0)
            {
                Vector3 newPos = paths[selectedHandleIndex] - paths[selectedHandleIndex + 1];
                newPos = newPos.normalized;
                newPos *= 5;
                newPos += paths[selectedHandleIndex];

                splineAudioSource.path.AddNewPointAtIndex(0, newPos);

                selectedHandleIndex = 0;
                Debug.Log("selectedHandleIndex == 0");
            }
            else if (selectedHandleIndex == paths.Length - 1)
            {
                Vector3 newPos = paths[selectedHandleIndex] - paths[selectedHandleIndex - 1];
                newPos = newPos.normalized;
                newPos *= 5;
                newPos += paths[selectedHandleIndex];

                splineAudioSource.path.AddNewPointAtIndex(paths.Length, newPos);

                selectedHandleIndex = paths.Length;
                Debug.Log("selectedHandleIndex == paths.Length - 1");
            }
            else
            {
                Vector3 newPos = paths[selectedHandleIndex - 1] - paths[selectedHandleIndex];
                newPos = newPos.normalized;
                newPos *= 5;
                newPos += paths[selectedHandleIndex];

                splineAudioSource.path.AddNewPointAtIndex(selectedHandleIndex, newPos);
                Debug.Log("else");
            }

            // Marca el objeto como sucio (necesita guardarse)
            EditorUtility.SetDirty(splineAudioSource);

            Debug.Log($"Current: {selectedHandleIndex} - Max: {paths.Length - 1}");
        }

    }
}
