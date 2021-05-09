using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.Sound;
using Millennium.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Millennium.OutGame.Screen
{
    public class Title : ScreenBase
    {
        [SerializeField]
        private Button m_StartButton;

        [SerializeField]
        private Button m_InformationButton;


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            SelectFirstButton();

            m_StartButton.OnClickAsAsyncEnumerable(token)
                .Take(1)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    SoundManager.I.PlaySe(SeType.Accept).Forget();

                    await Transit("Assets/Millennium/Assets/Prefabs/OutGame/UI/PlayerSelect.prefab", token);
                }, token);

            m_InformationButton.OnClickAsAsyncEnumerable(token)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    SoundManager.I.PlaySe(SeType.Accept).Forget();

                    await Transit("Assets/Millennium/Assets/Prefabs/OutGame/UI/DialogInformation.prefab", token);
                }, token);
        }


    }
}
