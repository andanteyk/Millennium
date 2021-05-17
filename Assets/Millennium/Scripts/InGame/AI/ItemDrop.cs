using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks.Triggers;
using Cysharp.Threading.Tasks;
using Millennium.InGame.Entity;

namespace Millennium.InGame.AI
{
    [RequireComponent(typeof(EntityLiving))]
    public class ItemDrop : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_Item;

        private async UniTaskVoid Start()
        {
            var entity = GetComponent<EntityLiving>();
            var token = this.GetCancellationTokenOnDestroy();

            await UniTask.WaitUntil(() => entity.Health <= 0, PlayerLoopTiming.FixedUpdate, cancellationToken: token);
            token.ThrowIfCancellationRequested();

            Instantiate(m_Item);
            m_Item.transform.position = transform.position;
        }
    }
}
