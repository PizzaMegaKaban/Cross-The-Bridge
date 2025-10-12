using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayCanvasScript : MonoBehaviour
{
    public Text score;
    public Text gold;

    public GameObject currentLevel;
    public TextMeshProUGUI levelNumber;

    public GameObject levelTimer;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("MovingPlanesInLevel", -1) == -1)
        {
            currentLevel.SetActive(false);
            levelTimer.SetActive(false);
        }
        else
        {
            levelNumber.text = (PlayerPrefs.GetInt("MovingPlanesInLevel", -1) - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2)).ToString();
            currentLevel.SetActive(true);
            levelTimer.SetActive(true);
        }

        score.text = "0";
        gold.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        gold.text = PlayerPrefs.GetInt("CurrentGameCoins", 0).ToString();
        score.text = PlayerPrefs.GetInt("CurrentGameScore", 0).ToString();
    }
}
