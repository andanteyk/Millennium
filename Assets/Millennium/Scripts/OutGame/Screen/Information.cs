using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Millennium.OutGame.Screen
{
    public class Information : ScreenBase
    {
        [SerializeField]
        private Button m_CloseButton;

        [SerializeField]
        private TextMeshProUGUI m_Text;

        private void Start()
        {
            base.SelectFirstButton();

            OnStart().Forget();
        }

        private async UniTaskVoid OnStart()
        {
            var token = this.GetCancellationTokenOnDestroy();

            var textAsset = await Addressables.LoadAssetAsync<TextAsset>("Assets/Millennium/Assets/Data/Information.txt");

            m_Text.text = textAsset.text;


            await m_CloseButton.onClick.GetAsyncEventHandler(token).OnInvokeAsync();
            await Transit("Assets/Millennium/Assets/Prefabs/OutGame/UI/Title.prefab", token);
        }

    }
}
