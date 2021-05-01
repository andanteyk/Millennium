using Cysharp.Threading.Tasks;
using Millennium.InGame.Effect;
using Millennium.Sound;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace Millennium.InGame.Entity.Bullet
{
    public abstract class BulletBase : Entity
    {
        [SerializeField]
        private int m_Power = 100;
        public int Power { get => m_Power; protected set => m_Power = value; }

        [SerializeField, FormerlySerializedAs("Speed")]
        private Vector3 m_Speed;
        public Vector3 Speed { get => m_Speed; set => m_Speed = value; }

        public EffectType EffectOnDestroy = EffectType.CrossDecay;


        protected virtual async void Start()
        {
            await OnStart(this.GetCancellationTokenOnDestroy());
        }

        protected async UniTask OnStart(CancellationToken token)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

            while (!token.IsCancellationRequested)
            {
                transform.position += m_Speed * Time.deltaTime;

                if (!InGameConstants.ExtendedFieldArea.Contains(transform.position))
                {
                    Destroy(gameObject);
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }
        }



        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.GetComponent<Entity>() is EntityLiving entity)
            {
                entity.DealDamage(new DamageSource(this, Power));
            }

            EffectManager.I.Play(EffectOnDestroy, transform.position);
            Destroy(gameObject);
        }
    }
}
