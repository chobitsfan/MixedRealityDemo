using UnityEngine;

namespace SparseDesign
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PlayParticleEffect : MonoBehaviour
    {
        [SerializeField] ParticleSystem m_particle = default;
        [SerializeField] bool m_StartStopped = true;

        //Todo: add so that all particle systems in children also can be activated
        void Awake()
        {
            if (!m_particle)
            {
                m_particle = this.GetComponent<ParticleSystem>();
            }
            if (m_StartStopped)
            {
                m_particle.Stop();
            }
        }

        public void Play()
        {
            m_particle.Play();
        }

        public void Play(Vector2 pos)
        {
            m_particle.transform.position = pos;
            this.Play();
        }

        public void Play(Vector3 pos)
        {
            m_particle.transform.position = pos;
            m_particle.Play();
        }

        public static void PlayAll(GameObject obj)
        {
            var playParts = obj.gameObject.GetComponentsInChildren<PlayParticleEffect>();
            for (int i = 0; i < playParts.Length; i++)
            {
                playParts[i].Play();
            }
        }
    }
}
