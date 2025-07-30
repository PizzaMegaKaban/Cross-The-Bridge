using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public Text countdownText;
    public float startTime = 10f;

    private void Start()
    {
        StartCoroutine(Countdown(startTime));
    }

    private IEnumerator Countdown(float time)
    {
        EventManager.OnSetPlayerOnPlane.Invoke();
        while (time > 0)
        {
            countdownText.text = Mathf.Ceil(time).ToString();
            yield return new WaitForSeconds(1f);
            time -= 1f;
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);
        EventManager.OnRespawnPerform.Invoke();
    }
}
