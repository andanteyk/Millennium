using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public class BossYuuka : BossBase
    {
        [SerializeField]
        private GameObject m_NormalShotPrefab;

        [SerializeField]
        private GameObject m_LargeShotPrefab;

        [SerializeField]
        private GameObject m_ShieldPrefab;



        private void Start()
        {
            OnStart().Forget();
        }

        private async UniTaskVoid OnStart()
        {
            var destroyToken = this.GetCancellationTokenOnDestroy();


            SetupHealthGauge(4, destroyToken);
            DamageWhenEnter(destroyToken).Forget();
            EffectManager.I.Play(EffectType.Warning, Vector3.zero);
            SoundManager.I.PlaySe(SeType.Warning).Forget();

            await RandomMove(4, destroyToken);


            Health = HealthMax = 8000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        await RandomMove(1, token);
                        await PlayerAimShot1(token);
                        await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);
                    }

                    EffectManager.I.Play(EffectType.Concentration, transform.position);
                    SoundManager.I.PlaySe(SeType.Concentration).Forget();
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                    await PlayerAimShot2(token);
                }
            }, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 12000;
            await RunPhase(SkillIFF, destroyToken);
            await DropUltimateAccelerant(false, destroyToken);
            await OnEndPhase(destroyToken);



            Health = HealthMax = 8000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await RandomMove(1, token);

                    for (int i = 0; i < 3 && !token.IsCancellationRequested; i++)
                    {
                        await PlayerAimShot3(token);
                        await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);
                    }

                    await PlayerAimShot4(token);
                }
            }, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 12000;
            await RunPhase(SkillQED, destroyToken);



            await PlayDeathEffect(destroyToken);

            if (destroyToken.IsCancellationRequested)
                return;
            Destroy(gameObject);
        }



        private async UniTask PlayerAimShot1(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;


            float speed = 96;

            float playerDirection = BallisticMath.AimToPlayer(transform.position);

            for (int i = 0; i < 4 && !token.IsCancellationRequested; i++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    float direction = playerDirection + Mathf.PI / 6 * k;
                    var offset = BallisticMath.FromPolar(4, direction + Mathf.PI / 2);
                    BulletBase.Instantiate(m_NormalShotPrefab, transform.position + offset, BallisticMath.FromPolar(speed, direction));
                    BulletBase.Instantiate(m_NormalShotPrefab, transform.position - offset, BallisticMath.FromPolar(speed, direction));
                }

                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);

                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
            }
        }

        private async UniTask PlayerAimShot2(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            float playerDirection = BallisticMath.AimToPlayer(transform.position);


            foreach (float r in BallisticMath.CalculateWayRadians(playerDirection, 12, 12 * Mathf.Deg2Rad))
            {
                for (int k = 0; k < 4; k++)
                {
                    var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(32 - k * 4, r));
                    _ = DOTween.Sequence()
                        .AppendInterval(1f + k * 0.05f)
                        .Append(DOTween.To(() => bullet.Speed, value => bullet.Speed = value, BallisticMath.FromPolar(96, r), 0.5f)
                            .SetEase(Ease.Linear))
                        .SetLink(bullet.gameObject);
                }

                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: token);
            }
        }

        private async UniTask PlayerAimShot3(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            float playerDirection = BallisticMath.AimToPlayer(transform.position);


            for (int k = 0; k < 4; k++)
            {
                foreach (float r in BallisticMath.CalculateWayRadians(playerDirection, 9))
                {
                    var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(32 + k * 16, r));
                }
                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
            }

        }

        private async UniTask PlayerAimShot4(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            EffectManager.I.Play(EffectType.Caution, BallisticMath.PlayerPosition + Vector3.up * 32);


            float playerDirection = BallisticMath.AimToPlayer(transform.position);


            await foreach (var i in UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(1.5f), TimeSpan.FromSeconds(0.1f), updateTiming: PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(16)
                .WithCancellation(token))
            {
                foreach (var r in BallisticMath.CalculateWayRadians(playerDirection + Seiran.Shared.Next(-2, 2 + 1) * 4 * Mathf.Deg2Rad, 3, 8 * Mathf.Deg2Rad))
                {
                    BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(32 + i * 6, r));
                }

                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
            }

        }


        private async UniTask SkillIFF(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await PlaySkillBalloon("ユウカ", "I.F.F", token);

            try
            {
                m_DamageRatio = 0.1f;

                EffectManager.I.Play(EffectType.Concentration, transform.position);
                SoundManager.I.PlaySe(SeType.Concentration).Forget();

                EffectManager.I.Play(EffectType.Caution, new Vector3(-64, -16));

                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);


                // generate shield
                var shield = Instantiate(m_ShieldPrefab);
                shield.transform.position = new Vector3(-64, -16);
                {
                    var spriteRenderer = shield.GetComponent<SpriteRenderer>();
                    _ = DOTween.To(() => spriteRenderer.size, value => spriteRenderer.size = value, new Vector2(64, 16), 1)
                        .ChangeStartValue(new Vector2(64, 0))
                        .SetLink(shield.gameObject);
                }
                _ = shield.transform.DOLocalMoveX(64, 8)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(shield.gameObject);
                token.Register(() => Destroy(shield.gameObject));


                // fire "test" shot
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5f)).Take(1).WithCancellation(token))
                {
                    foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextSingle(0, Mathf.PI * 2), 36))
                        BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(48, r));

                    EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }

                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                m_DamageRatio = 1;

                // main loop
                await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate).WithCancellation(token))
                {
                    EffectManager.I.Play(EffectType.Concentration, transform.position);
                    SoundManager.I.PlaySe(SeType.Concentration).Forget();
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

                    await Baramaki(token);

                    await RandomMove(1, token);
                    await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
                }

            }
            finally
            {
                m_DamageRatio = 1;
            }
        }



        private async UniTask SkillQED(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await PlaySkillBalloon("ユウカ", "Q.E.D", token);

            int rectSize = 48;



            await foreach (var loopCount in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .WithCancellation(token))
            {
                EffectManager.I.Play(EffectType.Concentration, transform.position);
                SoundManager.I.PlaySe(SeType.Concentration).Forget();

                for (int y = -1; y <= 1; y += 2)
                    for (int x = -1; x <= 1; x += 2)
                        EffectManager.I.Play(EffectType.Caution, transform.position + new Vector3(rectSize * x, rectSize * y));

                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);


                // generate shield
                var shield = Instantiate(m_ShieldPrefab);
                shield.transform.position = transform.position;
                {
                    var spriteRenderer = shield.GetComponent<SpriteRenderer>();
                    _ = DOTween.To(() => spriteRenderer.size, value => spriteRenderer.size = value, new Vector2(48, 48), 1)
                        .ChangeStartValue(new Vector2(48, 0))
                        .SetLink(shield.gameObject);
                }
                _ = shield.transform.DOLocalMoveY(-320, 4)
                    .SetEase(Ease.InQuad)
                    .SetDelay(3.5f)
                    .SetLink(shield.gameObject)
                    .OnComplete(() => Destroy(shield.gameObject));
                token.Register(() => Destroy(shield.gameObject));



                int detail = 32;
                await foreach (var i in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05f), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .Take(detail)
                    .WithCancellation(token))
                {
                    var waypoint = new[] { new Vector3(-rectSize, -rectSize), new Vector3(rectSize, -rectSize), new Vector3(rectSize, rectSize), new Vector3(-rectSize, rectSize) };

                    for (int k = 0; k < 4; k++)
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + Vector3.Lerp(waypoint[k], waypoint[(k + 1) & 3], (float)i / detail) + (i & 1) * 0.125f * waypoint[k], Vector3.zero);
                        _ = DOTween.To(() => bullet.Speed, value => bullet.Speed = value, BallisticMath.FromPolar(32, (90f * i / detail + k * 90 + 135 + (loopCount & 1) * 180) * Mathf.Deg2Rad), 2)
                            .SetEase(Ease.Linear)
                            .SetDelay(2 - 0.05f * i)
                            .SetLink(bullet.gameObject);

                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                    }

                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }



                await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: token);

                int aimDetail = 5;
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.25f), PlayerLoopTiming.FixedUpdate)
                   .Take(3)
                   .WithCancellation(token))
                {
                    var playerDirection = BallisticMath.AimToPlayer(transform.position);
                    await foreach (var i in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05f), PlayerLoopTiming.FixedUpdate)
                        .Select((_, i) => i)
                        .Take(aimDetail)
                        .WithCancellation(token))
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + ((float)i / aimDetail - 0.5f) * 64 * Vector3.right, BallisticMath.FromPolar(64, playerDirection));

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                    }
                }


                await RandomMove(1, token);
            }

        }


        private UniTask Baramaki(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            int bullets = 128;
            int chunk = 32;

            return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(bullets)
                .ForEachAsync(i =>
                {
                    for (int k = 0; k < 2; k++)
                    {
                        var r = i * 7 * Mathf.Deg2Rad + k * Mathf.PI;
                        var bullet = BulletBase.Instantiate(m_LargeShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(32, r));

                        static async UniTaskVoid MoveBullet(BulletBase bullet, float radian, int index, int chunk, CancellationToken token)
                        {
                            token.ThrowIfCancellationRequested();
                            await bullet.DOSpeed(Vector3.zero, 1);

                            await UniTask.Delay(TimeSpan.FromSeconds((chunk - index % chunk) * 0.05), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

                            token.ThrowIfCancellationRequested();
                            await bullet.DOSpeed(BallisticMath.FromPolar(32, radian), 1f);

                            token.ThrowIfCancellationRequested();
                            await bullet.DOSpeed(BallisticMath.FromPolar(192, radian), 1);
                        }

                        MoveBullet(bullet, r, i, chunk, bullet.GetCancellationTokenOnDestroy()).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                    }
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();

                }, token);
        }

    }

}
