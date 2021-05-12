using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.Sound;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Millennium.InGame.Entity.Bullet
{
    public class YuzuBomb : BulletBase
    {
        private void Start()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            var colliders = GetComponents<Collider2D>();

            var token = this.GetCancellationTokenOnDestroy();

            DestroyWhenExpired(token);
            DamageWhenEnter(token);

            Array.ForEach(colliders, c => c.enabled = false);

            UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ =>
                {
                    spriteRenderer.sprite = null;
                    Array.ForEach(colliders, c => c.enabled = true);

                    EffectManager.I.Play(EffectType.Explosion, transform.position);
                    SoundManager.I.PlaySe(SeType.Explosion).Forget();
                }, token);
        }

    }
}
