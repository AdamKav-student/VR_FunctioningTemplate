using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class Cubespawner : MonoBehaviour
    {
        public GameObject cubePrefab;
        public Button spawnButton;

        private void Start()
        {
            spawnButton.onClick.AddListener(SpawnCube);
        }

        private void SpawnCube()
        {
            Instantiate(cubePrefab, transform.position, Quaternion.identity);
        }
    }
}
