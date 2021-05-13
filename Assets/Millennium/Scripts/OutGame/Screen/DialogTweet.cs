using Cysharp.Threading.Tasks;
using Millennium.Sound;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Millennium.OutGame.Screen
{
    public class DialogTweet : ScreenBase
    {
        [SerializeField]
        private Button m_CloseButton;

        private void Start()
        {
            SelectFirstButton();

            OnStart().Forget();
        }

        private async UniTaskVoid OnStart()
        {
            var token = this.GetCancellationTokenOnDestroy();

            await m_CloseButton.onClick.GetAsyncEventHandler(token).OnInvokeAsync();

            if (token.IsCancellationRequested)
                return;

            SoundManager.I.PlaySe(SeType.Accept).Forget();
            Destroy(gameObject);
        }
    }
}
