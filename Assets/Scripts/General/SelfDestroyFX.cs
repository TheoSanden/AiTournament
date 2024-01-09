using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace General
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SelfDestroyFX : MonoBehaviour
    {
        ParticleSystem m_system;

        void Start()
        {
            m_system = GetComponent<ParticleSystem>();
        }

        void Update()
        {
            if (m_system != null && !m_system.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}