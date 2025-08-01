using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalPlaneTriggerScript : MonoBehaviour
{
    private PlaneController _normalPlaneController;
    [SerializeField] private string targetTag = "Player"; // Тег проверяемого объекта
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

        // ВАРИАНТ 1

        Collider targetCollider = other;
        Bounds containerBounds = triggerCollider.bounds;
        Bounds targetBounds = targetCollider.bounds;

        Debug.Log($"Триггер: targetBounds.min = {targetBounds.min}, targetBounds.max = {targetBounds.max}, containerBounds = {containerBounds}");

        if (containerBounds.min.x < targetBounds.min.x && containerBounds.min.z < targetBounds.min.z &&
            containerBounds.max.x > targetBounds.max.x && containerBounds.max.z > targetBounds.max.z)
        {
            Debug.Log("Пересечение!");
            blockLevelCompeleted = true;
            int finishedMovingPlanes = PlayerPrefs.GetInt("MovingPlanesInLevel", -1);
            int finishedLevel = finishedMovingPlanes == -1 ? finishedMovingPlanes : (finishedMovingPlanes - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2)) + 1;
            if (finishedLevel != -1 && finishedLevel > PlayerPrefs.GetInt("LastUnlockedLevel", 1))
                PlayerPrefs.SetInt("LastUnlockedLevel", finishedLevel);
            EventManager.OnLevelFinished.Invoke();
        }



        // ВАРИАНТ 2

        // if (!other.CompareTag(targetTag)) return;

        // Получаем все коллайдеры цели (вдруг их несколько)
        //Collider[] targetColliders = other.GetComponentsInChildren<Collider>();
        //bool anyOutside = false;

        //foreach (var target in targetColliders)
        //{
        //    if (!Physics.ComputePenetration(
        //        target, target.transform.position, target.transform.rotation,
        //        triggerCollider, triggerCollider.transform.position, triggerCollider.transform.rotation,
        //        out Vector3 dir, out float distance))
        //    {
        //        anyOutside = true;
        //        break; // Как только один не пересекается — значит не полностью внутри
        //    }
        //}

        //if (!anyOutside)
        //{
        //    Debug.Log("Объект полностью внутри коллайдера с учётом поворота!");
        //    blockLevelCompeleted = true;
        //    int finishedMovingPlanes = PlayerPrefs.GetInt("MovingPlanesInLevel", -1);
        //    int finishedLevel = finishedMovingPlanes == -1 ? finishedMovingPlanes : (finishedMovingPlanes - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2)) + 1;
        //    if (finishedLevel != -1 && finishedLevel > PlayerPrefs.GetInt("LastUnlockedLevel", 1))
        //        PlayerPrefs.SetInt("LastUnlockedLevel", finishedLevel);
        //    EventManager.OnLevelFinished.Invoke();
        //}



        // ВАРИАНТ 3


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
