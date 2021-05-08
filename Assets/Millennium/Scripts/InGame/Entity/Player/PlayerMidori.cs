using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Player
{
    public class PlayerMidori : Player
    {
        private GameObject GetNearestEnemy()
        {
            // –ˆƒtƒŒ[ƒ€‚¶‚á‚È‚¢‚©‚ç‹–‚µ‚Ä
            var enemies = GameObject.FindGameObjectsWithTag(InGameConstants.EnemyTag);
            return enemies.Aggregate((GameObject)null, (current, next) => ((current != null ? Vector3.Distance(transform.position, current.transform.position) : float.PositiveInfinity) > Vector3.Distance(transform.position, next.transform.position) ? next : current));
        }


        protected override UniTask SubShot()
        {
            var nearest = GetNearestEnemy();

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


        protected override async UniTask FireBomb(CancellationToken token)
        {
            MoveSpeedModifier = 0.25f;

            EffectManager.I.Play(EffectType.Concentration, transform.position).SetParent(transform);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);


            await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.2), PlayerLoopTiming.FixedUpdate)
                .Take(15)
                .ForEachAsync(_ =>
                {
                    var nearest = GetNearestEnemy();
                    float baseRadian = nearest != null ? Mathf.Atan2(nearest.transform.position.y - transform.position.y, nearest.transform.position.x - transform.position.x) : (Mathf.PI / 2);

                    foreach (var r in BallisticMath.CalculateWayRadians(baseRadian + Seiran.Shared.NextSingle(-5, 5) * Mathf.Deg2Rad, 3, 15 * Mathf.Deg2Rad))
                    {
                        var bullet = BulletBase.Instantiate(m_BombPrefab, transform.position, BallisticMath.FromPolar(512, r));
                        bullet.transform.rotation = Quaternion.AngleAxis(r * Mathf.Rad2Deg - 90, Vector3.forward);
                    }

                    EffectManager.I.Play(EffectType.PlusDecayGreen, transform.position);
                    SoundManager.I.PlaySe(SeType.MomoiMidoriBomb).Forget();
                }, token);

            MoveSpeedModifier = 1;
        }
    }
}
