using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    /// <summary>
    /// �A���X�̃T�u�V���b�g���[�U�[ (�挩�̖����Ȃ��̂Ŗ��O���Ђǂ�)
    /// </summary>
    public class PlayerLaser : BulletBase
    {
        public Transform OwnerTransform { get; set; }
        public Vector3 RelativeDistance { get; set; }


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            DestroyWhenExpired(token);
            DamageWhenStay(token);
            CollisionSwitcher(0.5f, 1, token);

            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ =>
                {
                    if (OwnerTransform != null)
                    {
                        transform.position = OwnerTransform.position + RelativeDistance;
                    }
                }, token);
        }
    }
}
