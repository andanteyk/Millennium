using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        // Start is called before the first frame update
        async void Start()
        {

        }

        // TEST
        private void OnTriggerEnter2D(Collider2D collision)
        {
            Destroy(gameObject);
        }
    }
}