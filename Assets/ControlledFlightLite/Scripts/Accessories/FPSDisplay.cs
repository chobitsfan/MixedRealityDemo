using UnityEngine;
using System.Text;

namespace SparseDesign
{
    public class FPSDisplay : MonoBehaviour
    {
        float deltaTime = 0.0f;
        float fpsFiltered = -1;

        [SerializeField] bool m_debug = true;
        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            if (m_debug || (GameSettings.GetInstance() && GameSettings.GetInstance().m_allDebug))
            {
                int w = Screen.width, h = Screen.height;

                GUIStyle style = new GUIStyle();

                Rect rect = new Rect(0, 0, w, h * 2 / 100);
                style.alignment = TextAnchor.UpperLeft;
                style.fontSize = h * 2 / 100;
                style.normal.textColor = Color.white;//new Color(0.0f, 0.0f, 0.5f, 1.0f);
                float msec = deltaTime * 1000.0f;
                float fps = 1.0f / deltaTime;

                if (Time.time < 0.5)
                {
                    fpsFiltered = fps;
                }

                fpsFiltered = Mathf.Lerp(fpsFiltered, fps, 1f / 2.0f * deltaTime);//deltaTime is the wrong thing here...

                StringBuilder builder = new StringBuilder(64);
                builder.AppendFormat("Time: {0:0.00}\n", Time.time);
                builder.AppendFormat("{0:0.0} ms ({1:0.} fps", msec, fps);
                builder.AppendFormat(", {0:0.})", fpsFiltered);

                GUI.Label(rect, builder.ToString(), style);
            }
        }
    }
}