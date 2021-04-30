using Cysharp.Threading.Tasks;
using Millennium.InGame.Effect;
using Millennium.IO;
using Millennium.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Millennium
{

    public class EntryPoint : MonoBehaviour
    {
        // TEST
        private async void Start()
        {
            await UniTask.WhenAll(
                EffectManager.I.Load(),
                SoundManager.I.Load());

            // TODO: debug
            await StartInGame();
        }




        private async UniTask StartInGame()
        {
            if (SceneManager.GetSceneByName("OutGame").isLoaded)
                await SceneManager.UnloadSceneAsync("OutGame");
            if (!SceneManager.GetSceneByName("InGame").isLoaded)
                await SceneManager.LoadSceneAsync("InGame", LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("InGame"));

            // TODO: debug
            SoundManager.I.PlayBgm(Sound.BgmType.Test).Forget();
        }

        private async UniTask StartOutGame()
        {
            if (SceneManager.GetSceneByName("InGame").isLoaded)
                await SceneManager.UnloadSceneAsync("InGame");
            if (!SceneManager.GetSceneByName("OutGame").isLoaded)
                await SceneManager.LoadSceneAsync("OutGame", LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("OutGame"));
        }

    }

}
