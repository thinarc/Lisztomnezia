using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace ninjaqwert.UTCamSync
{
    [Overlay(typeof(SceneView), "CVS")]
    [Icon("Assets/CameraViewSync/favicon.png")]
    public class CameraViewSyncOverlay : Overlay
    {
        private Button toggleButton;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    paddingLeft = 4,
                    paddingRight = 4
                }
            };

            toggleButton = new Button(OnTogglePiloting)
            {
                style =
                {
                    width = 28,
                    height = 28,
                    marginRight = 4,
                    backgroundImage = GetIcon(),
                    backgroundColor = Color.clear
                },
                tooltip = GetTooltip()
            };

            root.Add(toggleButton);
            return root;
        }

        void OnTogglePiloting()
        {
            if (CameraViewSync.IsPiloting)
                CameraViewSync.StopPilotingFromWindow();
            else
                CameraViewSync.StartPilotingFromWindow();

            // Update existing button's look, change icon, etc
            toggleButton.style.backgroundImage = GetIcon();
            toggleButton.tooltip = GetTooltip();
        }

        Texture2D GetIcon()
        {
            string iconName = CameraViewSync.IsPiloting ? "d_PreMatQuad" : "d_SceneViewCamera";
            return EditorGUIUtility.IconContent(iconName).image as Texture2D;
        }

        string GetTooltip()
        {
            return CameraViewSync.IsPiloting ? "Stop Piloting" : "Start Piloting";
        }
    }
}
