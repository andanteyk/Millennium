using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
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
                    var fader = await Fader.CreateFade();
                    fader.SetColor(Color.cyan);             // TODO: そもそもデザインがよくないので　埋まらない色/形にする
                    await fader.Show();


                    var instance = Instantiate(await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/OutGame/UI/PlayerSelect.prefab"));
                    instance.transform.SetParent(GetComponentInParent<Canvas>().transform, false);


                    await fader.Hide();
                    Destroy(fader.gameObject);
                    Destroy(gameObject);
                });

            m_InformationButton.OnClickAsAsyncEnumerable(token)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    gameObject.SetActive(false);

                    var instance = Instantiate(await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/OutGame/UI/DialogInformation.prefab"));
                    instance.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
                    await instance.OnDestroyAsync();            // note: cancellation token?

                    gameObject.SetActive(true);

                    SelectFirstButton();
                });
        }


    }
}
