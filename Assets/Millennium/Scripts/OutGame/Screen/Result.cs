using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.Sound;
using Millennium.UI;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Millennium.OutGame.Screen
{
    public class Result : ScreenBase
    {
        [SerializeField]
        private TextMeshProUGUI m_Title;
        [SerializeField]
        private TextMeshProUGUI m_ScoreText;
        [SerializeField]
        private TextMeshProUGUI m_Time;

        [SerializeField]
        private Button m_OKButton;

        [SerializeField]
        private Button m_TweetButton;

        private bool m_IsCleared;


#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ShowPopup(string url);
#endif


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            // Shot 押しっぱなしで遷移してくると即座に ok されて進んでしまう場合があるので
            UniTask.Delay(TimeSpan.FromSeconds(1)).ContinueWith(SelectFirstButton);

            m_OKButton.OnClickAsAsyncEnumerable(token)
                .Take(1)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    SoundManager.I.PlaySe(SeType.Accept).Forget();

                    await Transit("Assets/Millennium/Assets/Prefabs/OutGame/UI/Title.prefab", token);
                }, token).Forget();

            m_TweetButton.OnClickAsAsyncEnumerable(token)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    SoundManager.I.PlaySe(SeType.Accept).Forget();

                    // note: 一応インジェクションされないように注意

                    string body = $"ブルアカ二次創作 STG <Millennium Assault> を{(m_IsCleared ? "クリア" : "プレイ")}しました! (スコア: {m_ScoreText.text})";
                    string url = @$"https://twitter.com/intent/tweet?text={ body }&hashtags={ "MillenniumAssault" }&url={ @"https://andanteyk.github.io/MillenniumAssault" }";

#if UNITY_WEBGL && !UNITY_EDITOR
                    ShowPopup(url);
                    await ShowTweetDialog(token);
#else
                    Application.OpenURL(url);
                    await ShowTweetDialog(token);
#endif



                }, token).Forget();
        }

        public override void ReceiveOutGameParameter(EntryPoint.OutGameParams param)
        {
            base.ReceiveOutGameParameter(param);

            m_ScoreText.text = param.Score.ToString();
            m_Time.text = TimeSpan.FromSeconds(param.BattleSeconds).ToString(@"mm\:ss\.ff");
            m_IsCleared = param.IsCleared;

            m_Title.text = m_IsCleared ? "Battle Completed" : "Defeated";
        }

        private async UniTask ShowTweetDialog(CancellationToken token)
        {
            // TODO: cancellable
            var fader = await Fader.CreateFade();
            fader.SetColor(Color.cyan);
            await fader.Show();



            var instance = Instantiate(await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/OutGame/UI/DialogTweet.prefab"));
            instance.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
            instance.transform.SetAsLastSibling();


            await fader.Hide();
            Destroy(fader.gameObject);

            token.ThrowIfCancellationRequested();

            await instance.GetCancellationTokenOnDestroy().ToUniTask().Item1;

            await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);
            SelectFirstButton();
        }
    }
}
