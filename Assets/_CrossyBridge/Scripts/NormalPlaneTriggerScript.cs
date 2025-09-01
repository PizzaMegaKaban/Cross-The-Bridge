using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalPlaneTriggerScript : MonoBehaviour
{
    private PlaneController _normalPlaneController;
    [SerializeField] private string targetTag = "Player"; // ��� ������������ �������
    private Collider triggerCollider;
    private bool blockLevelCompeleted = false;

    void Start()
    {
        _normalPlaneController = gameObject.transform.parent.gameObject.GetComponent<PlaneController>();

        if (_normalPlaneController.isGameFinishBlock)
        {
            triggerCollider = gameObject.transform.parent.gameObject.GetComponent<Collider>();
            blockLevelCompeleted = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Character>() != null && !_normalPlaneController.isMove &&
            !_normalPlaneController.isGameFinishBlock && _normalPlaneController.planeOrderNumber != -1)
            PlayerPrefs.SetInt("LastPassedNormalPlane", _normalPlaneController.planeOrderNumber);
    }

    private void OnTriggerStay(Collider other)
    {
        if (blockLevelCompeleted) return;

        if (!_normalPlaneController.isGameFinishBlock) return;

        if (!other.CompareTag(targetTag)) return;

        Collider targetCollider = other;
        Bounds containerBounds = triggerCollider.bounds;
        Bounds targetBounds = targetCollider.bounds;

        if (containerBounds.min.x < targetBounds.min.x && containerBounds.min.z < targetBounds.min.z &&
            containerBounds.max.x > targetBounds.max.x && containerBounds.max.z > targetBounds.max.z)
        {
            blockLevelCompeleted = true;
            int finishedMovingPlanes = PlayerPrefs.GetInt("MovingPlanesInLevel", -1);
            int finishedLevel = finishedMovingPlanes == -1 ? finishedMovingPlanes : (finishedMovingPlanes - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2)) + 1;
            if (finishedLevel != -1 && finishedLevel > PlayerPrefs.GetInt("LastUnlockedLevel", 1))
                PlayerPrefs.SetInt("LastUnlockedLevel", finishedLevel);
            EventManager.OnLevelFinished.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            var planeController = gameObject.transform.parent.gameObject.GetComponent<PlaneController>();
            if (planeController.progressNumber == -1) return;
            EventManager.OnPlaneTrigger.Invoke(planeController.progressNumber);
        }
    }
}
