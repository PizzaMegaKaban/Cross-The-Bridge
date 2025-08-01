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
    public int deltaPlatesForLevel = 2;
    // public int movingPlaneNumberInLevel = 5;


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
    public Material backgroundMaterial;

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
    private int movingPlaneNumberInLevel = -1;
    private bool movingPlanesLimitReached = false;
    private bool stopBlock = false;

    // Use this for initialization
    void Start()
    {
        EventManager.OnLevelFinished.AddListener(GameOver);
        EventManager.BlockStopClick.AddListener(BlockStopped);

        PlayerPrefs.SetInt("DeltaPlatesForLevel", deltaPlatesForLevel);
        movingPlaneNumberInLevel = PlayerPrefs.GetInt("MovingPlanesInLevel", -1);
        SelectLevel(movingPlaneNumberInLevel);
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

    private void BlockStopped()
    {
        stopBlock = true;
        Debug.Log("GameManager - Canvas was clicked!");
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
            // TODO срабатывается при просто нажатии на мышку
            // if (Input.GetMouseButtonDown(0))
            if (stopBlock)
            {
                stopBlock = !stopBlock;
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

    private void OnDestroy()
    {
        EventManager.OnLevelFinished.RemoveListener(GameOver);
        EventManager.BlockStopClick.RemoveListener(BlockStopped);
    }

    public void StartGame()
    {
        SelectLevel(PlayerPrefs.GetInt("MovingPlanesInLevel", -1));
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
            if (transform.childCount < totalPlaneOnScene && !movingPlanesLimitReached)
            // if (transform.childCount < totalPlaneOnScene)
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
        if (movingPlaneProbability <= movingPlaneFrequency && countPlane != 0 && countPlane % 2 == 0 && 
            ((movingPlaneNumberInLevel != -1 && countMovingPlane < movingPlaneNumberInLevel) || movingPlaneNumberInLevel == -1)) //Create moving plane
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
        else if (movingPlaneNumberInLevel != -1 && countMovingPlane >= movingPlaneNumberInLevel)
        {
            var planeQuaternion = isForwardSide ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, -90, 0);

            currentPlane = (GameObject)Instantiate(normalPlane, planePosition, planeQuaternion);
            planePosition = currentPlane.transform.position + forwardDirection * zPlaneScale;

            currentPlane.GetComponent<PlaneController>().isGameFinishBlock = true;
            currentPlane.GetComponent<BoxCollider>().isTrigger = true;
            movingPlanesLimitReached = true;
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

    public void SelectLevel(int level = -1, bool roughtLevelNumber = false)
    {
        int selectedLevel = roughtLevelNumber ? level : (level == -1 ? level : level - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2));

        (Color top, Color bottom) = GetGradientColorByLevel(selectedLevel);

        backgroundMaterial.SetColor("_TopColor", top);
        backgroundMaterial.SetColor("_BottomColor", bottom);
    }

    private (Color topColor, Color bottomColor) GetGradientColorByLevel(int level = -1)
    {
        if (level == -1)
        {
            ColorUtility.TryParseHtmlString("#009CFF", out Color topColor);
            ColorUtility.TryParseHtmlString("#39B4FF", out Color bottomColor);
            return (topColor, bottomColor);
        } else
        {
            int firstDigit = level / 10;
            Color topColor = GetGolorByNumber(firstDigit);

            int secondDigit = level % 10;
            Color bottomColor = GetGolorByNumber(secondDigit);

            return (topColor, bottomColor);
        }
    }

    private Color GetGolorByNumber(int number)
    {
        switch (number)
        {
            //orange state
            case 0:
                ColorUtility.TryParseHtmlString("#FFA300", out Color orange2Color);
                return orange2Color;
            case 1:
                ColorUtility.TryParseHtmlString("#FFB600", out Color orange3Color);
                return orange3Color;
                        
            //yellow state
            case 2:
                ColorUtility.TryParseHtmlString("#FFCF00", out Color yellow2Color);
                return yellow2Color;
            case 3:
                ColorUtility.TryParseHtmlString("#FFDD00", out Color yellow3Color);
                return yellow3Color;
            case 4:
                ColorUtility.TryParseHtmlString("#FFE800", out Color yellow4Color);
                return yellow4Color;
            case 5:
                ColorUtility.TryParseHtmlString("#FFEF00", out Color yellow5Color);
                return yellow5Color;
            case 6:
                ColorUtility.TryParseHtmlString("#FFF900", out Color yellow6Color);
                return yellow6Color;
            case 7:
                ColorUtility.TryParseHtmlString("#FFFF00", out Color yellow7Color);
                return yellow7Color;
            case 8:
                ColorUtility.TryParseHtmlString("#F2FF00", out Color yellow8Color);
                return yellow8Color;
            
            //green state
            case 9:
                ColorUtility.TryParseHtmlString("#E1FF00", out Color green1Color);
                return green1Color;
            case 10:
                ColorUtility.TryParseHtmlString("#C8FF00", out Color green2Color);
                return green2Color;
            case 11:
                ColorUtility.TryParseHtmlString("#B3FF00", out Color green3Color);
                return green3Color;
            case 12:
                ColorUtility.TryParseHtmlString("#9BFF00", out Color green4Color);
                return green4Color;
            case 13:
                ColorUtility.TryParseHtmlString("#81FF00", out Color green5Color);
                return green5Color;
            case 14:
                ColorUtility.TryParseHtmlString("#65FF00", out Color green6Color);
                return green6Color;
            case 15:
                ColorUtility.TryParseHtmlString("#36FF00", out Color green9Color);
                return green9Color;
            case 16:
                ColorUtility.TryParseHtmlString("#09FF00", out Color green11Color);
                return green11Color;
            case 17:
                ColorUtility.TryParseHtmlString("#00FF53", out Color green14Color);
                return green14Color;

            //blue state
            case 18:
                ColorUtility.TryParseHtmlString("#00FF86", out Color blue1Color);
                return blue1Color;
            case 19:
                ColorUtility.TryParseHtmlString("#00FFAA", out Color blue2Color);
                return blue2Color;
            case 20:
                ColorUtility.TryParseHtmlString("#00FFC4", out Color blue3Color);
                return blue3Color;
            case 21:
                ColorUtility.TryParseHtmlString("#00FFD7", out Color blue4Color);
                return blue4Color;
            case 22:
                ColorUtility.TryParseHtmlString("#00FFF3", out Color blue5Color);
                return blue5Color;
            case 23:
                ColorUtility.TryParseHtmlString("#00F5FF", out Color blue6Color);
                return blue6Color;
            case 24:
                ColorUtility.TryParseHtmlString("#00E6FF", out Color blue7Color);
                return blue7Color;
            case 25:
                ColorUtility.TryParseHtmlString("#00DAFF", out Color blue8Color);
                return blue8Color;
            case 26:
                ColorUtility.TryParseHtmlString("#00CAFF", out Color blue9Color);
                return blue9Color;
            case 27:
                ColorUtility.TryParseHtmlString("#00B3FF", out Color blue10Color);
                return blue10Color;
            case 28:
                ColorUtility.TryParseHtmlString("#009BFF", out Color blue11Color);
                return blue11Color;
            case 29:
                ColorUtility.TryParseHtmlString("#008DFF", out Color blue12Color);
                return blue12Color;
            case 30:
                ColorUtility.TryParseHtmlString("#0078FF", out Color blue13Color);
                return blue13Color;
            case 31:
                ColorUtility.TryParseHtmlString("#0067FF", out Color blue14Color);
                return blue14Color;
            case 32:
                ColorUtility.TryParseHtmlString("#004CFF", out Color blue15Color);
                return blue15Color;
            case 33:
                ColorUtility.TryParseHtmlString("#003FFF", out Color blue16Color);
                return blue16Color;
            case 34:
                ColorUtility.TryParseHtmlString("#0028FF", out Color blue17Color);
                return blue17Color;
            case 35:
                ColorUtility.TryParseHtmlString("#0013FF", out Color blue18Color);
                return blue18Color;
            case 36:
                ColorUtility.TryParseHtmlString("#2700FF", out Color blue19Color);
                return blue19Color;
            
            //purple state
            case 37:
                ColorUtility.TryParseHtmlString("#3D00FF", out Color purple1Color);
                return purple1Color;
            case 38:
                ColorUtility.TryParseHtmlString("#5200FF", out Color purple2Color);
                return purple2Color;
            case 39:
                ColorUtility.TryParseHtmlString("#6C00FF", out Color purple3Color);
                return purple3Color;
            case 40:
                ColorUtility.TryParseHtmlString("#8400FF", out Color purple4Color);
                return purple4Color;
            case 41:
                ColorUtility.TryParseHtmlString("#A200FF", out Color purple5Color);
                return purple5Color;
            
            //pink state
            case 42:
                ColorUtility.TryParseHtmlString("#B700FF", out Color pink1Color);
                return pink1Color;
            case 43:
                ColorUtility.TryParseHtmlString("#D100FF", out Color pink2Color);
                return pink2Color;
            case 44:
                ColorUtility.TryParseHtmlString("#ED00FF", out Color pink3Color);
                return pink3Color;
            case 45:
                ColorUtility.TryParseHtmlString("#FF00FA", out Color pink4Color);
                return pink4Color;
            case 46:
                ColorUtility.TryParseHtmlString("#FF00C4", out Color pink5Color);
                return pink5Color;

            //red state
            case 47:
                ColorUtility.TryParseHtmlString("#FF008F", out Color red1Color);
                return red1Color;
            case 48:
                ColorUtility.TryParseHtmlString("#FF008F", out Color red2Color);
                return red2Color;
            case 49:
                ColorUtility.TryParseHtmlString("#FF0017", out Color red3Color);
                return red3Color;

            default:
                return Color.white;
        }
    }
}
