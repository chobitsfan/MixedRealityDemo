using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparseDesign
{
    public class GameSettings : MonoBehaviour
    {
        [Range(-1, 144)] [SerializeField] int m_targetFramerate = -1;
        [SerializeField] public Color m_backGroundColor = Color.black;
        [Tooltip("Activates all debug displays")] [SerializeField] public bool m_allDebug = false;

        static GameSettings instance = null;
        static public GameSettings GetInstance() { return instance; }
        private void Awake()
        {
            GameSettings.instance = this;
        }

        void Start()
        {
            Application.targetFrameRate = m_targetFramerate;
        }

    }
}