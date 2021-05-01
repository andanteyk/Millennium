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
            float skipFrom = 5;

            await UniTask.WhenAll(stage.Enemies.Select(enemy =>
                {
                    async UniTask instantiate(EnemyData enemy)
                    {
                        float spawnSeconds = enemy.SpawnSeconds - skipFrom;
                        if (spawnSeconds < 0)
                            return;

                        await UniTask.Delay(TimeSpan.FromSeconds(spawnSeconds));
                        var instance = Instantiate(enemy.Prefab);
                        instance.transform.position = enemy.Position;
                    }
                    return instantiate(enemy);
                }));
        }

        // TODO : test
        private async void Start()
        {
            var data = await Addressables.LoadAssetAsync<StageData>("Assets/Millennium/Assets/Data/Stage1.asset");

            Play(data).Forget();
        }

    }
}
