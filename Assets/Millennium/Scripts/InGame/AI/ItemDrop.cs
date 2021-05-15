using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks.Triggers;
using Cysharp.Threading.Tasks;

namespace Millennium.InGame.AI
{
    public class ItemDrop : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_Item;


        private async UniTaskVoid Start()
        {
            await this.OnDestroyAsync();

            Instantiate(m_Item);
            m_Item.transform.position = transform.position;
        }
    }
}
