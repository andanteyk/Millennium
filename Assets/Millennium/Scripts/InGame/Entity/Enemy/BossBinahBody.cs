using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{

    public class BossBinahBody : EnemyBase
    {
        [SerializeField]
        private BossBinah m_Parent;


        public override void DealDamage(DamageSource damage)
        {
            m_Parent.DealDamage(damage);
        }
    }
}
