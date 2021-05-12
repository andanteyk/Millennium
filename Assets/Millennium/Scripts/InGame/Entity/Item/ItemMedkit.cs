using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.Mathematics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Item
{
    public class ItemMedkit : ItemBase
    {
        protected override void ApplyItemEffect(Player.Player player)
        {
            player.Extend();
        }
    }
}
