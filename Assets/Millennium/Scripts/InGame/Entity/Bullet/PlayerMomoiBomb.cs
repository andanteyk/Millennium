using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.InGame.Effect;

namespace Millennium.InGame.Entity.Bullet
{
    public class PlayerMomoiBomb : BulletBase
    {
        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            Move(token);
            DestroyWhenExpired(token);
            DamageWhenEnter(token);

            this.GetAsyncTriggerEnter2DTrigger()
                .ForEachAsync(collision =>
                {
                    if (collision.gameObject.GetComponent<Entity>() is EntityLiving entity)
                    {
                        EffectManager.I.Play(EffectType.Hit, collision.transform.position);
                    }
                }, token);
        }
    }
}
