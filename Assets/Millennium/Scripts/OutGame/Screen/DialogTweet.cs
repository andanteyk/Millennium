using Cysharp.Threading.Tasks;
using Millennium.Sound;
using System;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif


namespace Millennium.OutGame.Screen
{
    public class DialogTweet : ScreenBase
    {
        [SerializeField]
        private Button m_CloseButton;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void RegisterPopupEvent(string url);
#endif


        public string TweetMessage { get; set; }


        private void Start()
        {
            SelectFirstButton();

            OnStart().Forget();
        }

        private async UniTaskVoid OnStart()
        {
            var token = this.GetCancellationTokenOnDestroy();

            RegisterTweetEvent();

            await m_CloseButton.onClick.GetAsyncEventHandler(token).OnInvokeAsync();

            token.ThrowIfCancellationRequested();

            SoundManager.I.PlaySe(SeType.Accept).Forget();
            Destroy(gameObject);
        }


        public void RegisterTweetEvent()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            RegisterPopupEvent(Uri.EscapeUriString(TweetMessage));
#else
            Application.OpenURL(Uri.EscapeUriString(TweetMessage));
#endif
        }
    }
}
