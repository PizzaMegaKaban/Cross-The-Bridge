using UnityEngine;
using System.Collections;
using UnityStandardAssets_ImageEffects;

public class CameraController : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerController playerController;

    private Vector3 _cameraPositionDelta = new Vector3(20f, 23f, 28f);

    void Start()
    {
        EventManager.OnSetCameraByPlayer.AddListener(SetCameraByPlayer);
    }

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

    void OnDestroy()
    {
        EventManager.OnSetCameraByPlayer.RemoveListener(SetCameraByPlayer);
    }

    private void SetCameraByPlayer(Vector3 playerPosition)
    {
        transform.position = _cameraPositionDelta + playerPosition;
    }
}