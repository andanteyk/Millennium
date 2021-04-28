using Cysharp.Threading.Tasks;
using Millennium.InGame.Effect;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Bullet
{
    public abstract class BulletBase : MonoBehaviour
    {
        [SerializeField]
        private int m_Power = 100;
        public int Power { get => m_Power; protected set => m_Power = value; }

        public Vector3 Speed;

        public EffectType EffectOnDestroy = EffectType.CrossDecay;


        protected virtual async void Start()
        {
            await OnStart(this.GetCancellationTokenOnDestroy());
        }

        protected async UniTask OnStart(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                transform.position += Speed * Time.deltaTime;

                if (!InGameConstants.ExtendedFieldArea.Contains(transform.position))
                {
                    Destroy(gameObject);
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }
        }



        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            EffectManager.I.Play(EffectOnDestroy, transform.position);
            Destroy(gameObject);
        }
    }
}
