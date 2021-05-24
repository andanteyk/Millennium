using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.Mathematics;
using System;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    public abstract class BulletBase : Entity
    {
        [SerializeField]
        private int m_Power = 100;
        public int Power { get => m_Power; protected set => m_Power = value; }

        [SerializeField]
        private Vector3 m_Speed;
        public Vector3 Speed { get => m_Speed; set => m_Speed = value; }

        [SerializeField]
        private float m_LifeTime = 20;

        public EffectType EffectOnDestroy = EffectType.CrossDecay;


        public Entity Owner;


        protected UniTask Move(CancellationToken token)
        {
            return UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ => transform.position += m_Speed * Time.deltaTime, token);
        }

        protected UniTask DestroyWhenExpired(CancellationToken token)
        {
            return UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(m_LifeTime), PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ => Destroy(gameObject), token);
        }

        protected UniTask DestroyWhenFrameOut(CancellationToken token)
        {
            return UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .Where(_ => !InGameConstants.ExtendedFieldArea.Contains(transform.position))
                .ForEachAsync(_ => Destroy(gameObject), token);
        }

        protected UniTask DamageWhenEnter(CancellationToken token)
        {
            return this.GetAsyncTriggerEnter2DTrigger()
                .ForEachAsync(collision =>
                {
                    if (collision.gameObject.GetComponent<Entity>() is EntityLiving entity)
                    {
                        entity.DealDamage(new DamageSource(Owner != null ? Owner : this, Power));
                    }
                }, token);
        }

        protected UniTask DamageWhenStay(CancellationToken token)
        {
            return this.GetAsyncTriggerStay2DTrigger()
                .ForEachAsync(collision =>
                {
                    if (collision.gameObject.GetComponent<Entity>() is EntityLiving entity)
                    {
                        entity.DealDamage(new DamageSource(Owner != null ? Owner : this, Power));
                    }

                    EffectManager.I.Play(EffectOnDestroy, collision.transform.position);
                }, token);
        }

        protected UniTask CollisionSwitcher(float intervalSeconds, int activeFrames, CancellationToken token)
        {
            var colliders = GetComponentsInChildren<Collider2D>();
            return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds), PlayerLoopTiming.FixedUpdate)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    Array.ForEach(colliders, c => c.enabled = true);

                    await UniTask.DelayFrame(activeFrames, PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                    Array.ForEach(colliders, c => c.enabled = false);
                }, token);
        }


        protected UniTask DestroyWhenEnter(CancellationToken token)
        {
            return this.GetAsyncTriggerEnter2DTrigger()
                .ForEachAsync(collision =>
                {
                    EffectManager.I.Play(EffectOnDestroy, transform.position);
                    Destroy(gameObject);
                }, token);
        }



        public DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> DOSpeed(Vector3 endValue, float duration)
            => DOTween.To(() => Speed, value => Speed = value, endValue, duration)
                .SetEase(Ease.Linear)
                .SetLink(gameObject);

        // DOAngularVelocity ÇæÇ∆ angular velocity ÇïœçXÇ∑ÇÈÇÊÇ§Ç…å©Ç¶ÇÈÇÃÇ≈Åc
        public DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> DOSpeedAngle(float baseSpeed, float startValue, float endValue, float duration)
           => DOTween.To(() => startValue, value => Speed = BallisticMath.FromPolar(baseSpeed, value), endValue, duration)
               .SetEase(Ease.Linear)
               .SetLink(gameObject);


        public static BulletBase Instantiate(GameObject prefab, Vector3 position, Vector3 speed)
        {
            var instance = Instantiate(prefab);
            instance.transform.position = position;

            var bullet = instance.GetComponent<BulletBase>();
            bullet.Speed = speed;

            return bullet;
        }

        public static BulletBase Instantiate(GameObject prefab, Vector3 position)
        {
            var instance = Instantiate(prefab);
            instance.transform.position = position;

            return instance.GetComponent<BulletBase>();
        }
    }
}
