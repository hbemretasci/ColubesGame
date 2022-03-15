using UnityEngine;

public class CubeController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    private GameState gameStateScript;

    public bool isCubeOnGame;
    public bool isThrowCube;
    public bool isComboCube;
    public bool isCubeOnAir;
    public int comboAmount;

    //player force and speeds
    private const float playerUpForce = 20.0f;
    private const float playerDirectionForce = 2.0f;
    private const float playerTorqueForce = 7.5f;
    private const float cubeStopSpeed = .10f;

    private Vector2 aim;

    private void Start()
    {
        isThrowCube = false;
        isCubeOnGame = false;
        isComboCube = false;
        isCubeOnAir = false;

        gameStateScript = GameObject.Find("Game").GetComponent<GameState>();
        if (gameObject.CompareTag("New")) NewCubeThrow();
    }
    
    private void Update()
    {
        if ((!isCubeOnAir) && (gameObject.transform.position.y > 2.0f)) isCubeOnAir = true;
      
        if (rb.velocity.magnitude < cubeStopSpeed)
        {
            if ((isThrowCube) && (isCubeOnGame)) isThrowCube = false;
            if ((isComboCube) && (gameObject.CompareTag("Player")) && (isCubeOnAir)) isComboCube = false;
        }

        if (gameObject.transform.position.y < -3.0f) gameObject.tag = "GameOver";
    }

    public void SetTrailEffect(bool status)
    {
        TrailRenderer[] trails;

        trails = gameObject.GetComponentsInChildren<TrailRenderer>();
        foreach (TrailRenderer trail in trails)
            trail.emitting = status;
    }

    private void NewCubeThrow()
    {
        float x;
        GameObject targetCube;
        Vector2 targetPosition, sourcePosition;

        int xR = Random.Range(-1, 2);
        int zR = Random.Range(-1, 2);

        targetCube = GameObject.Find(gameObject.name);
        if ((targetCube != gameObject) && (!targetCube.CompareTag("Area")))
        {
            isComboCube = true;
            sourcePosition = new Vector2(transform.position.x, transform.position.z);
            targetPosition = new Vector2(targetCube.transform.position.x, targetCube.transform.position.z);
            aim = targetPosition - sourcePosition;
            rb.AddForce(Vector3.up * playerUpForce, ForceMode.Impulse);
            rb.AddForce(new Vector3(aim.x, 0, aim.y) * playerDirectionForce, ForceMode.Impulse);
        }
        else
        {
            if (transform.position.x > 0) x = -2.5f;
            else x = 2.5f;
            aim = new Vector2(x, 3f);
            rb.AddForce(Vector3.up * playerUpForce, ForceMode.Impulse);
            Invoke("ApplyForce", 0.2f);
        }

        if (gameObject.name == gameStateScript.goalCubeName) gameObject.tag = "LastCube";
        else gameObject.tag = "Player";

        //Apply Torquqe Force
        rb.AddTorque(new Vector3(xR, 0, zR) * playerTorqueForce, ForceMode.Impulse);
    }

    private void ApplyForce()
    {
        rb.AddForce(new Vector3 (aim.x, 0, aim.y) * playerDirectionForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (gameObject.name != collision.gameObject.name)
        {
            if ((isThrowCube) && (collision.gameObject.CompareTag("Player")))
            {
                if (gameStateScript.soundStatus) AudioManager.Instance.PlaySound("Touch");
                isThrowCube = false;
                SetTrailEffect(false);
            }
        }
        else
        {
            gameObject.tag = "Destroyed";
            collision.gameObject.tag = "Destroyed";
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!gameObject.CompareTag("Player")) return;
        if (rb.velocity.magnitude < cubeStopSpeed) gameObject.tag = "GameOver";
    }

    private void OnTriggerExit(Collider other)
    {
        isCubeOnGame = true;
    }
}