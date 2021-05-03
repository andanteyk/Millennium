using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.InGame.Effect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    public class PlayerLaser : BulletBase
    {
        public Transform OwnerTransform { get; set; }
        public Vector3 RelativeDistance { get; set; }


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            DestroyWhenFrameOut(token);

            this.GetAsyncTriggerEnter2DTrigger()
                .ForEachAsync(collision =>
                {
                    if (collision.gameObject.GetComponent<Entity>() is EntityLiving entity)
                    {
                        entity.DealDamage(new DamageSource(this, Power));
                    }

                    EffectManager.I.Play(EffectOnDestroy, transform.position);
                }, token);

            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ =>
                {
                    var position = transform.position + Speed * Time.deltaTime;
                    position.x = OwnerTransform.position.x + RelativeDistance.x;
                    transform.position = position;
                }, token);
        }


    }
}