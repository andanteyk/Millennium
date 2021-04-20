using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Bullet
{
    public abstract class BulletBase : MonoBehaviour
    {
        public Vector3 Speed;

        async void Start()
        {
            var screenArea = new Rect(-256 / 2 - 32, -224 / 2 - 32, 256 + 32 * 2, 224 + 32 * 2);

            while (true)
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Destroy(gameObject);
        }
    }
}
