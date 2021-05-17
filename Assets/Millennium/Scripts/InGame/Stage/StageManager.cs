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
            Yuzu,
        }

        private Player m_Player = null;
        private GameObject m_Background = null;
        private string m_CurrentStage = null;
        private float m_BattleStarted;
        private bool m_AllowSpawn = true;

        private async UniTask<StageData> LoadStage(string stageAddress)
        {
            m_CurrentStage = stageAddress;
            var stage = await Addressables.LoadAssetAsync<StageData>(stageAddress);
            UnityEngine.Assertions.Assert.IsTrue(stage != null);
            return stage;
        }


        public async UniTask Play(StageData stage, CancellationToken token)
        {
            SoundManager.I.PlayBgm(Sound.BgmType.Battle).Forget();
            EffectManager.I.Play(EffectType.StageStart, Vector3.zero);
            m_Background = Instantiate(stage.Background);


            float skipFrom = 0;         // for debug


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

                        if (m_AllowSpawn)
                        {
                            var instance = Instantiate(enemy.Prefab);
                            instance.transform.position = enemy.Position;
                        }
                    }
                    return instantiate(enemy, token);
                }));
        }


        private async UniTask InstantiatePlayer(EntryPoint.InGameParams param)
        {
            var playerPrefab = await Addressables.LoadAssetAsync<GameObject>(param.PlayerType switch
            {
                PlayerType.Alice => "Assets/Millennium/Assets/Prefabs/InGame/Player/PlayerAlice.prefab",
                PlayerType.Momoi => "Assets/Millennium/Assets/Prefabs/InGame/Player/PlayerMomoi.prefab",
                PlayerType.Midori => "Assets/Millennium/Assets/Prefabs/InGame/Player/PlayerMidori.prefab",
                PlayerType.Yuzu => "Assets/Millennium/Assets/Prefabs/InGame/Player/PlayerYuzu.prefab",
                _ => throw new InvalidOperationException($"playerType `{param.PlayerType}` is not supported"),
            });

            m_Player = Instantiate(playerPrefab).GetComponent<Player>();
            m_Player.transform.position = new Vector3(0, -100, 0);
            m_Player.IsDebugMode = param.IsDebugMode;
        }


        public async UniTask PlayNextStage()
        {
            var stages = new[] {
                "Assets/Millennium/Assets/Data/Stage1.asset",
                "Assets/Millennium/Assets/Data/Stage2.asset",
                "Assets/Millennium/Assets/Data/Stage3.asset",
                "Assets/Millennium/Assets/Data/Stage4.asset",
                "Assets/Millennium/Assets/Data/Stage5.asset",
                "Assets/Millennium/Assets/Data/Stage6.asset",
            };


            m_Player?.AddStageClearReward();

            var fade = await UI.Fader.CreateFade();
            fade.SetColor(Color.cyan);
            DontDestroyOnLoad(fade);
            await fade.Show();


            if (m_Background != null)
                Destroy(m_Background);


            GC.Collect();


            int index = Array.IndexOf(stages, m_CurrentStage);
            if (index == -1 || index >= stages.Length - 1)
            {
                // goto result
                await EntryPoint.StartOutGame(new EntryPoint.OutGameParams
                {
                    FirstUIAddress = "Assets/Millennium/Assets/Prefabs/OutGame/UI/Result.prefab",
                    Score = m_Player.Score + m_Player.Health * 500000 + m_Player.BombCount * 100000,
                    IsCleared = true,
                    BattleSeconds = Time.time - m_BattleStarted
                });
            }
            else
            {
                var stage = await LoadStage(stages[index + 1]);
                Play(stage, this.GetCancellationTokenOnDestroy()).Forget();
            }

            await fade.Hide();
        }

        public async UniTask GameOver()
        {
            Time.timeScale = 0;

            var fade = await UI.Fader.CreateFade();
            fade.SetColor(Color.cyan);
            DontDestroyOnLoad(fade);
            await fade.Show();

            await EntryPoint.StartOutGame(new EntryPoint.OutGameParams
            {
                FirstUIAddress = "Assets/Millennium/Assets/Prefabs/OutGame/UI/Result.prefab",
                Score = m_Player.Score,
                IsCleared = false,
                BattleSeconds = Time.time - m_BattleStarted
            });

            Time.timeScale = 1;
            await fade.Hide();
            Destroy(fade.gameObject);
        }


        public void SuppressEnemySpawn() => m_AllowSpawn = false;
        public void ResumeEnemySpawn() => m_AllowSpawn = true;


        public async void OnStart(EntryPoint.InGameParams param)
        {
            m_BattleStarted = Time.time;

            await InstantiatePlayer(param);

            Play(await LoadStage("Assets/Millennium/Assets/Data/Stage6.asset"), this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
