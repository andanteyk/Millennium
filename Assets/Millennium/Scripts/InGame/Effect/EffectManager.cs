using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

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

        public async UniTask Load()
        {
            // TODO: ロードが終わる前にコールされる可能性がある　なんかで待ち合わせできるようにしたほうがいいかも
            Prefabs = new Dictionary<EffectType, GameObject>
            {
                { EffectType.Explosion, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/Explosion.prefab") },

                { EffectType.PlusDecay, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/PlusDecay.prefab") },
                { EffectType.PlusDecayRed, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/PlusDecayRed.prefab") },
                { EffectType.PlusDecayGreen, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/PlusDecayGreen.prefab") },
                { EffectType.PlusDecayBlue, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/PlusDecayBlue.prefab") },

                { EffectType.CrossDecay, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/CrossDecay.prefab") },
                { EffectType.CrossDecayRed, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/CrossDecayRed.prefab") },
                { EffectType.CrossDecayGreen, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/CrossDecayGreen.prefab") },
                { EffectType.CrossDecayBlue, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/CrossDecayBlue.prefab") },
                { EffectType.Caution, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/Caution.prefab") },
                { EffectType.MuzzleFlash, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/MuzzleFlash.prefab") },
                { EffectType.Concentration, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/Concentration.prefab") },
                { EffectType.SpreadExplosion, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/SpreadExplosion.prefab") },

                { EffectType.MomoiUlt, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/MomoiUlt.prefab") },
                { EffectType.MidoriUlt, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/MidoriUlt.prefab") },
                { EffectType.Hit, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/Hit.prefab") },

                { EffectType.Warning, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/Warning.prefab") },
                { EffectType.StageStart, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/StageStart.prefab") },
                { EffectType.StageClear, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/StageClear.prefab") },

                { EffectType.CrossDecayYellow, await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Effect/CrossDecayYellow.prefab") },
            };
        }


        public Transform Play(EffectType effect, Vector3 position)
        {
            Assert.IsTrue(Prefabs[effect] != null, $"{effect} is null");

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

        Caution,

        MuzzleFlash,
        Concentration,
        SpreadExplosion,
        MomoiUlt,
        MidoriUlt,
        Hit,
        Warning,
        StageStart,
        StageClear,
        CrossDecayYellow,
    }
}
