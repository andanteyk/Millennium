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
                    if (collision.gameObject.GetComponent<Entity>() is EntityLiving entity)
                    {
                        entity.DealDamage(new DamageSource(Owner != null ? Owner : this, Power));
                    }

                    var blast = Instantiate(m_BlastPrefab, transform.position + new Vector3(0, 4) + Seiran.Shared.InsideUnitCircle() * 8);
                    blast.Owner = Owner;

                    EffectManager.I.Play(EffectOnDestroy, transform.position);
                    Destroy(gameObject);
                }, token);
        }
    }
}
