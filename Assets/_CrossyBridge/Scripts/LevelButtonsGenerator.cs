using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelButtonsGenerator : MonoBehaviour
{
    public GameObject buttonPrefab;        // Assign a 3D button prefab (e.g., Cube with TextMesh)
    public GameObject unableButtonPrefab;
    public Transform buttonParent;         // Assign an empty GameObject in the scene
    public int numberOfLevels;
    public int currentLevel;
    public float spacing = 2f;
    public LevelButtonHandler levelButtonHandler;

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("LastUnlockedLevel", 1);

        for (int i = 1; i <= numberOfLevels; i++)
        {
            int levelIndex = i;
            Vector3 position = new Vector3(0, -i * spacing, 0);

            if (levelIndex > currentLevel)
            {
                GameObject ugo = Instantiate(unableButtonPrefab, buttonParent);
                ugo.transform.localPosition = position;

                Button ubtn = ugo.GetComponent<Button>();
                ubtn.interactable = false;

                continue;
            }

            GameObject go = Instantiate(buttonPrefab, buttonParent);
            go.transform.localPosition = position;

            TextMeshProUGUI tm = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tm != null)
                tm.text = levelIndex.ToString();

            Button btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => levelButtonHandler.GoToLevel(levelIndex));
        }
    }
}