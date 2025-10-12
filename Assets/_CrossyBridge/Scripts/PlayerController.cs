using UnityEngine;
using System.Collections;
using SgLib;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    public static event System.Action PlayerDie = delegate {};

    [Header("Gameplay Config")]
    public float movingSpeed = 13f;
    //Player moving speed
    public float rotatingSpeed = 250f;
    //Player rotating speed
    public float flickerDuration = 1f;
    public float flickerInterval = 0.25f;

    [Header("Object Preferences")]
    public UIManager uIManager;
    public GameManager gameManager;
    public GameObject playerChild;
    public ParticleSystem particle;
    [HideInInspector]
    public Vector3 dir;
    [HideInInspector]
    public bool isRunning;
    public bool isRespawnGoing = false;

    private Vector3 raycastPoint;
    private bool isRotateLeft;
    private bool isRotateForward;
    private float fixDistance;
    private float zPlayerScale;
    private float zPlaneScale;
    private float xPlaneScale;
    private bool enableRespawn;

    void Start()
    {
        EventManager.OnRespawnPerform.AddListener(RespawnPlayer);
        EventManager.OnSetPlayerOnPlane.AddListener(SetPlayerOnPlane);

        enableRespawn = true;
        isRespawnGoing = false;

        // Change the character to the selected one
        GameObject currentCharacter = CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex];
        Character[] gameCars = gameObject.GetComponentsInChildren<Character>(true);

        for (int i = 0; i < CharacterManager.Instance.characters.Length; i++)
        {
            if (i == CharacterManager.Instance.CurrentCharacterIndex)
                gameCars[i].gameObject.SetActive(true);
            else
                gameCars[i].gameObject.SetActive(false);
        }

        var currentCharacterColoring = currentCharacter.GetComponentInChildren<Coloring>().gameObject;

        // окраска автомобиля
        string htmlCarColor = PlayerPrefs.GetString("CarColor" + CharacterManager.Instance.CurrentCharacterIndex.ToString(), "#EEEEEE");
        Color carColor;
        if (!ColorUtility.TryParseHtmlString(htmlCarColor, out carColor))
            carColor = Color.gray;
        currentCharacterColoring.GetComponent<Renderer>().sharedMaterial.SetColor("_Color", carColor);

        Mesh charMesh = currentCharacterColoring.GetComponent<MeshFilter>().sharedMesh;
        Material charMaterial = currentCharacterColoring.GetComponent<Renderer>().sharedMaterial;
        playerChild.GetComponent<MeshFilter>().mesh = charMesh;
        playerChild.GetComponent<MeshRenderer>().material = charMaterial;

        dir = Vector3.forward; //first moving direction
        zPlaneScale = gameManager.normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.z;
        xPlaneScale = gameManager.normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.x;

        fixDistance = ((zPlaneScale / 2) - xPlaneScale) + (xPlaneScale / 2);
        zPlayerScale = playerChild.GetComponent<Renderer>().bounds.size.z;

        var t = transform.position;
        StartCoroutine(MovePlayer());
    }
	
    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameOver)
        {
            // Check game over
            if (transform.rotation == Quaternion.Euler(0, 0, 0))
            {
                raycastPoint = new Vector3(0, 1, -zPlayerScale / 2f - 0.3f);
            }
            else
            {
                raycastPoint = new Vector3(zPlayerScale / 2f + 0.3f, 1, 0);
            }

            Debug.DrawLine(transform.position + raycastPoint, transform.position + raycastPoint + Vector3.down * 5f, Color.green);


            // Ray raydown = new Ray(transform.position + raycastPoint, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(transform.position + raycastPoint, Vector3.down, out hit, 5f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore)) //Still alive
            {
                if (!gameManager.gameOver)
                {
                    if (hit.collider.TryGetComponent<PlaneController>(out PlaneController planeController) && planeController.isTheLastPlane) //This is the last plane, turn right here
                    {
                        // Debug.Log($"PlayerController {planeController}");
                        if (dir == Vector3.forward) //Player moving forward -> turn and rotate left
                        {
                            isRotateForward = false; //Reset

                            if (transform.position.z >= hit.transform.position.z - zPlaneScale && !isRotateLeft) //Rotate left
                            {
                                isRotateLeft = true;
                                StartCoroutine(RotatePlayer(Vector3.down, -90));
                            }

                            if (transform.position.z >= hit.transform.position.z + fixDistance) //change direction
                            {
                                float zAxis = hit.transform.position.z + fixDistance;
                                transform.position = new Vector3(transform.position.x, transform.position.y, zAxis);
                                dir = Vector3.left;
                                hit.collider.GetComponent<PlaneController>().isTheLastPlane = false;
                            }
                        }
                        else //Player moving left -> turn and rotate forward
                        {
                            isRotateLeft = false; //Reset

                            if (transform.position.x <= hit.transform.position.x + zPlaneScale && !isRotateForward)
                            {
                                isRotateForward = true;
                                StartCoroutine(RotatePlayer(Vector3.up, 90)); //Rotate
                            }
                            if (transform.position.x <= hit.transform.position.x - fixDistance) //Change direction
                            {
                                float xAxis = hit.transform.position.x - fixDistance;
                                transform.position = new Vector3(xAxis, transform.position.y, transform.position.z);
                                dir = Vector3.forward;
                                hit.collider.GetComponent<PlaneController>().isTheLastPlane = false;
                            }
                        }
                    }
                }
            }
            else //Die -> game over
            {
                playerChild.GetComponent<Animator>().enabled = false;
                if (gameManager.listIndex < gameManager.listMovingPlane.Count)
                {
                    gameManager.listMovingPlane[gameManager.listIndex].GetComponent<PlaneController>().stopMoving = true;
                }

                isRunning = false;

                if (!gameManager.gameOver)
                {
                    // Debug.Log($"LastPassedNormalPlane = {PlayerPrefs.GetInt("LastPassedNormalPlane")}");
                    Rigidbody rb = playerChild.GetComponent<Rigidbody>();
                    // Debug.Log($"IsKinematic = {rb.isKinematic}");
                    // Debug.Log($"Velocity = {rb.velocity}");
                    if (enableRespawn)
                    {
                        PlayerPrefs.SetInt("LastCarDir", dir == Vector3.forward ? 0 : 1);
                        enableRespawn = !enableRespawn;
                        gameManager.PreGameOver();
                    } else
                    {
                        gameManager.GameOver();
                    }
                }

                // Fall down
                StartCoroutine(CRPlayerFall(0.5f));
            }
        }
    }

    void OnDestroy()
    {
        EventManager.OnRespawnPerform.RemoveListener(RespawnPlayer);
        EventManager.OnSetPlayerOnPlane.RemoveListener(SetPlayerOnPlane);
    }

    IEnumerator MovePlayer()
    {
        while (true)
        {
            if (gameManager.GameState == GameState.Playing)
            {
                isRunning = true;
                while (!gameManager.gameOver)
                {
                    transform.position += dir * movingSpeed * Time.deltaTime;                   
                    yield return null;
                }
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator CRPlayerFall(float delay)
    {
        // Fire event
        // PlayerDie();

        yield return new WaitForSeconds(delay);
        var trans = transform.position;

        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);

        // Fall down
        Rigidbody rb = playerChild.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.down * 13f;
    }

    IEnumerator RotatePlayer(Vector3 dir, float rotateAngle)
    {
       
        float currentAngle = 0;
        while (currentAngle < Mathf.Abs(rotateAngle))
        {
            float rotateAmount = rotatingSpeed * Time.deltaTime;
            currentAngle += rotateAmount;
            transform.Rotate(dir, rotateAmount);
            yield return null;
        }

        if (dir == Vector3.down)
        {
            transform.eulerAngles = new Vector3(0, 270, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

    }

    private void SetPlayerOnPlane()
    {
        gameManager.listIndex++;
        isRespawnGoing = true;
        // получаем последний блок, на котором был пользователь
        int lastPassedNormalPlane = PlayerPrefs.GetInt("LastPassedNormalPlane", 1);
        // получаем список всех NormalPlane, которые у нас есть сейчас
        var normalPlanes = gameManager.GetComponentsInChildren<PlaneController>();
        // выбрасываем из полученной коллекции normalPlanes с порядковым номером -1
        normalPlanes = normalPlanes.Where(np => np.planeOrderNumber != -1).ToArray();
        // получаем NormalPlane с порядковым номером, который больше на 1 того, что мы получили в п. 1
        var normalPlaneForSpawn = normalPlanes.FirstOrDefault(np => np.planeOrderNumber == lastPassedNormalPlane + 1);
        // если дальше только последний блок - берём тот блок, где мы упали
        if (normalPlaneForSpawn == null)
            normalPlaneForSpawn = normalPlanes.FirstOrDefault(np => np.planeOrderNumber == lastPassedNormalPlane);
        // присваиваем transform.position этого блока родительскому компоненту нашей машины
        Vector3 normalPlaneForSpawnPosition = normalPlaneForSpawn.transform.position;
        normalPlaneForSpawnPosition.y = -0.5f;
        // Debug.Log($"normalPlaneForSpawnPosition = {normalPlaneForSpawnPosition}");
        gameObject.transform.position = normalPlaneForSpawnPosition;
        gameObject.transform.rotation = normalPlaneForSpawn.transform.rotation;
        // изменяем rigidbody для машины
        //Rigidbody rb = playerChild.GetComponent<Rigidbody>();
        //rb.isKinematic = true;
        //rb.velocity = new Vector3(0f, 0f, 0f);
        // обнуляем transform.position для нашей машины
        var currentCharacterCar = gameObject.GetComponentInChildren<Character>(includeInactive: false);
        // currentCharacterCar.gameObject.transform.position = new Vector3(0f, 0f, 0f);
        currentCharacterCar.gameObject.transform.position = normalPlaneForSpawnPosition;
        currentCharacterCar.gameObject.transform.rotation = normalPlaneForSpawn.transform.rotation;

        EventManager.OnSetCameraByPlayer.Invoke(currentCharacterCar.gameObject.transform.position);

        StartCoroutine(FlickerCoroutine());
    }

    private IEnumerator FlickerCoroutine()
    {
        GameObject characterCarGO = gameObject.GetComponentInChildren<Character>(includeInactive: false).gameObject;
        float elapsed = 0f;

        while (elapsed < flickerDuration)
        {
            ToggleRenderers(false, characterCarGO);
            yield return new WaitForSeconds(flickerInterval / 2f);

            ToggleRenderers(true, characterCarGO);
            yield return new WaitForSeconds(flickerInterval / 2f);

            elapsed += flickerInterval;
        }

        // Ensure all renderers are re-enabled at the end
        ToggleRenderers(true, characterCarGO);
    }

    private void ToggleRenderers(bool state, GameObject characterCarGO)
    {
        Renderer[] childRenderers = characterCarGO.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in childRenderers)
            renderer.enabled = state;
    }

    private void RespawnPlayer()
    {
        isRespawnGoing = false;
        // uIManager.HideCountdown();
        // переключаем GameState в Playing
        gameManager.ContinueGame();
        dir = PlayerPrefs.GetInt("LastCarDir", 0) == 0 ? Vector3.forward : Vector3.left;
        StartCoroutine(MovePlayer());
    }
}
