using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Millennium.UI
{
    public class Fader : MonoBehaviour
    {
        [SerializeField]
        private Image m_Image;

        [SerializeField]
        private Sprite[] m_Sprites;

        [SerializeField]
        private float m_Interval = 0.1f;


        private CancellationTokenSource m_CancellationTokenSource = null;



        public async UniTask Show()
        {
            Cancel();

            m_CancellationTokenSource = new CancellationTokenSource();

            var token = m_CancellationTokenSource.Token;

            m_Image.enabled = true;
            await m_Sprites.ToUniTaskAsyncEnumerable()
                .ForEachAwaitAsync(async sprite =>
            {
                m_Image.sprite = sprite;
                await UniTask.Delay(TimeSpan.FromSeconds(m_Interval), ignoreTimeScale: true, cancellationToken: token);
            }, token);
        }

        public async UniTask Hide()
        {
            Cancel();

            m_CancellationTokenSource = new CancellationTokenSource();

            var token = m_CancellationTokenSource.Token;

            await m_Sprites.Reverse().ToUniTaskAsyncEnumerable()
                .ForEachAwaitAsync(async sprite =>
                {
                    m_Image.sprite = sprite;
                    await UniTask.Delay(TimeSpan.FromSeconds(m_Interval), ignoreTimeScale: true, cancellationToken: token);
                }, token);
            m_Image.enabled = false;
        }

        public void ShowImmediately()
        {
            Cancel();

            m_Image.enabled = true;
            m_Image.sprite = m_Sprites[m_Sprites.Length - 1];
        }

        public void HideImmediately()
        {
            Cancel();

            m_Image.enabled = false;
        }


        private void Cancel()
        {
            m_CancellationTokenSource?.Cancel();
            m_CancellationTokenSource?.Dispose();
        }


        private void OnDestroy()
        {
            Cancel();
        }



        // TODO: ìKêÿÇ≈Ç»Ç¢ÇÃÇ≈ÇÕ
        public static async UniTask<Fader> CreateFade()
        {
            return Instantiate(await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/Common/UI/Fade.prefab")).GetComponent<Fader>();
        }
    }
}
