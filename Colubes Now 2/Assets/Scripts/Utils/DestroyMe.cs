using UnityEngine;

public class DestroyMe : MonoBehaviour
{

    void Start()
    {
        Invoke("FreeMe", 3.0f); ;
    }

    private void FreeMe()
    {
        Destroy(gameObject);
    }

}
