using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Stage;
using Millennium.IO;
using Millennium.Sound;
using Millennium.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Millennium.OutGame.Screen
{
    public class PlayerSelect : ScreenBase
    {
        [SerializeField]
        private Button m_MomoiButton;

        [SerializeField]
        private Button m_MidoriButton;

        [SerializeField]
        private Button m_AliceButton;

        private bool m_IsDebugMode = false;


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            SelectFirstButton();

            ButtonAction(m_MomoiButton, StageManager.PlayerType.Momoi, token);
            ButtonAction(m_MidoriButton, StageManager.PlayerType.Midori, token);
            ButtonAction(m_AliceButton, StageManager.PlayerType.Alice, token);

            ListenDebugCommand(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void ButtonAction(Button button, StageManager.PlayerType playerType, CancellationToken token)
        {
            button.OnClickAsAsyncEnumerable(token)
                .Take(1)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    SoundManager.I.PlaySe(SeType.Accept).Forget();

                    var fader = await Fader.CreateFade();
                    DontDestroyOnLoad(fader);
                    fader.SetColor(Color.cyan);
                    await fader.Show();

                    await EntryPoint.StartInGame(new EntryPoint.InGameParams
                    {
                        PlayerType = playerType,
                        IsDebugMode = m_IsDebugMode
                    });

                    await fader.Hide();
                    Destroy(fader.gameObject);

                    // ‚±‚êŽ©g‚ÍƒV[ƒ“Ø‚è‘Ö‚¦‚ÅÁ‚¦‚é‚Í‚¸
                });
        }


        private async UniTaskVoid ListenDebugCommand(CancellationToken token)
        {
            var input = new InputControls();
            input.Enable();

            while (!token.IsCancellationRequested)
            {
                await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().y > 0, cancellationToken: token);

                token.ThrowIfCancellationRequested();


                using var timeoutTokenSource = new CancellationTokenSource();
                using var _ = timeoutTokenSource.CancelAfterSlim(TimeSpan.FromSeconds(3));

                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutTokenSource.Token);
                var linkedToken = linkedTokenSource.Token;

                try
                {
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().y == 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().y > 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().y == 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().y < 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().y == 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().y < 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().y == 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().x < 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().x == 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().x > 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().x == 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().x < 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().x == 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().x > 0, cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Direction.ReadValue<Vector2>().x == 0, cancellationToken: linkedToken);

                    await UniTask.WaitUntil(() => input.Player.Bomb.IsPressed(), cancellationToken: linkedToken);
                    await UniTask.WaitUntil(() => input.Player.Fire.IsPressed(), cancellationToken: linkedToken);
                }
                catch (OperationCanceledException)
                {
                    continue;
                }

                SoundManager.I.PlaySe(SeType.SpreadExplosion).Forget();
                m_IsDebugMode = true;
                return;
            }
        }
    }
}
