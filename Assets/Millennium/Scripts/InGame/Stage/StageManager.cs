using Cysharp.Threading.Tasks;
using Millennium.Sound;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.InGame.Stage
{
    public class StageManager : MonoBehaviour
    {
        public enum PlayerType
        {
            Alice,
            Momoi,
            Midori,
        }

        public async UniTask Play(StageData stage, PlayerType playerType)
        {
            // TODO: test
            SoundManager.I.PlayBgm(Sound.BgmType.Test).Forget();


            float skipFrom = 0;


            InstantiatePlayer(playerType).Forget();


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


        private async UniTask InstantiatePlayer(PlayerType playerType)
        {
            var playerPrefab = await Addressables.LoadAssetAsync<GameObject>(playerType switch
            {
                PlayerType.Alice => "Assets/Millennium/Assets/Prefabs/InGame/Player/PlayerAlice.prefab",
                PlayerType.Momoi => "Assets/Millennium/Assets/Prefabs/InGame/Player/PlayerMomoi.prefab",
                PlayerType.Midori => "Assets/Millennium/Assets/Prefabs/InGame/Player/PlayerMidori.prefab",
                _ => throw new InvalidOperationException($"playerType `{playerType}` is not supported"),
            });
            var player = Instantiate(playerPrefab);
            player.transform.position = new Vector3(0, -100, 0);
        }




        // TODO : test
        public async void OnStart(EntryPoint.InGameParams param)
        {
            var data = await Addressables.LoadAssetAsync<StageData>("Assets/Millennium/Assets/Data/TestStage.asset");

            Play(data, param.PlayerType).Forget();
        }
    }
}
