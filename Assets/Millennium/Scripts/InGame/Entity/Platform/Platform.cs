using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.InGame.Entity.Bullet;
using Millennium.Sound;

namespace Millennium.InGame.Entity.Platform
{
    public class Platform : Entity
    {
        private void Start()
        {
            this.GetAsyncTriggerEnter2DTrigger()
                .ForEachAsync(collision =>
                {
                    if (collision.gameObject.GetComponent<BulletBase>() != null)
                    {
                        SoundManager.I.PlaySe(SeType.PlayerBulletImmune).Forget();
                    }
                }, this.GetCancellationTokenOnDestroy());
        }
    }
}
