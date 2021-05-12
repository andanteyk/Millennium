using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.InGame.Effect;
using Millennium.Mathematics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    public class YuzuGrenade : BulletBase
    {
        [SerializeField]
        private GameObject m_BlastPrefab;

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            Move(token);
            DestroyWhenFrameOut(token);


            this.GetAsyncTriggerEnter2DTrigger()
                .ForEachAsync(collision =>
                {
                    EffectManager.I.Play(EffectOnDestroy, transform.position);

                    var blast = Instantiate(m_BlastPrefab).GetComponent<BulletBase>();
                    blast.transform.position = transform.position + Seiran.Shared.InsideUnitCircle() * 8;
                    blast.Owner = Owner;

                    Destroy(gameObject);
                }, token);
        }
    }
}
