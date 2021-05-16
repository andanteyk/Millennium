using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.InGame.Effect;
using Millennium.Sound;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Millennium.InGame.Entity.Enemy
{
    public abstract class EnemyBase : EntityLiving
    {
        [SerializeField]
        private int m_InitialHealth = 1000;

        public bool CanMove => Health > 0;

        private void Start()
        {
            Health = HealthMax = m_InitialHealth;

            DamageWhenEnter(this.GetCancellationTokenOnDestroy());
        }


        public override void DealDamage(DamageSource damage)
        {
            Health -= damage.Damage;

            var player = damage.Attacker as Player.Player;
            if (player != null)
            {
                player.AddScore(damage.Damage);
            }

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
                if (player != null)
                    player.AddScore(HealthMax);

                EffectManager.I.Play(EffectType.Explosion, transform.position);
                SoundManager.I.PlaySe(SeType.Explosion).Forget();
                Destroy(gameObject);
            }
        }


        protected UniTask DamageWhenEnter(CancellationToken token)
        {
            return this.GetAsyncTriggerEnter2DTrigger()
                .ForEachAsync(collision =>
                {
                    if (collision.gameObject.GetComponent<Entity>() is EntityLiving entity)
                    {
                        entity.DealDamage(new DamageSource(this, 100));
                    }
                }, token);
        }
    }
}
