using Cysharp.Threading.Tasks;

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
        }
    }
}
