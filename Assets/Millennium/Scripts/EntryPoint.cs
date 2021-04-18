using Cysharp.Threading.Tasks;
using Millennium.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium
{

    public class EntryPoint : MonoBehaviour
    {
        async void Start()
        {
            var input = new InputControls();
            input.Enable();

            while (true)
            {
                if (input.Player.Submit.triggered)
                {
                    Debug.Log("ok");
                }
                var direction = input.Player.Direction.ReadValue<Vector2>();
                if (direction.sqrMagnitude != 0)
                    Debug.Log($"x: {direction.x} / y: {direction.y}");
                await UniTask.Yield();
            }
        }
    }

}
