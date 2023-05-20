using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIfps : MonoBehaviour
{
    public int FramesPerSec { get; protected set; }

    [SerializeField] private float frequency = 0.5f;

    public int goodThreshold = 60;
    public int okayThreshold = 30;

    public Color goodColor = Color.green;
    public Color okayColor = Color.yellow;
    public Color badColor = Color.red;


    private Text counter;

    private void Start()
    {
        counter = GetComponent<Text>();
        counter.text = "";
        StartCoroutine(FPS());
    }

    private IEnumerator FPS()
    {
        for (; ; )
        {
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(frequency);

            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;

            FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
            counter.text = "FPS: " + FramesPerSec.ToString();

            if (FramesPerSec >= goodThreshold)
                counter.color = goodColor;
            else if (FramesPerSec >= okayThreshold)
                counter.color = okayColor;
            else
                counter.color = badColor;
        }
    }
}