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
            var tasks = new List<UniTask>();

            foreach (var enemy in stage.Enemies)
            {
                async UniTask instantiate(EnemyData enemy)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(enemy.SpawnSeconds));
                    var instance = Instantiate(enemy.Prefab);
                    instance.transform.position = enemy.Position;
                }
                tasks.Add(instantiate(enemy));
            }

            await UniTask.WhenAll(tasks);

            // note: ç≈å„Ç… goal object ÇíuÇ≠Ç∆Ç©ÇµÇ»Ç¢Ç∆Ç¢ÇØÇ»Ç¢Ç©Ç‡?
            Debug.Log("complete!");
        }

        // TODO : test
        private async void Start()
        {
            var data = await Addressables.LoadAssetAsync<StageData>("Assets/Millennium/Assets/Data/TestStage.asset");

            Play(data).Forget();
        }

    }
}
