using UnityEngine;
using System.Collections;
using UnityStandardAssets_ImageEffects;

public class CameraController : MonoBehaviour
{

    public GameManager gameManager;
    public PlayerController playerController;

    // Update is called once per frame
    void Update()
    {

        if (!gameManager.gameOver && playerController.isRunning)
        {
            if (playerController.dir == Vector3.left)
            {
                transform.position += new Vector3(-playerController.movingSpeed * Time.deltaTime, 0, 0);
            }
            else
            {
                transform.position += new Vector3(0, 0, playerController.movingSpeed * Time.deltaTime);
            }
        }
    }
}
