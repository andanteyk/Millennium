using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
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

            UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ =>
                {
                    spriteRenderer.sprite = null;
                    Array.ForEach(colliders, c =>
                    {
                        c.enabled = true;
                        // to activate colliders
                        c.transform.DOShakePosition(1f).SetLink(c.gameObject).WithCancellation(token);
                    });

                    EffectManager.I.Play(EffectType.Explosion, transform.position);
                    SoundManager.I.PlaySe(SeType.Explosion).Forget();
                }, token);
        }

    }
}
