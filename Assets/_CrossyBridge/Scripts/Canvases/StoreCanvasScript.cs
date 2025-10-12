using SgLib;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoreCanvasScript : MonoBehaviour
{
    public TextMeshProUGUI totalCoinsText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        totalCoinsText.text = CoinManager.Instance.Coins.ToString();
    }
}
