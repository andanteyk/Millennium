using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Item;
using Millennium.InGame.Stage;
using Millennium.Mathematics;
using Millennium.Sound;
using Millennium.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.InGame.Entity.Enemy
{
    public abstract class BossBase : EnemyBase
    {
        [SerializeField]
        protected GameObject m_BulletRemoverPrefab;

        protected CancellationTokenSource m_PhaseToken;

        protected float m_DamageRatio = 1;



        protected async UniTask MoveTo(Vector3 destination, float moveSeconds, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            await transform.DOMove(destination, moveSeconds)
                .SetUpdate(UpdateType.Fixed)
                .SetLink(gameObject)
                .WithCancellation(cancellationToken: token);
        }


        protected UniTask RandomMove(float moveSeconds, CancellationToken token)
        {
            var region = new Rect(Mathf.Lerp(InGameConstants.FieldArea.xMin, InGameConstants.FieldArea.xMax, 0.1f), Mathf.Lerp(InGameConstants.FieldArea.yMin, InGameConstants.FieldArea.yMax, 0.625f),
                InGameConstants.FieldArea.width * 0.8f, InGameConstants.FieldArea.height * 0.25f);


            float GetRandomProportion() => (Seiran.Shared.NextSingle(-1, 1) * Seiran.Shared.NextSingle(-1, 1) + 1) / 2;

            var destination = new Vector3(
                region.xMin + GetRandomProportion() * region.width,
                region.yMin + GetRandomProportion() * region.height);

            return MoveTo(destination, moveSeconds, token);
        }

        protected void SetupHealthGauge(int initialSubGauge, CancellationToken token)
        {
            InGameUI.I.BossHealthGauge.SetSubGauge(initialSubGauge);
            InGameUI.I.BossHealthGauge.Show();
            UniTaskAsyncEnumerable.EveryUpdate()
                .ForEachAsync(_ =>
                {
                    InGameUI.I.BossHealthGauge.SetGauge(Health, HealthMax);
                }, token)
                .Forget();
            token.Register(() => InGameUI.I.BossHealthGauge.Hide());
        }


        protected async UniTask RunPhase(Func<CancellationToken, UniTask> action, CancellationToken destroyToken)
        {
            UnityEngine.Assertions.Assert.IsNull(m_PhaseToken);
            m_PhaseToken = new CancellationTokenSource();
            using (var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(destroyToken, m_PhaseToken.Token))
            {
                var combinedToken = combinedTokenSource.Token;

                await action(combinedToken).SuppressCancellationThrow();
            }
        }


        protected UniTask OnEndPhase(CancellationToken token)
            => OnEndPhase(3, token);

        protected UniTask OnEndPhaseShort(CancellationToken token)
            => OnEndPhase(1.5f, token);

        protected async UniTask OnEndPhase(float waitSeconds, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            Instantiate(m_BulletRemoverPrefab).transform.position = transform.position;
            InGameUI.I.BossHealthGauge.DecrementSubGauge();

            EffectManager.I.Play(EffectType.Explosion, transform.position);
            SoundManager.I.PlaySe(SeType.Explosion).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

        }


        protected async UniTask PlayDeathEffect(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                Time.timeScale = 1 / 3f;

                Instantiate(m_BulletRemoverPrefab).transform.position = transform.position;
                InGameUI.I.BossHealthGauge.DecrementSubGauge();

                await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.125f))
                    .Take(6)
                    .ForEachAsync(_ =>
                    {
                        EffectManager.I.Play(EffectType.Explosion, transform.position + Seiran.Shared.InsideUnitCircle());
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();

                    }, token);

                token.ThrowIfCancellationRequested();

                EffectManager.I.Play(EffectType.Explosion, transform.position);
                EffectManager.I.Play(EffectType.SpreadExplosion, transform.position);
                SoundManager.I.PlaySe(SeType.SpreadExplosion).Forget();

                await DropMedkit(true, token);

                transform.position = new Vector3(0, 1024);          // 雑に非表示にする

                Time.timeScale = 1f;

                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

                EffectManager.I.Play(EffectType.StageClear, Vector3.zero);

                await UniTask.Delay(TimeSpan.FromSeconds(6), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Time.timeScale = 1f;
                GotoNextStage();
            }
        }


        protected async UniTask DropMedkit(bool isAutoCollect, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var medkit = Instantiate(await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Item/ItemMedkit.prefab")).GetComponent<ItemMedkit>();
            medkit.transform.position = transform.position;

            if (isAutoCollect)
                medkit.SetAutoCollect();
        }

        protected async UniTask DropUltimateAccelerant(bool isAutoCollect, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var ultimate = Instantiate(await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Item/ItemUltimateAccelerant.prefab")).GetComponent<ItemUltimateAccelerant>();
            ultimate.transform.position = transform.position;

            if (isAutoCollect)
                ultimate.SetAutoCollect();
        }

        protected void PlayBalloon(string name, string text)
        {
            var balloon = EffectManager.I.PlayBalloonOnTop();
            balloon.Text = text;

            if (string.IsNullOrEmpty(name))
                balloon.ShowsName = false;
            else
                balloon.NameText = name;
        }

        protected async UniTask PlaySkillBalloon(string name, string text, CancellationToken token)
        {
            PlayBalloon(name, text);
            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();

            await UniTask.Delay(TimeSpan.FromSeconds(1.5), cancellationToken: token);
        }


        // for mid boss
        protected async UniTask EscapeEvent(GameObject rewardItem, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            m_DamageRatio = 0;

            if (Health <= 0)
            {
                var item = Instantiate(rewardItem);
                item.transform.position = transform.position;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.5), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

            await MoveTo(new Vector3(128, 128), 2, token);
        }

        protected void SuppressEnemySpawn(CancellationToken destroyToken)
        {
            var stageManager = FindObjectOfType<StageManager>();
            stageManager.SuppressEnemySpawn();
            destroyToken.Register(() => stageManager.ResumeEnemySpawn());
        }



        protected void GotoNextStage()
        {
            var stageManager = FindObjectOfType<Stage.StageManager>();
            if (stageManager != null)
                stageManager.PlayNextStage().Forget();

            // null のときはゲームオーバー (でシーンアンロード中) …のはず
        }


        public override void DealDamage(DamageSource damage)
        {
            if (m_PhaseToken == null)
                return;


            Health -= Mathf.FloorToInt(damage.Damage * m_DamageRatio);

            var player = damage.Attacker as Player.Player;
            if (player != null)
                player.AddScore(damage.Damage);


            if (Health / Math.Max(HealthMax, 0.0) <= 0.1)
            {
                SoundManager.I.PlaySe(SeType.PlayerBulletHitCritical).Forget();
            }
            else
            {
                SoundManager.I.PlaySe(SeType.PlayerBulletHit).Forget();
            }

            if (Health <= 0)
            {
                m_PhaseToken?.Cancel();
                m_PhaseToken?.Dispose();
                m_PhaseToken = null;
            }
        }

        private void OnDestroy()
        {
            m_PhaseToken?.Cancel();
            m_PhaseToken?.Dispose();
            m_PhaseToken = null;
        }

    }
}
