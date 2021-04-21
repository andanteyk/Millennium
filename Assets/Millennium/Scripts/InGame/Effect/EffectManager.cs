using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.InGame.Effect
{
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager I { get; private set; }
        private Dictionary<EffectType, GameObject> Prefabs;


        private void Awake()
        {
            if (I != null)
            {
                Debug.LogError(nameof(EffectManager) + " は一つだけ配置すべきです");
                Destroy(this);
                return;
            }

            I = this;
        }

        private async void Start()
        {
            // TODO: ロードが終わる前にコールされる可能性がある　なんかで待ち合わせできるようにしたほうがいいかも
            Prefabs = new Dictionary<EffectType, GameObject>();

            Prefabs.Add(EffectType.Explosion, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/Explosion.prefab"));

            Prefabs.Add(EffectType.PlusDecay, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/PlusDecay.prefab"));
            Prefabs.Add(EffectType.PlusDecayRed, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/PlusDecayRed.prefab"));
            Prefabs.Add(EffectType.PlusDecayGreen, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/PlusDecayGreen.prefab"));
            Prefabs.Add(EffectType.PlusDecayBlue, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/PlusDecayBlue.prefab"));

            Prefabs.Add(EffectType.CrossDecay, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/CrossDecay.prefab"));
            Prefabs.Add(EffectType.CrossDecayRed, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/CrossDecayRed.prefab"));
            Prefabs.Add(EffectType.CrossDecayGreen, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/CrossDecayGreen.prefab"));
            Prefabs.Add(EffectType.CrossDecayBlue, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/CrossDecayBlue.prefab"));

        }


        public Transform Play(EffectType effect, Vector3 position)
        {
            var instance = Instantiate(Prefabs[effect]);
            instance.transform.position = position;
            return instance.transform;
        }
    }

    public enum EffectType
    {
        Explosion,
        PlusDecay,
        PlusDecayRed,
        PlusDecayGreen,
        PlusDecayBlue,
        CrossDecay,
        CrossDecayRed,
        CrossDecayGreen,
        CrossDecayBlue,
    }
}
