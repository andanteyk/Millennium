using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.Sound
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager I { get; private set; }

        private Dictionary<BgmType, AudioClip> BgmClips;
        private Dictionary<SeType, AudioClip> SeClips;

        private AudioSource CurrentBgm = null;
        private List<AudioSource> SeBuffer;

        private const int SeCapacity = 8;

        private GameObject AudioSourcePrefab;


        public bool IsLoaded { get; private set; } = false;

        private void Awake()
        {
            if (I != null)
            {
                Debug.LogError(nameof(SoundManager) + " は一つだけ配置すべきです");
                Destroy(this);
                return;
            }

            I = this;
        }

        public async UniTask Load()
        {
            BgmClips = new Dictionary<BgmType, AudioClip>
            {
                { BgmType.None, null },
                { BgmType.Test, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Bgm/Binar_intro.wav") }
            };

            SeClips = new Dictionary<SeType, AudioClip>
            {
                { SeType.Ok, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/Ok.wav") },
                { SeType.Cancel, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/Cancel.wav") },
                { SeType.Accept, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/Accept.wav") },
                { SeType.Disabled, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/Disabled.wav") },

                { SeType.Explosion, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/InGame/Explosion.wav") },
                { SeType.PlayerBulletHit, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/InGame/PlayerBulletHit.wav") },
                { SeType.PlayerBulletHitCritical, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/InGame/PlayerBulletHit2.wav") },
                { SeType.EnemyShot, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/InGame/EnemyShot.wav") },
                { SeType.PlayerDamaged, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/InGame/PlayerDamaged.wav") },
                { SeType.PlayerBulletImmune, await Addressables.LoadAssetAsync<AudioClip>("Assets/Millennium/Assets/Sounds/Se/InGame/PlayerBulletImmune.wav") },
            };

            AudioSourcePrefab = await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/Common/Sounds/AudioSource.prefab");
            SeBuffer = new List<AudioSource>(SeCapacity);
            for (int i = 0; i < SeCapacity; i++)
                SeBuffer.Add(CreateSource(null));

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
                CurrentBgm.priority = 10;
            }

            CurrentBgm.loop = true;
            CurrentBgm.volume = 0.1f;       // TODO: 作業用に低め
            CurrentBgm.Play();
        }

        public async UniTask PlaySe(SeType se)
        {
            await AwaitLoading();

            var clip = SeClips[se];

            // 同一フレームで同じのが発声していたらスキップ
            if (SeBuffer.Exists(s => s.isPlaying && s.clip == clip && s.time == 0))
                return;

            // 空き枠を探す(見つからなかったら一番長く再生しているやつを消す)
            var source = SeBuffer.Find(s => !s.isPlaying);
            if (source == null)
                source = SeBuffer.Aggregate((AudioSource)null, (current, next) => (current != null ? current.time : 0) <= next.time ? next : current);

            source.clip = clip;
            source.Play();

            // TODO: ↑ で上書きされたときに正しく動作しないかも?
            await UniTask.WaitWhile(() => source.isPlaying, cancellationToken: source.GetCancellationTokenOnDestroy());
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

        Explosion,
        PlayerBulletHit,
        PlayerBulletHitCritical,
        EnemyShot,
        PlayerDamaged,
        PlayerBulletImmune,
    }
}