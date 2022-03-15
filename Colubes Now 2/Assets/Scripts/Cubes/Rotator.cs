using UnityEngine;

public class Rotator : MonoBehaviour
{
    void Start()
    {
        Destroy(GetComponent<CubeController>());
        Destroy(GetComponent<Rigidbody>());

        transform.localScale *= 1.5f;
    }

    void Update()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }
}
