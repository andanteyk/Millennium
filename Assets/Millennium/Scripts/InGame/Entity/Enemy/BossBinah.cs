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
    public class BossBinah : BossBase
    {
        [SerializeField]
        private GameObject m_NormalShotPrefab;

        [SerializeField]
        private GameObject[] m_Bodies;


        private Sequence[] m_MoveSequences;


        private void Start()
        {
            OnStart().Forget();
        }


        private async UniTaskVoid OnStart()
        {
            var destroyToken = this.GetCancellationTokenOnDestroy();

            SetupHealthGauge(4, destroyToken);
            EffectManager.I.Play(EffectType.Warning, Vector3.zero);
            SoundManager.I.PlaySe(SeType.Warning).Forget();


            // TODO: 多段ヒットするのでアリス大正義すぎる
            Health = HealthMax = 50000;
            await RunPhase(async token =>
            {
                await MoveAppeared(token);

                await (
                    Move1(token),
                    UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5), PlayerLoopTiming.FixedUpdate)
                        .Select((_, i) => i)
                        .ForEachAsync(i =>
                        {
                            foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 8))
                            {
                                var body = m_Bodies[i % m_Bodies.Length];
                                BulletBase.Instantiate(m_NormalShotPrefab, body.transform.position, BallisticMath.FromPolar(64, r));

                                EffectManager.I.Play(EffectType.MuzzleFlash, body.transform.position);
                                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                            }
                        }, token)
                    );
            }, destroyToken);
            await OnEndPhase(destroyToken);


            await PlayDeathEffect(destroyToken);

            if (destroyToken.IsCancellationRequested)
                return;
            Destroy(gameObject);
        }



        private async UniTask MoveAppeared(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                m_DamageRatio = 0.1f;

                foreach (var b in m_Bodies)
                    b.transform.position = new Vector3(-32, 128);

                var origin = new Vector3(-32, 32);
                await MoveStraight(origin, 2, token);
                await MoveCircularUpper(origin += new Vector3(32, -32), 1, token);
                await MoveCircularLower(origin += new Vector3(32, 32), 1, token);
                await MoveCircularUpper(origin += new Vector3(-32, 32), 1, token);
                await MoveCircularLower(origin += new Vector3(-32, -32), 1, token);
                await MoveStraight(origin += new Vector3(0, -32), 1, token);
                await PauseMove(token);

                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

                foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 16))
                {
                    BulletBase.Instantiate(m_NormalShotPrefab, m_Bodies[0].transform.position, BallisticMath.FromPolar(16, r));

                    EffectManager.I.Play(EffectType.MuzzleFlash, m_Bodies[0].transform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }

                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            }
            finally
            {
                m_DamageRatio = 1;
            }
        }


        /// <summary>
        /// 8 の字
        /// </summary>
        private async UniTask Move1(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 手作業つらくないですか_(:3｣∠)_
                await MoveStraight(new Vector3(64, 64), 3, token);
                await MoveCircularLower(new Vector3(80, 48), 1, token);
                await MoveCircularUpper(new Vector3(64, 32), 1, token);
                await MoveStraight(new Vector3(-64, 32), 3, token);
                await MoveCircularLower(new Vector3(-80, 16), 1, token);
                await MoveCircularUpper(new Vector3(-64, 0), 1, token);
                await MoveStraight(new Vector3(64, 0), 3, token);
                await MoveCircularLower(new Vector3(80, 16), 1, token);
                await MoveCircularUpper(new Vector3(64, 32), 1, token);
                await MoveStraight(new Vector3(-64, 32), 3, token);
                await MoveCircularLower(new Vector3(-80, 48), 1, token);
                await MoveCircularUpper(new Vector3(-64, 64), 1, token);
            }
        }



        private UniTask PauseMove(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (m_MoveSequences == null)
                return UniTask.CompletedTask;

            foreach (var s in m_MoveSequences)
                _ = s?.Pause();

            return UniTask.CompletedTask;
        }


        private async UniTask Move(Action<Sequence, Transform> sequenceSetter, CancellationToken token)
        {
            float unitDelaySeconds = 0.25f;


            token.ThrowIfCancellationRequested();


            m_MoveSequences ??= new Sequence[m_Bodies.Length];
            foreach (var s in m_MoveSequences)
                s?.Play();

            for (int i = 0; i < m_Bodies.Length; i++)
            {
                m_MoveSequences[i] = DOTween.Sequence()
                    .AppendInterval(unitDelaySeconds * i);

                sequenceSetter(m_MoveSequences[i], m_Bodies[i].transform);

                _ = m_MoveSequences[i].SetUpdate(UpdateType.Fixed)
                    .SetLink(m_Bodies[i])
                    .WithCancellation(token);
            }

            await m_MoveSequences[0];

            await PauseMove(token);
        }

        private UniTask MoveStraight(Vector3 destination, float duration, CancellationToken token)
        {
            return Move((sequence, t) =>
            {
                sequence.Append(
                    t.DOMove(destination, duration)
                    .SetEase(Ease.Linear));
            }, token);
        }


        /// <summary> 
        /// 　時計回り ◞ ◜
        /// 反時計回り ◝ ◟
        /// </summary>
        private UniTask MoveCircularUpper(Vector3 destination, float duration, CancellationToken token)
        {
            return Move((sequence, t) =>
            {
                sequence.Append(
                    t.DOMoveX(destination.x, duration)
                    .SetEase(Ease.InSine));
                sequence.Join(
                    t.DOMoveY(destination.y, duration)
                    .SetEase(Ease.OutSine));
            }, token);
        }

        /// <summary>
        /// 　時計回り ◝ ◟
        /// 反時計回り ◞ ◜
        /// </summary>
        private UniTask MoveCircularLower(Vector3 destination, float duration, CancellationToken token)
        {
            return Move((sequence, t) =>
            {
                sequence.Append(
                    t.DOMoveX(destination.x, duration)
                    .SetEase(Ease.OutSine));
                sequence.Join(
                    t.DOMoveY(destination.y, duration)
                    .SetEase(Ease.InSine));
            }, token);
        }

    }
}
