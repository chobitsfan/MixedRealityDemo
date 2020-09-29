using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparseDesign
{
    namespace ControlledFlight
    {
        [AddComponentMenu("Controlled Flight/Flock")]

        public class Flock : MonoBehaviour
        {
            [Tooltip("The game object that the flock will follow.\nCan be stationary or moving.")]
            [SerializeField] public GameObject m_origin = default;

            [Tooltip("A list of objects with MissileSupervisor component (typicall prefabs) that are spawned in the flock.\nEach flock member will be chosen at random from the list.")]
            public List<MissileSupervisor> m_birdPrefabs = new List<MissileSupervisor>();

            [Tooltip("Flock Size.")]
            [SerializeField] public int m_flockSize = 0;

            [Tooltip("Spawn flock when Start() is called.\nIf deactivated Spawnflock() can be called to spawn flock.")]
            [SerializeField] public bool m_autoSpawn = true;

            [Tooltip("The resulting flock members.")]
            [SerializeField] public List<GameObject> m_flockMembers = new List<GameObject>();

            void Start()
            {
                m_flockMembers.Clear();
                if (m_autoSpawn) SpawnFlock();
            }

            /// <summary>
            /// Spawns another flock according to settings. Can be called more than once.
            /// </summary>
            public void SpawnFlock()
            {
                if (m_origin) SpawnFlock(m_origin, m_flockSize);
            }

            private void SpawnFlock(GameObject origin, int flockSize)
            {
                if (m_birdPrefabs.Count < 1) return;

                for (int i = 0; i < flockSize; i++)
                {
                    GameObject prefab = CollectionTools.GetRandomFromList(m_birdPrefabs).gameObject;

                    //Spawn randomly around origin 
                    float dist = 5f;
                    Vector3 pos = origin.transform.position + dist * Random.insideUnitSphere;
                    GameObject newObj = Instantiate(prefab, pos, prefab.transform.rotation);
                    newObj.SetActive(true);

                    MissileSupervisor missileSupervisor = newObj.GetComponent<MissileSupervisor>();//Always available

                    missileSupervisor.m_guidanceSettings.m_targetType = MissileGuidance.TargetType.TARGET;
                    missileSupervisor.m_guidanceSettings.m_target = origin;
                    missileSupervisor.m_guidanceSettings.m_guidanceType = MissileGuidance.GuidanceType.PROPORTIONALNAVIGATION;

                    //Random acceleration limit to give somewhat random movement within flock
                    missileSupervisor.m_guidanceSettings.m_maxAcceleration *= Random.Range(0.8f, 1f / 0.8f);

                    //Launch in random directions
                    missileSupervisor.m_launchType = MissileSupervisor.LaunchType.CUSTOMDIRECTION;
                    missileSupervisor.m_launchCustomDir = Random.onUnitSphere;

                    missileSupervisor.m_autoLaunch = false;
                    missileSupervisor.StartLaunchSequence();

                    m_flockMembers.Add(newObj);
                }
            }

            /// <summary>
            /// Destroy all members in flock.
            /// </summary>
            public void DestroyFlock()
            {
                foreach (var obj in m_flockMembers)
                {
                    MonoBehaviour.Destroy(obj);
                }
                m_flockMembers.Clear();
            }
        }
    }
}