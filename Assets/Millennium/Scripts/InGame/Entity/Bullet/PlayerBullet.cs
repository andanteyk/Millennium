using Cysharp.Threading.Tasks;

namespace Millennium.InGame.Entity.Bullet
{
    public class PlayerBullet : BulletBase
    {
        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            Move(token);
            DestroyWhenFrameOut(token);
            DamageWhenEnter(token);
            DestroyWhenEnter(token);
        }
    }
}
