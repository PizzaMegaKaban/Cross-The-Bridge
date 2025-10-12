using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    private GameObject _activeCanvas;

    // Start is called before the first frame update
    void Awake()
    {
        EventManager.OnNewCanvasOpening.AddListener(OpenNewCanvas);

        DontDestroyOnLoad(this.gameObject);
    }

    public void OpenNewCanvas(GameObject newCanvas)
    {
        newCanvas.SetActive(true);
        if (_activeCanvas != null)
        {
            _activeCanvas.SetActive(false);
        }
        _activeCanvas = newCanvas;
        // Debug.Log($"New active canvas name = {_activeCanvas.name}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
