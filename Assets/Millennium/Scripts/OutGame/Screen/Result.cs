using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.Sound;
using Millennium.UI;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private Button m_OKButton;

        [SerializeField]
        private Button m_TweetButton;

        private bool m_IsCleared;


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            // Shot �������ςȂ��őJ�ڂ��Ă���Ƒ����� ok ����Đi��ł��܂��ꍇ������̂�
            UniTask.Delay(TimeSpan.FromSeconds(1)).ContinueWith(SelectFirstButton);

            m_OKButton.OnClickAsAsyncEnumerable(token)
                .Take(1)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    SoundManager.I.PlaySe(SeType.Accept).Forget();

                    await Transit("Assets/Millennium/Assets/Prefabs/OutGame/UI/Title.prefab", token);
                }, token).Forget();

            m_TweetButton.OnClickAsAsyncEnumerable(token)
                .ForEachAsync(_ =>
               {
                   SoundManager.I.PlaySe(SeType.Accept).Forget();

                   // note: �ꉞ�C���W�F�N�V��������Ȃ��悤�ɒ���

                   string body = $"�u���A�J�񎟑n�� STG <Millennium Assault> ��{(m_IsCleared ? "�N���A" : "�v���C")}���܂���! (�X�R�A: {m_ScoreText.text})";

                   Application.OpenURL(@$"https://twitter.com/intent/tweet?text={ body }&hashtags={ "MillenniumAssault" }&url={ @"https://andantesoft.hatenablog.com/" }");
               }, token).Forget();
        }

        public override void ReceiveOutGameParameter(EntryPoint.OutGameParams param)
        {
            base.ReceiveOutGameParameter(param);

            m_ScoreText.text = param.Score.ToString();
            m_IsCleared = param.IsCleared;

            m_Title.text = m_IsCleared ? "Battle Completed" : "Defeated";
        }
    }
}
