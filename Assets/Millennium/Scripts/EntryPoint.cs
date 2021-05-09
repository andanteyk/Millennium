using Cysharp.Threading.Tasks;
using Millennium.InGame.Effect;
using Millennium.InGame.Stage;
using Millennium.IO;
using Millennium.OutGame.Screen;
using Millennium.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Millennium
{

    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_NowLoading;

        public class InGameParams
        {
            public StageManager.PlayerType PlayerType;
        }

        public class OutGameParams
        {
            // ê›åvÇ™å´Ç≠Ç»Ç¢ :(
            public string FirstUIAddress;
            public long Score;
            public bool IsCleared;
        }



        private async void Start()
        {
            await UniTask.WhenAll(
                EffectManager.I.Load(),
                SoundManager.I.Load());

            Destroy(m_NowLoading);

            await StartOutGame(new OutGameParams
            {
                FirstUIAddress = "Assets/Millennium/Assets/Prefabs/OutGame/UI/Title.prefab"
            });
        }


        public static async UniTask StartInGame(InGameParams param)
        {
            if (SceneManager.GetSceneByName("OutGame").isLoaded)
                await SceneManager.UnloadSceneAsync("OutGame");
            if (!SceneManager.GetSceneByName("InGame").isLoaded)
                await SceneManager.LoadSceneAsync("InGame", LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("InGame"));

            FindObjectOfType<StageManager>().OnStart(param);
        }

        public static async UniTask StartOutGame(OutGameParams param)
        {
            if (SceneManager.GetSceneByName("InGame").isLoaded)
                await SceneManager.UnloadSceneAsync("InGame");
            if (!SceneManager.GetSceneByName("OutGame").isLoaded)
                await SceneManager.LoadSceneAsync("OutGame", LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("OutGame"));

            var instance = Instantiate(await Addressables.LoadAssetAsync<GameObject>(param.FirstUIAddress));
            instance.transform.SetParent(GameObject.Find("RootCanvas").GetComponent<Canvas>().transform, false);
            instance.GetComponent<ScreenBase>()?.ReceiveOutGameParameter(param);

            Time.timeScale = 1;

            SoundManager.I.PlayBgm(BgmType.Title).Forget();
        }
    }
}
