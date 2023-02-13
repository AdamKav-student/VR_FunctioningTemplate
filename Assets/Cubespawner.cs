using UnityEngine;
using UnityEngine.UI;

public class Cubespawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    
    public Gameobject cubePrefab;
    public Button spawnButton;
    
    

private void Start()
    {

        spawnButton.onClick.AddListener(SpawnCube);

    }

    private void SpawnCube()    
    {
        Instantiate(cubePrefab,transform.position, Quaternion.identity);   
    }

}

