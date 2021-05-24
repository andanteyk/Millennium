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
    public class YuzuMissile : BulletBase
    {
        [SerializeField]
        private GameObject m_BlastPrefab;


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            Move(token);
            DamageWhenEnter(token);
            DestroyWhenExpired(token);

            this.OnDestroyAsync().ContinueWith(() =>
            {
                EffectManager.I.Play(EffectOnDestroy, transform.position);

                var blast = BulletBase.Instantiate(m_BlastPrefab, transform.position);
                blast.Owner = Owner;
            });
        }
    }
}
