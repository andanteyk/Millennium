using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Millennium.InGame.Effect
{
    public class Balloon : MonoBehaviour
    {
        [SerializeField]
        private TextMeshPro m_Text;
        public string Text { get => m_Text.text; set => m_Text.text = value; }


        [SerializeField]
        private bool m_ShowsName = true;
        public bool ShowsName
        {
            get => m_ShowsName;
            set
            {
                m_ShowsName = value;

                m_NameArea.SetActive(value);
            }
        }

        [SerializeField]
        private GameObject m_NameArea;

        [SerializeField]
        private TextMeshPro m_NameText;
        public string NameText { get => m_NameText.text; set => m_NameText.text = value; }



        [SerializeField]
        private float m_DestroySeconds = 3;
        public float DestroySeconds { get => m_DestroySeconds; set => m_DestroySeconds = value; }

        [SerializeField]
        private bool m_DestroyByKey = false;
        public bool DestroyByKey { get => m_DestroyByKey; set => m_DestroyByKey = value; }


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            ShowsName = m_ShowsName;

            if (m_DestroySeconds > 0)
            {
                UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(m_DestroySeconds))
                    .ForEachAsync(_ => Destroy(gameObject), token);
            }

            if (m_DestroyByKey)
            {
                var input = new InputControls();
                input.Enable();

                UniTask.Create(async () =>
                {
                    await UniTask.WaitUntil(() => input.Player.Fire.WasPressedThisFrame(), cancellationToken: token);
                    token.ThrowIfCancellationRequested();
                    Destroy(gameObject);
                });
            }
        }


        public UniTask WaitUntilDestroy(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return this.OnDestroyAsync().AttachExternalCancellation(token);
        }
    }
}
