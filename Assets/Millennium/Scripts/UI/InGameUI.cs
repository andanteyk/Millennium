using System.Collections;
using System.Collections.Generic;
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
                Debug.LogError(nameof(InGameUI) + " は一つだけ配置すべきです");
                Destroy(this);
                return;
            }

            I = this;
        }


        [SerializeField]
        private HealthGauge m_BossHealthGauge;
        public HealthGauge BossHealthGauge => m_BossHealthGauge;

    }
}
