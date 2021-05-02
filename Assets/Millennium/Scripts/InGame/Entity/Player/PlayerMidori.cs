using Cysharp.Threading.Tasks;
using Millennium.InGame.Entity.Bullet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Millennium.InGame.Entity.Player
{
    public class PlayerMidori : Player
    {
        protected override UniTask SubShot()
        {
            // –ˆƒtƒŒ[ƒ€‚¶‚á‚È‚¢‚©‚ç‹–‚µ‚Ä
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            var nearest = enemies.Aggregate((GameObject)null, (current, next) => ((current != null ? Vector3.Distance(transform.position, current.transform.position) : float.PositiveInfinity) > Vector3.Distance(transform.position, next.transform.position) ? next : current));

            for (int i = 0; i < 2; i++)
            {
                var position = transform.position + new Vector3(32 * (i - 0.5f), 0);

                var shot = Instantiate(m_SubShotPrefab);
                shot.transform.position = position;

                var quaternion = Quaternion.AngleAxis(
                        nearest != null ?
                        Mathf.Atan2(nearest.transform.position.y - position.y, nearest.transform.position.x - position.x) * Mathf.Rad2Deg - 90f :
                        0f, Vector3.forward);

                shot.transform.rotation = quaternion;

                var bullet = shot.GetComponent<BulletBase>();
                bullet.Speed = quaternion * bullet.Speed;
            }
            return UniTask.CompletedTask;
        }
    }
}
