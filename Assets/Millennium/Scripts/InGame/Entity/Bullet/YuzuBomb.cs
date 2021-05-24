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
        private async UniTaskVoid Start()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            var colliders = GetComponents<Collider2D>();

            var token = this.GetCancellationTokenOnDestroy();

            DestroyWhenExpired(token).Forget();
            DamageWhenEnter(token).Forget();

            Array.ForEach(colliders, c => c.enabled = false);


            await UniTask.Delay(TimeSpan.FromSeconds(0.25), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

            spriteRenderer.sprite = null;
            Array.ForEach(colliders, c => c.enabled = true);
            EffectManager.I.Play(EffectType.Explosion, transform.position);
            SoundManager.I.PlaySe(SeType.Explosion).Forget();

            await UniTask.DelayFrame(2, PlayerLoopTiming.FixedUpdate, token);

            Array.ForEach(colliders, c => c.enabled = false);
        }

    }
}
