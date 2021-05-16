using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.Mathematics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Item
{
    public abstract class ItemBase : Entity
    {
        private bool m_IsAutoCollecting = false;

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            var rigidbody = GetComponent<Rigidbody2D>();

            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ =>
                {
                    var position = transform.position +
                        (m_IsAutoCollecting ?
                            BallisticMath.FromPolar(128, BallisticMath.AimToPlayer(transform.position)) :
                            new Vector3(0, -32))
                        * Time.deltaTime;
                    rigidbody.MovePosition(position);

                    if (position.y < InGameConstants.ExtendedFieldArea.yMin)
                        Destroy(gameObject);
                }, token);

            this.GetAsyncTriggerEnter2DTrigger()
                .ForEachAsync(collision =>
                {
                    if (collision.gameObject.GetComponent<Player.Player>() is Player.Player player)
                    {
                        ApplyItemEffect(player);
                        Destroy(gameObject);
                    }

                }, token);
        }

        public void SetAutoCollect()
        {
            m_IsAutoCollecting = true;
        }


        protected abstract void ApplyItemEffect(Player.Player player);
    }
}
