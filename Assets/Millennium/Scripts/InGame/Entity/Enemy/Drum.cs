using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public class Drum : EnemyBase
    {
        [SerializeField]
        private GameObject m_BlastPrefab;

        public override void DealDamage(DamageSource damage)
        {
            base.DealDamage(damage);

            if (Health <= 0)
            {
                var blast = Instantiate(m_BlastPrefab);
                blast.transform.position = transform.position;
            }
        }
    }
}
