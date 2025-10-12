using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnCanvasScript : MonoBehaviour
{
    public GameObject respawnComponent;
    public GameObject countdownComponent;

    // Start is called before the first frame update
    void Start()
    {
        respawnComponent.SetActive(true);
        countdownComponent.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        respawnComponent.SetActive(true);
        countdownComponent.SetActive(false);
    }

    public void CallRespawn()
    {
        countdownComponent.SetActive(true);
        respawnComponent.SetActive(false);
    }
}
