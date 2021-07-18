using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RadialButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image circle;
    public Image icon;
    public string title;
    public bool useableByAll;
    public bool useableWhenUnitOnTile;

    public float animSpeed = 10;

    Color defaultColour;

    public RadialMenu menu;

    public void Animate()
    {
        StartCoroutine(AnimateButtonIn());
    }

    IEnumerator AnimateButtonIn()
    {
        transform.localScale = Vector3.zero;

        float timer = 0f;

        while(timer < (1 / animSpeed))
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.one * timer * animSpeed;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        menu.selected = this;

        defaultColour = circle.color;
        float shade = 0.8f;
        circle.color = new Color(circle.color.r * shade, circle.color.g * shade, circle.color.b * shade, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        circle.color = defaultColour;
        menu.selected = null;
    }
}
