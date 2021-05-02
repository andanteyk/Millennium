using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Entity.Bullet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Player
{
    public class PlayerAlice : Player
    {
        protected override UniTask SubShot()
        {
            for (int i = 0; i < 2; i++)
            {
                var relative = new Vector3(32 * (i - 0.5f), 0);

                var bullet = Instantiate(m_SubShotPrefab);
                bullet.transform.position = transform.position + relative;

                var laser = bullet.GetComponent<PlayerLaser>();
                laser.OwnerTransform = transform;
                laser.RelativeDistance = relative;
            }

            return UniTask.CompletedTask;
        }
    }
}