using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Entity.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.AI
{
    [RequireComponent(typeof(EnemyBase))]
    public class FallDownUpMove : MonoBehaviour
    {
        [SerializeField]
        private float m_StaySeconds = 5;

        [SerializeField]
        private float m_Distance = 100;

        // Start is called before the first frame update
        private async void Start()
        {
            var owner = GetComponent<EnemyBase>();

            await DOTween.Sequence()
                .Append(transform.DOLocalMoveY(-m_Distance, m_StaySeconds / 4))
                .AppendInterval(m_StaySeconds)
                .Append(transform.DOLocalMoveY(m_Distance, m_StaySeconds / 4).SetEase(Ease.InQuad))
                .AppendCallback(() => Destroy(gameObject))
                .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
        }


    }
}