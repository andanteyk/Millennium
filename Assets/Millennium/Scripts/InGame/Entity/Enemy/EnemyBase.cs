using Cysharp.Threading.Tasks;
using Millennium.InGame.Effect;
using Millennium.Sound;
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
        }


        public override void DealDamage(DamageSource damage)
        {
            Health -= damage.Damage;

            if (Health <= 0)
            {
                EffectManager.I.Play(EffectType.Explosion, transform.position);
                SoundManager.I.PlaySe(SeType.Explosion).Forget();
                Destroy(gameObject);
            }
        }
    }
}
