using Cysharp.Threading.Tasks;
using Millennium.InGame.Entity.Bullet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Player
{
    public class PlayerMomoi : Player
    {
        protected override UniTask SubShot()
        {
            for (int i = -2; i <= 2; i++)
            {
                if (i == 0)
                    continue;

                float angle = 15f * i;
                var quaternion = Quaternion.AngleAxis(angle, Vector3.forward);

                var shot = Instantiate(m_SubShotPrefab);
                shot.transform.position = transform.position;
                shot.transform.rotation = quaternion;

                var bullet = shot.GetComponent<BulletBase>();
                bullet.Speed = quaternion * bullet.Speed;
            }
            return UniTask.CompletedTask;
        }
    }
}
