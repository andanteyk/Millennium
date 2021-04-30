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
        public struct Waypoint
        {
            public Vector2 Point;
            public float Duration;
        }

        [SerializeField]
        private Waypoint[] m_Waypoints;

        private async void Start()
        {
            var owner = GetComponent<EnemyBase>();

            var sequence = DOTween.Sequence();
            foreach (var waypoint in m_Waypoints)
            {
                // TODO: discard ‚É‚·‚×‚«H
                _ = sequence.Append(transform.DOLocalMove(waypoint.Point, waypoint.Duration).SetEase(Ease.InOutQuad));
            }
            _ = sequence.AppendCallback(() => Destroy(gameObject));

            await sequence.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
}
