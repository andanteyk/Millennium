using Cysharp.Threading.Tasks;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Player;
using Millennium.Sound;
using System;
using System.Collections.Generic;
using System.Threading;
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

        private Player m_Player = null;
        private string m_CurrentStage = null;


        private async UniTask<StageData> LoadStage(string stageAddress)
        {
            m_CurrentStage = stageAddress;
            var stage = await Addressables.LoadAssetAsync<StageData>(stageAddress);
            UnityEngine.Assertions.Assert.IsTrue(stage != null);
            return stage;
        }


        public async UniTask Play(StageData stage, CancellationToken token)
        {
            // TODO: test
            SoundManager.I.PlayBgm(Sound.BgmType.Test).Forget();


            float skipFrom = 0;         // for debug

            EffectManager.I.Play(EffectType.StageStart, Vector3.zero);

            await UniTask.WhenAll(stage.Enemies.Select(enemy =>
                {
                    async UniTask instantiate(EnemyData enemy, CancellationToken token)
                    {
                        float spawnSeconds = enemy.SpawnSeconds - skipFrom;
                        if (spawnSeconds < 0)
                            return;

                        await UniTask.Delay(TimeSpan.FromSeconds(spawnSeconds), cancellationToken: token);

                        if (token.IsCancellationRequested)
                            return;

                        var instance = Instantiate(enemy.Prefab);
                        instance.transform.position = enemy.Position;
                    }
                    return instantiate(enemy, token);
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

            m_Player = Instantiate(playerPrefab).GetComponent<Player>();
            m_Player.transform.position = new Vector3(0, -100, 0);
        }


        public async UniTask PlayNextStage()
        {
            var stages = new[] {
                "Assets/Millennium/Assets/Data/Stage1.asset",
                "Assets/Millennium/Assets/Data/Stage2.asset",
            };


            m_Player?.AddStageClearReward();

            var fade = await UI.Fader.CreateFade();
            fade.SetColor(Color.cyan);
            DontDestroyOnLoad(fade);
            await fade.Show();


            int index = Array.IndexOf(stages, m_CurrentStage);
            if (index == -1 || index >= stages.Length - 1)
            {
                // goto result
                await EntryPoint.StartOutGame(new EntryPoint.OutGameParams
                {
                    FirstUIAddress = "Assets/Millennium/Assets/Prefabs/OutGame/UI/Result.prefab",
                    Score = m_Player.Score + m_Player.Health * 5000 + m_Player.BombCount * 100000,
                    IsCleared = true,
                });
            }
            else
            {
                var stage = await LoadStage(stages[index + 1]);
                Play(stage, this.GetCancellationTokenOnDestroy()).Forget();
            }

            await fade.Hide();
        }


        // TODO: test
        public async void OnStart(EntryPoint.InGameParams param)
        {
            await InstantiatePlayer(param.PlayerType);

            Play(await LoadStage("Assets/Millennium/Assets/Data/Stage1.asset"), this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
