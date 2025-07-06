using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PlaneController : MonoBehaviour
{
    public bool isTheLastPlane;
    public bool isMove = false;
    public bool isTheTopXAxis;
    public bool isTheTopZAxis;
    public bool movingByXAxis;
    public bool isVisible = false;
    public bool stopMoving = false;
    public float planeMovingSpeed;
    public int movingAmplitude;
    public bool isGameFinishBlock = false;

    bool isDestroying = false;

    // Use this for initialization
    void Start()
    {

        if (isMove)
        {
            StartCoroutine(MovePlane());
        }
    }

    void Update()
    {   
        if (Camera.main.WorldToViewportPoint(transform.position).x < -1.5f && !isDestroying)
        {
            isDestroying = true;
            Destroy(gameObject);
        }
    }

    void OnBecameVisible()
    {
        isVisible = true;
    }

    IEnumerator MovePlane()
    {
        if (movingByXAxis)
        {
            while (!stopMoving)
            {
                Vector3 startPos = transform.position;
                Vector3 endPos;
                if (isTheTopXAxis)
                {
                    endPos = transform.position + new Vector3(-(movingAmplitude * 2), 0, 0);
                }
                else
                {
                    endPos = transform.position + new Vector3((movingAmplitude * 2), 0, 0);
                }
                float t = 0;
                while (t < planeMovingSpeed && !stopMoving)
                {
                    t += Time.deltaTime;
                    float fraction = t / planeMovingSpeed;
                    transform.position = Vector3.Lerp(startPos, endPos, fraction);
                    yield return null;
                }

                isTheTopXAxis = !isTheTopXAxis;               
            }
        }
        else
        {
            while (!stopMoving)
            {
                Vector3 startPos = transform.position;
                Vector3 endPos;
                if (isTheTopZAxis)
                {
                    endPos = transform.position + new Vector3(0, 0, -(movingAmplitude * 2));
                }
                else
                {
                    endPos = transform.position + new Vector3(0, 0, (movingAmplitude * 2));
                }
                float t = 0;
                while (t < planeMovingSpeed && !stopMoving)
                {
                    t += Time.deltaTime;
                    float fraction = t / planeMovingSpeed;
                    transform.position = Vector3.Lerp(startPos, endPos, fraction);
                    yield return null;
                }

                isTheTopZAxis = !isTheTopZAxis;
            }         
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGameFinishBlock && other.GetComponent<Character>() != null)
        {
            int finishedMovingPlanes = PlayerPrefs.GetInt("MovingPlanesInLevel", -1);
            int finishedLevel = finishedMovingPlanes == -1 ? finishedMovingPlanes : (finishedMovingPlanes - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2)) + 1;
            if (finishedLevel != -1 && finishedLevel > PlayerPrefs.GetInt("LastUnlockedLevel", 1))
                PlayerPrefs.SetInt("LastUnlockedLevel", finishedLevel);
            EventManager.OnLevelFinished.Invoke();
        }
    }
}
