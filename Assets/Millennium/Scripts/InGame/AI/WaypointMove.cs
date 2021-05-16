using Cysharp.Threading.Tasks;
using DG.Tweening;
using Millennium.InGame.Entity.Enemy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.AI
{
    [RequireComponent(typeof(EnemyBase))]
    public class WaypointMove : MonoBehaviour
    {
        [Serializable]
        public class Waypoint
        {
            public Vector2 Point;
            public float Duration;
            public Ease Ease = Ease.InOutQuad;
        }

        [SerializeField]
        private Waypoint[] m_Waypoints;

        private void Start()
        {
            var owner = GetComponent<EnemyBase>();

            var sequence = DOTween.Sequence()
                .SetUpdate(UpdateType.Fixed)
                .SetLink(gameObject);
            foreach (var waypoint in m_Waypoints)
            {
                sequence.Append(transform.DOLocalMove(waypoint.Point, waypoint.Duration)
                   .SetEase(waypoint.Ease)
                   .SetRelative());
            }
            sequence.AppendCallback(() => Destroy(gameObject));

            sequence.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
