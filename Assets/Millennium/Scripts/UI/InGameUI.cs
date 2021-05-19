using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

namespace Millennium.UI
{
    public class InGameUI : MonoBehaviour
    {
        public static InGameUI I { get; private set; }

        private void Awake()
        {
            if (I != null)
            {
                Debug.LogError(nameof(InGameUI) + " ‚Íˆê‚Â‚¾‚¯”z’u‚·‚×‚«‚Å‚·");
                Destroy(this);
                return;
            }

            I = this;
        }


        [SerializeField]
        private HealthGauge m_BossHealthGauge;
        public HealthGauge BossHealthGauge => m_BossHealthGauge;

        [SerializeField]
        private TextMeshProUGUI m_ScoreText;
        public void SetScore(long value) => m_ScoreText.text = value.ToString();

        [SerializeField]
        private HealthGauge m_PlayerHealthGauge;
        public HealthGauge PlayerHealthGauge => m_PlayerHealthGauge;

        [SerializeField]
        private HealthGauge m_SkillGauge;
        public HealthGauge SkillGauge => m_SkillGauge;

        [SerializeField]
        private TextMeshProUGUI m_StageText;
        public void SetStage(string name) => m_StageText.text = name;


        [SerializeField]
        private GameObject[] m_ShowInConsoleOnly;
        [SerializeField]
        private GameObject[] m_ShowInMobileOnly;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool IsMobile();
#endif


        private void Start()
        {
            bool isMobile =
#if UNITY_WEBGL && !UNITY_EDITOR
                IsMobile();
#else
                false;
#endif

            foreach (var r in m_ShowInConsoleOnly)
                r.SetActive(!isMobile);
            foreach (var r in m_ShowInMobileOnly)
                r.SetActive(isMobile);
        }

    }
}
