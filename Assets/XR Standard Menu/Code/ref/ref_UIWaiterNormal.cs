using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ref_UIWaiterNormal : MonoBehaviour
{
    public TMP_Text title, body;
    public Slider progress;
    public void SetTitle(string title)
    {
        this.title.text = title;
    }
    public void SetBody(string body)
    {
        this.body.text = body;
    }
    public void Progress(float a, float b)
    {
        progress.value = a / b;
    }
}
