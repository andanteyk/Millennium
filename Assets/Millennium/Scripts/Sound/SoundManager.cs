using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.Sound
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager I { get; private set; }

        private Dictionary<BgmType, AudioClip> BgmClips = new Dictionary<BgmType, AudioClip>();
        private Dictionary<SeType, AudioClip> SeClips = new Dictionary<SeType, AudioClip>();

        private AudioSource CurrentBgm = null;

        private GameObject AudioSourcePrefab;

        public bool IsLoaded { get; private set; } = false;

        private void Awake()
        {
            if (I != null)
            {
                Debug.LogError(nameof(SoundManager) + " ‚Íˆê‚Â‚¾‚¯”z’u‚·‚×‚«‚Å‚·");
                Destroy(this);
                return;
            }

            I = this;
        }

        private async void Start()
        {
            BgmClips.Add(BgmType.None, null);
            BgmClips.Add(BgmType.Test, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Bgm/Binar_intro.wav"));

            SeClips.Add(SeType.Ok, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/Ok.wav"));
            SeClips.Add(SeType.Cancel, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/Cancel.wav"));
            SeClips.Add(SeType.Accept, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/Accept.wav"));
            SeClips.Add(SeType.Disabled, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/Disabled.wav"));

            AudioSourcePrefab = await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/Common/Sounds/AudioSource.prefab");

            IsLoaded = true;
        }

        public async UniTask PlayBgm(BgmType bgm)
        {
            await AwaitLoading();

            if (CurrentBgm != null)
            {
                await StopBgm();

                CurrentBgm.clip = BgmClips[bgm];
            }
            else
            {
                CurrentBgm = CreateSource(BgmClips[bgm]);
            }

            CurrentBgm.loop = true;
            CurrentBgm.volume = 0.1f;       // TODO
            CurrentBgm.Play();
        }

        public async UniTask PlaySe(SeType se)
        {
            await AwaitLoading();

            var source = CreateSource(SeClips[se]);
            source.Play();

            await UniTask.WaitWhile(() => source.isPlaying);

            Destroy(source.gameObject);
        }

        public async UniTask StopBgm()
        {
            if (CurrentBgm != null)
            {
                await DOTween.To(() => CurrentBgm.volume, value => CurrentBgm.volume = value, 0, 1).ToUniTask();
                CurrentBgm.Stop();
            }
        }


        private AudioSource CreateSource(AudioClip clip)
        {
            var sourceObject = Instantiate(AudioSourcePrefab);
            sourceObject.transform.parent = this.transform;

            var source = sourceObject.GetComponent<AudioSource>();
            source.clip = clip;

            return source;
        }

        private async UniTask AwaitLoading()
        {
            if (!IsLoaded)
                await UniTask.WaitUntil(() => IsLoaded);
        }
    }


    public enum BgmType
    {
        None,
        Test,
    }

    public enum SeType
    {
        Ok,
        Cancel,
        Accept,
        Disabled,
    }
}