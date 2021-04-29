using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Millennium.InGame.Entity
{
    public class DamageSource
    {
        public readonly Entity Attacker;
        public readonly int Damage;

        public DamageSource(Entity attacker, int damage)
        {
            Attacker = attacker;
            Damage = damage;
        }
    }
}
