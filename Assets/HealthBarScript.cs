using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public float MaxHealth = 10;
    public float Health = 10;
    public Slider slider;
    public float Height = 0.1f;
    public float Width = 1f;
    public void UpdateValues()
    {
        slider.maxValue = MaxHealth;
        slider.value = Health;
        slider.minValue = 0;
        Rect temp = GetComponent<RectTransform>().rect;
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Width);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Height);
        Debug.Log("updated");
    }
}
