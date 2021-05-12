using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Item
{
    public class ItemUltimateAccelerant : ItemBase
    {
        [SerializeField]
        private int m_ChargeValue = 50000;

        protected override void ApplyItemEffect(Player.Player player)
        {
            player.AddSkillPoint(m_ChargeValue);
        }
    }
}
