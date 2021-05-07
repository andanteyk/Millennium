using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.Mathematics;
using Millennium.Sound;
using Millennium.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public abstract class BossBase : EnemyBase
    {
        [SerializeField]
        protected GameObject m_BulletRemoverPrefab;

        protected CancellationTokenSource m_PhaseToken;

        protected float m_DamageRatio = 1;


        protected async UniTask RandomMove(float moveSeconds, CancellationToken token)
        {
            var region = new Rect(Mathf.Lerp(InGameConstants.FieldArea.xMin, InGameConstants.FieldArea.xMax, 0.1f), Mathf.Lerp(InGameConstants.FieldArea.yMin, InGameConstants.FieldArea.yMax, 0.625f),
                InGameConstants.FieldArea.width * 0.8f, InGameConstants.FieldArea.height * 0.25f);


            float GetRandomProportion() => (Seiran.Shared.NextSingle(-1, 1) * Seiran.Shared.NextSingle(-1, 1) + 1) / 2;

            var destination = new Vector3(
                region.xMin + GetRandomProportion() * region.width,
                region.yMin + GetRandomProportion() * region.height);

            await transform.DOMove(destination, moveSeconds)
                .SetUpdate(UpdateType.Fixed)
                .SetLink(gameObject)
                .WithCancellation(cancellationToken: token);
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


        protected async UniTask OnEndPhase(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            Instantiate(m_BulletRemoverPrefab).transform.position = transform.position;
            InGameUI.I.BossHealthGauge.DecrementSubGauge();

            EffectManager.I.Play(EffectType.Explosion, transform.position);
            SoundManager.I.PlaySe(SeType.Explosion).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(3), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

            //EffectManager.I.Play(EffectType.Concentration, transform.position);
            //SoundManager.I.PlaySe(SeType.Concentration).Forget();
            //await UniTask.Delay(TimeSpan.FromSeconds(2), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);
        }


        protected async UniTask PlayDeathEffect(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            try
            {
                Time.timeScale = 1 / 3f;

                Instantiate(m_BulletRemoverPrefab).transform.position = transform.position;
                InGameUI.I.BossHealthGauge.DecrementSubGauge();

                for (int i = 0; i < 8 && !token.IsCancellationRequested; i++)
                {
                    EffectManager.I.Play(EffectType.Explosion, transform.position + Seiran.Shared.InsideUnitCircle());
                    SoundManager.I.PlaySe(SeType.Explosion).Forget();
                    await UniTask.Delay(TimeSpan.FromSeconds(0.125f), cancellationToken: token);
                }

                // TODO: ‚±‚±‚Åƒ‚ƒfƒ‹‚ð”ñ•\Ž¦‚É‚·‚é
                GetComponent<SpriteRenderer>().color = Color.clear;

                EffectManager.I.Play(EffectType.Explosion, transform.position);
                EffectManager.I.Play(EffectType.SpreadExplosion, transform.position);
                SoundManager.I.PlaySe(SeType.SpreadExplosion).Forget();

                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Time.timeScale = 1f;
            }
        }


        public override void DealDamage(DamageSource damage)
        {
            if (m_PhaseToken == null)
                return;


            Health -= Mathf.FloorToInt(damage.Damage * m_DamageRatio);

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
