using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    public class PlayerBomb : BulletBase
    {
        protected async override void Start()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();

            await UniTask.WhenAll(
                OnStart(this.GetCancellationTokenOnDestroy()),
                transform.DOScale(4, 0.5f).WithCancellation(this.GetCancellationTokenOnDestroy()),
                UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ => { spriteRenderer.flipX = Time.time % 0.5f < 0.25f; }, this.GetCancellationTokenOnDestroy()));
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            // NOP 

            // TODO: お行儀がよくないのでは？？？
        }

        protected void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.GetComponent<Entity>() is EntityLiving entity)
            {
                entity.DealDamage(new DamageSource(this, Power));
            }
        }
    }
}
