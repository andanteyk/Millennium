using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity
{
    public abstract class EntityLiving : Entity
    {
        public int Health { get; protected set; }
        public int HealthMax { get; protected set; }

        public abstract void DealDamage(DamageSource damage);
    }
}