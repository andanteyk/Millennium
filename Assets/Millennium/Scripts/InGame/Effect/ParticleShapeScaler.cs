using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Millennium.InGame.Effect
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleShapeScaler : MonoBehaviour
    {
        [SerializeField]
        private float m_StartSeconds = 0;

        [SerializeField]
        private float m_EndSeconds = 3;

        [SerializeField]
        private float m_EndValue = 4;

        [SerializeField]
        private Ease m_Ease = Ease.InQuad;


        private void Start()
        {
            var particle = GetComponent<ParticleSystem>();
            var shape = particle.shape;

            var sequence = DOTween.Sequence();
            sequence.AppendInterval(m_StartSeconds);
            sequence.Append(DOTween.To(() => shape.scale, value => shape.scale = value,
                new Vector3(m_EndValue, m_EndValue, 1), m_EndSeconds - m_StartSeconds)
                .SetEase(m_Ease)
                .SetLink(gameObject));
        }
    }
}
