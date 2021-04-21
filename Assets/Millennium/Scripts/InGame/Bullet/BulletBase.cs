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


        async void Start()
        {
            async UniTask OnStart(CancellationToken token)
            {
                var screenArea = new Rect(-256 / 2 - 32, -224 / 2 - 32, 256 + 32 * 2, 224 + 32 * 2);

                while (!token.IsCancellationRequested)
                {
                    transform.position += Speed * Time.deltaTime;

                    if (!screenArea.Contains(transform.position))
                    {
                        Destroy(gameObject);
                        break;
                    }

                    await UniTask.Yield();
                }
            }

            await OnStart(this.GetCancellationTokenOnDestroy());
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            EffectManager.I.Play(EffectOnDestroy, collision.transform.position);
            Destroy(gameObject);
        }
    }
}
