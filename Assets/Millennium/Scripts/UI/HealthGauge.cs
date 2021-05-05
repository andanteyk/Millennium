using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Millennium.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HealthGauge : MonoBehaviour
    {
        [SerializeField]
        private Image m_Gauge;

        [SerializeField]
        private Image m_SubGauge;


        private CanvasGroup m_CanvasGroup;

        private float m_GaugeWidth;
        private float m_SubGaugeWidth;

        private void Start()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();

            m_GaugeWidth = m_Gauge.rectTransform.sizeDelta.x;
            m_SubGaugeWidth = m_SubGauge.rectTransform.sizeDelta.x;

            Hide();
            SetGaugeColor(Color.red);
        }

        public void SetGauge(int current, int max)
        {
            m_Gauge.rectTransform.sizeDelta = new Vector2(
                m_GaugeWidth * current / max,
                m_Gauge.rectTransform.sizeDelta.y);
        }

        public void SetSubGauge(int value)
        {
            m_SubGauge.rectTransform.sizeDelta = new Vector2(
                m_SubGaugeWidth * value,
                m_SubGauge.rectTransform.sizeDelta.y);
        }

        public void DecrementSubGauge()
        {
            m_SubGauge.rectTransform.sizeDelta = new Vector2(
                m_SubGauge.rectTransform.sizeDelta.x - m_SubGaugeWidth,
                m_SubGauge.rectTransform.sizeDelta.y);
        }


        public void SetGaugeColor(Color color)
        {
            m_Gauge.color = color;
        }


        public void Show()
        {
            m_CanvasGroup.alpha = 1;
        }

        public void Hide()
        {
            m_CanvasGroup.alpha = 0;
        }
    }
}
