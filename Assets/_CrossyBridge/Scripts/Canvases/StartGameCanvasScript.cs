using SgLib;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartGameCanvasScript : MonoBehaviour
{
    public TextMeshProUGUI bestScore;
    public TextMeshProUGUI gold;

    public GameObject currentLevel;
    public TextMeshProUGUI levelNumber;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("MovingPlanesInLevel", -1) == -1)
            currentLevel.SetActive(false);
        else
        {
            levelNumber.text = (PlayerPrefs.GetInt("MovingPlanesInLevel", -1) - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2)).ToString();
            currentLevel.SetActive(true);
        }

        bestScore.text = ScoreManager.Instance.HighScore.ToString();
        gold.text = CoinManager.Instance.Coins.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
