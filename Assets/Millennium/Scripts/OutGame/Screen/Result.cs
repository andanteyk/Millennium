using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Millennium.OutGame.Screen
{
    public class Result : ScreenBase
    {
        [SerializeField]
        private Button m_OKButton;

        [SerializeField]
        private Button m_TweetButton;

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            SelectFirstButton();

            m_OKButton.OnClickAsAsyncEnumerable(token)
                .Take(1)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    await Transit("Assets/Millennium/Assets/Prefabs/OutGame/UI/Title.prefab", token);
                }, token);

            m_TweetButton.OnClickAsAsyncEnumerable(token)
                .ForEachAsync(_ =>
               {
                   // note: �ꉞ�C���W�F�N�V��������Ȃ��悤�ɒ���
                   // ������� URL �̓G�X�P�[�v����H

                   Application.OpenURL(@$"https://twitter.com/intent/tweet?text={ "�e�X�g" }&hashtags={ "�҂�҂�" }&url={ @"https://andantesoft.hatenablog.com/" }");
               }, token);
        }
    }
}
