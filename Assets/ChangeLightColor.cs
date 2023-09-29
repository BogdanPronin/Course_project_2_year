using UnityEngine;

public class ChangeLightColor : MonoBehaviour
{
    float color = 0.6f;
    float brightness = 0.2f;

    private void Update() => GetComponent<Light>().color = Color.HSVToRGB(color, brightness, 1.0f);

    public void ChangeColor(float newColor) => color = newColor;
    public void ChangeBrightness(float newBrightness) => brightness = newBrightness;
    public void ChangeAngle(float newAngle) => transform.eulerAngles = new Vector2(transform.eulerAngles.x, newAngle);
}
