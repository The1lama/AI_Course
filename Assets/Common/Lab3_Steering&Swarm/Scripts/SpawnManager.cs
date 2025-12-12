using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Common.Lab3_Steering_Swarm.Scripts
{
    public class SpawnManager : MonoBehaviour
    {

        [Header("Spawn")]
        public GameObject agentPrefab;
        public int spawnAmount = 10;
        public Vector2 spawnAreaSize = new Vector2(10, 10);


        private void OnEnable()
        {
            for (int i = 0; i < spawnAmount; i++)
            {
                var x = transform.position.x + Random.Range(-spawnAreaSize.x/2, spawnAreaSize.y/2);
                var y = transform.position.z + Random.Range(-spawnAreaSize.x/2, spawnAreaSize.y/2);
                Instantiate(agentPrefab,new Vector3(x, transform.position.y, y), Quaternion.identity, transform);
            }
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            var size = new Vector3(spawnAreaSize.x, 0, spawnAreaSize.y)*2;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
