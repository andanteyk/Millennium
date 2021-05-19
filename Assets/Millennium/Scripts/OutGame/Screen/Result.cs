using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.InGame.Stage;
using Millennium.IO;
using Millennium.Sound;
using Millennium.UI;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static Millennium.InGame.Stage.StageManager;

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

        private PlayerType m_PlayerType;
        private bool m_IsCleared;



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
                .ForEachAwaitWithCancellationAsync((_, token) => ShowTweetDialog(token), token);
        }

        public override void ReceiveOutGameParameter(EntryPoint.OutGameParams param)
        {
            base.ReceiveOutGameParameter(param);

            m_ScoreText.text = param.Score.ToString();
            m_Time.text = TimeSpan.FromSeconds(param.BattleSeconds).ToString(@"mm\:ss\.ff");
            m_IsCleared = param.IsCleared;
            m_PlayerType = param.PlayerType;

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
            instance.GetComponent<DialogTweet>().TweetMessage = CreateMessage();


            await fader.Hide();
            Destroy(fader.gameObject);

            token.ThrowIfCancellationRequested();

            await instance.OnDestroyAsync();

            await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);

            token.ThrowIfCancellationRequested();
            SelectFirstButton();
        }


        private string CreateMessage()
        {
            string playerName = m_PlayerType switch
            {
                PlayerType.Momoi => "モモイ",
                PlayerType.Midori => "ミドリ",
                PlayerType.Alice => "アリス",
                PlayerType.Yuzu => "ユズ",
                _ => throw new NotSupportedException(),
            };

            string body = $"ブルアカ二次創作 STG <Millennium Assault> を [{playerName}] で{(m_IsCleared ? "クリア" : "プレイ")}! ({m_ScoreText.text} pt)";
            string url = @$"https://twitter.com/intent/tweet?text={ body }&hashtags={ "MillenniumAssault" }&url={ @"https://andanteyk.github.io/MillenniumAssault" }";
            return url;
        }
    }
}
