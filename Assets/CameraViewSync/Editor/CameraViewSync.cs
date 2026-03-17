using UnityEditor;
using UnityEngine;
using System;

namespace ninjaqwert.UTCamSync
{
    [InitializeOnLoad]
    public class CameraViewSync
    {
        static Camera pilotedCamera;
        static bool isPiloting = false;
        static Vector3 lastSceneViewPosition;
        static Quaternion lastSceneViewRotation;
        static bool hasInitializedLastTransform = false;

        // EditorPrefs keys for persistence
        const string PILOTING_STATE_KEY = "CamViewSync_IsPiloting";
        const string PILOTED_CAMERA_ID_KEY = "CamViewSync_CameraInstanceID";

        static CameraViewSync()
        {
            // Hook into the Scene view update loop
            SceneView.duringSceneGui += OnSceneGUI;

            // Handle domain reload - restore piloting state
            EditorApplication.delayCall += RestorePilotingState;

            // Clean up on domain unload
            AssemblyReloadEvents.beforeAssemblyReload += SavePilotingState;
        }

        static void RestorePilotingState()
        {
            if (EditorPrefs.GetBool(PILOTING_STATE_KEY, false))
            {
                int cameraInstanceID = EditorPrefs.GetInt(PILOTED_CAMERA_ID_KEY, 0);
                if (cameraInstanceID != 0)
                {
                    UnityEngine.Object cameraObj = EditorUtility.InstanceIDToObject(cameraInstanceID);
                    if (cameraObj is Camera camera)
                    {
                        pilotedCamera = camera;
                        isPiloting = true;
                        Debug.Log($"Restored piloting state for: {pilotedCamera.name}");
                    }
                    else
                    {
                        // Camera no longer exists, clear the state
                        ClearPilotingState();
                    }
                }
            }
        }

        static void SavePilotingState()
        {
            if (isPiloting && pilotedCamera != null)
            {
                EditorPrefs.SetBool(PILOTING_STATE_KEY, true);
                EditorPrefs.SetInt(PILOTED_CAMERA_ID_KEY, pilotedCamera.GetInstanceID());
            }
            else
            {
                ClearPilotingState();
            }
        }

        static void ClearPilotingState()
        {
            EditorPrefs.DeleteKey(PILOTING_STATE_KEY);
            EditorPrefs.DeleteKey(PILOTED_CAMERA_ID_KEY);
        }

        [MenuItem("Tools/Camera View Sync/Start Piloting")]
        static void StartPiloting()
        {
            GameObject selectedObj = Selection.activeGameObject;
            if (selectedObj != null && selectedObj.TryGetComponent<Camera>(out Camera camera))
            {
                pilotedCamera = camera;
                isPiloting = true;
                hasInitializedLastTransform = false; // Reset to capture initial state
                Debug.Log($"Started piloting: {pilotedCamera.name}");

                // Force Scene View to repaint to show the piloting label
                SceneView.RepaintAll();
            }
            else
            {
                Debug.LogWarning("Please select a GameObject with a Camera component to pilot.");
            }
        }

        [MenuItem("Tools/Camera View Sync/Stop Piloting")]
        static void StopPiloting()
        {
            if (isPiloting)
            {
                string cameraName = pilotedCamera != null ? pilotedCamera.name : "Unknown";
                Debug.Log($"Stopped piloting: {cameraName}");
            }

            isPiloting = false;
            pilotedCamera = null;
            hasInitializedLastTransform = false;
            ClearPilotingState();

            // Force Scene View to repaint to hide the piloting label
            SceneView.RepaintAll();
        }

        // Validate menu items
        [MenuItem("Tools/Camera View Sync/Start Piloting", true)]
        static bool ValidateStartPiloting()
        {
            return !isPiloting && Selection.activeGameObject != null &&
                   Selection.activeGameObject.GetComponent<Camera>() != null;
        }

        [MenuItem("Tools/Camera View Sync/Stop Piloting", true)]
        static bool ValidateStopPiloting()
        {
            return isPiloting;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (!isPiloting || pilotedCamera == null)
                return;

            // Check if the piloted camera still exists
            if (pilotedCamera == null)
            {
                Debug.LogWarning("Piloted camera was destroyed. Stopping piloting.");
                StopPiloting();
                return;
            }

            Transform sceneCam = sceneView.camera.transform;

            // Initialize last transform on first frame
            if (!hasInitializedLastTransform)
            {
                lastSceneViewPosition = sceneCam.position;
                lastSceneViewRotation = sceneCam.rotation;
                hasInitializedLastTransform = true;
                return; // Skip first frame to avoid initial jump
            }

            // Only update if the Scene View camera has actually moved
            bool positionChanged = Vector3.Distance(lastSceneViewPosition, sceneCam.position) > 0.001f;
            bool rotationChanged = Quaternion.Angle(lastSceneViewRotation, sceneCam.rotation) > 0.1f;

            if (positionChanged || rotationChanged)
            {
                // Record the change for undo
                Undo.RecordObject(pilotedCamera.transform, "Camera Pilot Movement");

                // Copy Scene View camera transform to the piloted camera
                pilotedCamera.transform.position = sceneCam.position;
                pilotedCamera.transform.rotation = sceneCam.rotation;

                // Update last known transform
                lastSceneViewPosition = sceneCam.position;
                lastSceneViewRotation = sceneCam.rotation;

                // Mark the scene as dirty to ensure changes are saved
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(pilotedCamera);
                }
            }

            // Draw piloting indicator
            Handles.BeginGUI();
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.green }
            };
            GUI.Label(new Rect(10, 10, 250, 25), $"🎥 Piloting: {pilotedCamera.name}", labelStyle);
            Handles.EndGUI();
        }

        public static bool IsPiloting => isPiloting;

        public static void StartPilotingFromWindow() => StartPiloting();
        public static void StopPilotingFromWindow() => StopPiloting();
    }
}