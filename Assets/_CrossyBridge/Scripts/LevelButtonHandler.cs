using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelButtonHandler : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject levelSelect;

    public void GoToLevel(int level)
    {
        PlayerPrefs.SetInt("MovingPlanesInLevel", level == -1 ? -1 : level + PlayerPrefs.GetInt("DeltaPlatesForLevel", 2));
        string sceneName = "Level" + level.ToString();
        gameManager.SelectLevel(level, true);
        gameManager.RestartGame(0f);
        levelSelect.SetActive(false);
    }

    public void CloseLevelSelect()
    {
        levelSelect.SetActive(false);
    }
}
