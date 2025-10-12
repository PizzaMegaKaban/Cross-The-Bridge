using SgLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCanvasScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponentInChildren<RewardUIController>().Reward(PlayerPrefs.GetInt("Reward", 0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
