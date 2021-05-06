using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    [RequireComponent(typeof(BulletBase))]
    public class BulletAccel : MonoBehaviour
    {
        public Vector3 Accel;
        public float MinSpeed = -1;
        public float MaxSpeed;

        private BulletBase m_Bullet;


        // TODO
    }
}