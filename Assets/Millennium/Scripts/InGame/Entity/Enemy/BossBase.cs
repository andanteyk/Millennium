using Cysharp.Threading.Tasks;
using DG.Tweening;
using Millennium.Mathematics;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public abstract class BossBase : EnemyBase
    {


        protected async UniTask RandomMove(float moveSeconds, CancellationToken token)
        {
            var region = new Rect(Mathf.Lerp(InGameConstants.FieldArea.xMin, InGameConstants.FieldArea.xMax, 0.1f), Mathf.Lerp(InGameConstants.FieldArea.yMin, InGameConstants.FieldArea.yMax, 0.625f),
                InGameConstants.FieldArea.width * 0.8f, InGameConstants.FieldArea.height * 0.25f);


            float GetRandomProportion() => (Seiran.Shared.NextSingle(-1, 1) * Seiran.Shared.NextSingle(-1, 1) + 1) / 2;

            var destination = new Vector3(
                region.xMin + GetRandomProportion() * region.width,
                region.yMin + GetRandomProportion() * region.height);

            await transform.DOMove(destination, moveSeconds)
                .SetUpdate(UpdateType.Fixed)
                .SetLink(gameObject)
                .ToUniTask(cancellationToken: token);
        }

    }
}
