using System;
using System.Text;
using UnityEngine;

namespace Common.Lab5_GOAP.Scripts
{
    public class GoapDebugHUD : MonoBehaviour
    {
        public GoapAgent agent;

        [Header("HUD")]
        public bool show = true;
        public Vector2 screenOffset = new Vector2(10, 10);

        private GUIStyle _style;

        private void Awake()
        {
        }

        private void OnGUI()
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                richText = true,
                border = new RectOffset(1, 1, 1, 1),
                
            };
            if (!show || agent == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("<b>GOAP Debug</b>");
            sb.AppendLine(agent.GetDebugString());
            
            GUI.Label(new Rect(screenOffset.x, screenOffset.y, Screen.width, Screen.height), sb.ToString(), _style);
        }
    }
    
}
