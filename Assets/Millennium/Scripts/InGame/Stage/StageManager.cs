using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.InGame.Stage
{
    public class StageManager : MonoBehaviour
    {
        public async UniTask Play(StageData stage)
        {
            await UniTask.WhenAll(stage.Enemies.Select(enemy =>
                {
                    async UniTask instantiate(EnemyData enemy)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(enemy.SpawnSeconds));
                        var instance = Instantiate(enemy.Prefab);
                        instance.transform.position = enemy.Position;
                    }
                    return instantiate(enemy);
                }));
        }

        // TODO : test
        private async void Start()
        {
            var data = await Addressables.LoadAssetAsync<StageData>("Assets/Millennium/Assets/Data/TestStage.asset");

            Play(data).Forget();
        }

    }
}
