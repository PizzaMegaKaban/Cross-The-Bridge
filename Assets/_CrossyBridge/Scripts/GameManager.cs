using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SgLib;

#if EASY_MOBILE
//using EasyMobile;
#endif

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    PreGameOver,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static event System.Action<GameState, GameState> GameStateChanged = delegate { };

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }

    private GameState _gameState = GameState.Prepare;

    [Header("Check to enable premium features (require EasyMobile plugin)")]
    public bool enablePremiumFeatures = true;

    [Header("Gameplay Config")]
    public int initialPlanes = 5;
    //How many plane you create when start game
    public int totalPlaneOnScene = 9;
    //How many plane you have on scene
    public int minPlaneNumber = 8;
    //Min plane's number of path
    public int maxPlaneNumber = 10;
    //Max plane's number of path
    public int maxFluctuationRange = 6;
    //Max fluctuation range of plane
    public int minFluctuationRange = 3;
    //Min fluctuation range of plane
    public float minPlaneSpeed = 0.45f;
    //Min plane speed
    public float maxPlaneSpeed = 0.65f;
    //Max plane speed
    public float minDeviation = 0.3f;
    //Min deviation when moving plane stop at the same position with plane ahead
    public int bridgeNumber = 5;
    /* 5 brigdes first, you will have 1 moving plane, 5 brigdes next, you will have 2 moving plane........
    , moving plane will be plus 1 everytime you cross 5 brigdes*/
    public float firstMovingPlaneFrequency = 0.9f;
    [Range(0f, 1f)]
    public float movingPlaneFrequency;
    //Probability to create moving plane
    public float amplitudeDecreases = 0.1f;
    public float limitMovingPlaneFrequency = 0.5f;
    [Range(0f, 1f)]
    public float goldFrequency;

    [Header("Object Preferences")]
    public PlayerController playerController;
    public UIManager uIManager;
    public GameObject normalMovingPlanePrefab;
    public GameObject winterMovingPlanePrefab;
    public GameObject firstPlane;
    public GameObject normalSummerPlanePrefab;
    public GameObject lastForwardSummerPlanePrefab;
    public GameObject lastLeftSummerPlanePrefab;
    public GameObject normalWinterPlanePrefab;
    public GameObject lastForwardWinterPlanePrefab;
    public GameObject lastLeftWinterPlanePrefab;
    public GameObject snowParticle;
    public GameObject goldPrefab;
    [HideInInspector]
    public List<GameObject> listMovingPlane = new List<GameObject>();
    [HideInInspector]
    public int listIndex = 0;
    [HideInInspector]
    public bool gameOver = false;

    private GameObject normalPlane;
    private GameObject lastForwardPlane;
    private GameObject lastLeftPlane;
    private GameObject movingPlane;

    private GameObject currentPlane;
    private Vector3 planePosition;
    private Vector3 forwardDirection = Vector3.forward;
    private Vector3 leftDirection = Vector3.left;
    private bool firstPlaneOnForwardIsCreated = false;
    private bool firstPlaneOnRightIsCreated = false;
    private float checkPosition;
    private float xPlaneScale;
    private float yPlaneScale;
    private float zPlaneScale;
    private int planeNumber;
    private int countPlane = 0;
    private int turn = 1;
    private int countMovingPlane = 0;
 

    // Use this for initialization
    void Start()
    {
        GameState = GameState.Prepare;

        //PlayerPrefs.DeleteAll();
        xPlaneScale = Mathf.Round(normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.x);
        yPlaneScale = Mathf.Round(normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.y);
        zPlaneScale = Mathf.Round(normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.z);

        //Random plane's type
        RandomPlaneType();

        //Create position for next plane
        planePosition = firstPlane.transform.position + forwardDirection * zPlaneScale;

        //Change first plane 
        firstPlane.GetComponent<MeshFilter>().sharedMesh = normalPlane.GetComponent<MeshFilter>().sharedMesh;
        firstPlane.GetComponent<Renderer>().sharedMaterial = normalPlane.GetComponent<Renderer>().sharedMaterial;

        //Set parent
        firstPlane.transform.SetParent(transform);

        //reset score and create plane
        ScoreManager.Instance.Reset();

        for (int i = 0; i < initialPlanes; i++)
        {
            currentPlane = (GameObject)Instantiate(normalPlane, planePosition, Quaternion.Euler(0, 0, 0));
            planePosition = currentPlane.transform.position + forwardDirection * zPlaneScale;
            currentPlane.transform.SetParent(transform);
        }

        Vector3 planeBehindPosition = firstPlane.transform.position + Vector3.back * zPlaneScale;
        for (int i = 0; i < 3; i++)
        {
            GameObject planeBehind = Instantiate(normalPlane, planeBehindPosition, Quaternion.Euler(0, 0, 0)) as GameObject;
            planeBehind.transform.SetParent(transform);
            planeBehindPosition = planeBehind.transform.position + Vector3.back * zPlaneScale;
        }
       
        planeNumber = Random.Range(minPlaneNumber, maxPlaneNumber); //Create plane number for path
     
        firstPlaneOnForwardIsCreated = true;
        movingPlaneFrequency = firstMovingPlaneFrequency;

        StartCoroutine(CreatePlane());

        SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
    }
	
    // Update is called once per frame
    void Update()
    {
        // Exit on Android Back button
        #if UNITY_ANDROID && EASY_MOBILE
        if (Input.GetKeyUp(KeyCode.Escape))
        {   

            NativeUI.AlertPopup alert = NativeUI.ShowTwoButtonAlert(
                                      "Exit Game",
                                      "Are you sure you want to exit?",
                                      "Yes", 
                                      "No");

            if (alert != null)
            {
                alert.OnComplete += (int button) =>
                {
                    switch (button)
                    {
                        case 0: // Yes
                            Application.Quit();
                            break;
                        case 1: // No
                            break;
                    }
                };
            }     
        }
        #endif

        if (playerController.isRunning && !gameOver) //Not game over
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (listIndex < listMovingPlane.Count) //Make sure the the listIndex not run out of the list
                {
                    if (listMovingPlane[listIndex].GetComponent<PlaneController>().isVisible) //This moving plane is visible
                    {
                        listMovingPlane[listIndex].GetComponent<PlaneController>().stopMoving = true; //Stop moving plane

                        GameObject currentPlane = listMovingPlane[listIndex];

                        Vector3 point = new Vector3(0, yPlaneScale / 2, 0); //Draw raycast from this point

                        if (currentPlane.transform.rotation == Quaternion.Euler(0, -90, 0))
                        {
                            Ray rayRight = new Ray(currentPlane.transform.position + point, Vector3.right);
                            RaycastHit hit;
                            if (Physics.Raycast(rayRight, out hit, zPlaneScale)) //Draw raycast with length is zPlaneScale
                            {
                                PlaneController planeController = hit.collider.GetComponent<PlaneController>();

                                if (planeController != null)
                                {
                                    if (planeController.isMove) //This plane is normal plane
                                    {
                                        checkPosition = hit.transform.position.z; //Remember z position of this plane 
                                    }
                                }
                                

                                float distance = Mathf.Abs(currentPlane.transform.position.z - checkPosition);

                                if (distance <= minDeviation)//distance is less than minDeviation -> bonus coin
                                {
                                    currentPlane.transform.position = new Vector3(currentPlane.transform.position.x,
                                        currentPlane.transform.position.y,
                                        checkPosition);

                                    CreateGold(currentPlane, 1); //Bonus coin

                                    ScoreManager.Instance.AddScore(2); // Bonus score

                                    SoundManager.Instance.PlaySound(SoundManager.Instance.placeUp);
                                }
                                else
                                {
                                    SoundManager.Instance.PlaySound(SoundManager.Instance.place);
                                }
                            }
                        }
                        else
                        {
                            Ray rayBack = new Ray(currentPlane.transform.position + point, Vector3.back);
                            RaycastHit hit;
                            if (Physics.Raycast(rayBack, out hit, zPlaneScale))
                            {
                                PlaneController planeController = hit.collider.GetComponent<PlaneController>();
                                if (planeController != null)
                                {
                                    if (!planeController.isMove) //This is normal plane
                                    {
                                        checkPosition = hit.transform.position.x; //Remember x position of this plane
                                    }
                                }
                                

                                float distance = Mathf.Abs(currentPlane.transform.position.x - checkPosition);
                                if (distance <= minDeviation)//distance is less than minDeviation -> bonus coin
                                {
                                    currentPlane.transform.position = new Vector3(checkPosition,
                                        currentPlane.transform.position.y,
                                        currentPlane.transform.position.z);

                                    CreateGold(currentPlane, 1); //Bonus coin

                                    ScoreManager.Instance.AddScore(2); // Bonus score

                                    SoundManager.Instance.PlaySound(SoundManager.Instance.placeUp);
                                }
                                else
                                {
                                    SoundManager.Instance.PlaySound(SoundManager.Instance.place);
                                }
                            }
                        }

                        listIndex++; //Next moving plane
                    }
                }            
            }
        }
    }

    public void StartGame()
    {
        GameState = GameState.Playing;
    }

    public void GameOver()
    {
        gameOver = true;
        GameState = GameState.GameOver;

        SoundManager.Instance.StopMusic();
    }

    public void RestartGame(float delay)
    {
        StartCoroutine(CRRestart(delay));
    }

    IEnumerator CRRestart(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator CreatePlane()
    {
        while (!gameOver)
        {
            if (transform.childCount < totalPlaneOnScene)
            {
                countPlane++;

                if (turn > 0) //Create plane on forward side
                {
                    //Create first plane of path
                    firstPlaneOnRightIsCreated = false;//Reset 

                    if (!firstPlaneOnForwardIsCreated) //If firstPlaneOnForwardSide isn't created
                    {
                        firstPlaneOnForwardIsCreated = true;

                        float fixDistance = Mathf.Abs(zPlaneScale - xPlaneScale) / 2;
                        Vector3 fixPosition = new Vector3(-fixDistance, 0, fixDistance);

                        //Create the first plane of this path//Create position
                        planePosition = (currentPlane.transform.position + fixPosition) + forwardDirection * xPlaneScale;
                        currentPlane = (GameObject)Instantiate(normalPlane, planePosition, Quaternion.Euler(0, 0, 0));//Create the first plane of this path


                        CreateGold(currentPlane, goldFrequency);

                        //Create position for next plane
                        planePosition = currentPlane.transform.position + forwardDirection * zPlaneScale;

                        currentPlane.transform.SetParent(transform);  
                    }
                    else //First plane is created
                    {
                        if (countPlane == planeNumber)//This is the last plane of this path , the player change direction right here
                        {
                            currentPlane = (GameObject)Instantiate(lastForwardPlane, planePosition, Quaternion.Euler(0, 0, 0));
                            currentPlane.GetComponent<PlaneController>().isTheLastPlane = true;

                            currentPlane.transform.SetParent(transform);
                            ResetCountAndPlaneNumber();//Reset count , create new plane number for next path

                            CreateGold(currentPlane, goldFrequency);

                        }
                        else //This is not last plane of this path, caculate and generate normal plane or moving plane 
                        {
                            GeneratePlane(true);
                        }
                    }
                }
                //// New path
                else //Create plane of left side
                {
                    //Create first plane for this path
                    firstPlaneOnForwardIsCreated = false;
                    if (!firstPlaneOnRightIsCreated)
                    {
                        firstPlaneOnRightIsCreated = true;//First plane is created

                        float fixDistance = Mathf.Abs(zPlaneScale - xPlaneScale) / 2;
                        Vector3 fixPosition = new Vector3(-fixDistance, 0, fixDistance);

                        //Create the first plane of this path
                        planePosition = (currentPlane.transform.position + fixPosition) + leftDirection * xPlaneScale;
                        currentPlane = (GameObject)Instantiate(normalPlane, planePosition, Quaternion.Euler(0, 90, 0)); //Create the first plane of this path

                        CreateGold(currentPlane, goldFrequency);

                        //Create pisition for next plane
                        planePosition = currentPlane.transform.position + leftDirection * zPlaneScale;

                        currentPlane.transform.SetParent(transform);
                    }
                    else //First plane is created
                    {
                        if (countPlane == planeNumber) //This is the last plane of this path, the player change direction right here
                        {
                            currentPlane = (GameObject)Instantiate(lastLeftPlane, planePosition, Quaternion.Euler(0, 90, 0)); //Create plane 
                            currentPlane.GetComponent<PlaneController>().isTheLastPlane = true;

                            CreateGold(currentPlane, goldFrequency);

                            ResetCountAndPlaneNumber(); //Reset count , create new plane number for next path

                            currentPlane.transform.SetParent(transform);
                        }
                        else //This is not last plane of this path,caculate and generate normal plane or moving plane 
                        {
                            GeneratePlane(false);                           
                        }
                    }
                }
            }
            yield return null;
        }
    }


    void ResetCountAndPlaneNumber()
    {
        //Reset count
        countPlane = 0;
        turn = turn * (-1); //Change direction to create plane

        //Create plane's number for next path
        planeNumber = Random.Range(minPlaneNumber, maxPlaneNumber);
    }

    void GeneratePlane(bool isForwardSide)
    {
        float movingPlaneProbability = Random.Range(0f, 1f);
        if (movingPlaneProbability <= movingPlaneFrequency && countPlane != 0 && countPlane % 2 == 0) //Create moving plane
        {           
            //How many moving plane is created 
            int movingPlaneNumber = (countMovingPlane / bridgeNumber) + 1;

            ConfigMovingPlaneAppearanceProbability(countMovingPlane);

            countMovingPlane++;

            for (int i = 0; i < movingPlaneNumber; i++)
            {
                int movingLength = Random.Range(minFluctuationRange, maxFluctuationRange); //Create fluctuation range of plane
                float indexPisitionMovingPlane = Random.Range(0f, 1f);


                if (isForwardSide)
                {
                    currentPlane = (GameObject)Instantiate(movingPlane, planePosition, Quaternion.Euler(0, 0, 0));
                    planePosition = currentPlane.transform.position + forwardDirection * zPlaneScale;
                    PlaneController currentPlaneController = currentPlane.GetComponent<PlaneController>();


                    if (indexPisitionMovingPlane < 0.5f)
                    {
                        currentPlane.transform.position += new Vector3(movingLength, 0, 0);
                        currentPlaneController.isTheTopXAxis = true;
                    }
                    else
                    {
                        currentPlane.transform.position += new Vector3(-movingLength, 0, 0);
                        currentPlaneController.isTheTopXAxis = false;
                    }

                    currentPlaneController.movingByXAxis = true;
                    currentPlaneController.planeMovingSpeed = Random.Range(minPlaneSpeed, maxPlaneSpeed);
                    currentPlaneController.movingAmplitude = movingLength;
                    currentPlaneController.isMove = true;

                    listMovingPlane.Add(currentPlane);
                }
                else
                {
                    currentPlane = (GameObject)Instantiate(movingPlane, planePosition, Quaternion.Euler(0, -90, 0)); //Create plane                           
                    planePosition = currentPlane.transform.position + leftDirection * zPlaneScale;//Create position for next plane
                    PlaneController currentPlaneController = currentPlane.GetComponent<PlaneController>();

                    if (indexPisitionMovingPlane < 0.5f)
                    {
                        currentPlane.transform.position += new Vector3(0, 0, movingLength);
                        currentPlaneController.isTheTopZAxis = true;
                    }
                    else
                    {
                        currentPlane.transform.position += new Vector3(0, 0, -movingLength);
                        currentPlaneController.isTheTopZAxis = false;
                    }

                    currentPlaneController.movingByXAxis = false;
                    currentPlaneController.planeMovingSpeed = Random.Range(minPlaneSpeed, maxPlaneSpeed);
                    currentPlaneController.movingAmplitude = movingLength;
                    currentPlaneController.isMove = true;

                    listMovingPlane.Add(currentPlane);
                }
            }
        }
        else //Create normal plane
        {
            if (isForwardSide)
            {
                currentPlane = (GameObject)Instantiate(normalPlane, planePosition, Quaternion.Euler(0, 0, 0));
                planePosition = currentPlane.transform.position + forwardDirection * zPlaneScale;
            }
            else
            {
                currentPlane = (GameObject)Instantiate(normalPlane, planePosition, Quaternion.Euler(0, 90, 0));
                planePosition = currentPlane.transform.position + leftDirection * zPlaneScale;
            }

            currentPlane.transform.SetParent(transform);
            CreateGold(currentPlane, goldFrequency);
        }
    }


    void RandomPlaneType()
    {
        if (Random.value <= 0.5f) //Summer plane
        {
            normalPlane = normalSummerPlanePrefab;
            lastForwardPlane = lastForwardSummerPlanePrefab;
            lastLeftPlane = lastLeftSummerPlanePrefab;
            movingPlane = normalMovingPlanePrefab;

            // Disable snow particle
            snowParticle.SetActive(false);
        }
        else //Winter plane
        {
            normalPlane = normalWinterPlanePrefab;
            lastForwardPlane = lastForwardWinterPlanePrefab;
            lastLeftPlane = lastLeftWinterPlanePrefab;
            movingPlane = winterMovingPlanePrefab;

            // Enable snow
            snowParticle.SetActive(true);
        }
    }

    void CreateGold(GameObject plane, float frequency)
    {
        if (Random.value <= frequency)
        {
            Vector3 goldPos = new Vector3(plane.transform.position.x, -0.5f, plane.transform.position.z);
            Instantiate(goldPrefab, goldPos, Quaternion.identity);
        }
    }


    void ConfigMovingPlaneAppearanceProbability(int countMovingPlane)
    {
        if (movingPlaneFrequency <= limitMovingPlaneFrequency)
        {
            movingPlaneFrequency = limitMovingPlaneFrequency;
        }
        else
        {
            if (countMovingPlane % bridgeNumber == 0 && countMovingPlane != 0)
            {
                movingPlaneFrequency -= amplitudeDecreases;
            }
        }
    }
}
