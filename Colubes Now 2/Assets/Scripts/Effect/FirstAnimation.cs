using UnityEngine;

public class FirstAnimation : MonoBehaviour
{
    public SpriteRenderer second;
    private ChangeBackground changeBackgroundScript;

    private void Start()
    {
        changeBackgroundScript = GameObject.Find("Background").GetComponent<ChangeBackground>();
    }

    private void OnFadeOutComplete()
    {
        changeBackgroundScript.firstBackground += 2;
        if (changeBackgroundScript.firstBackground == 8) changeBackgroundScript.firstBackground = 0;
        gameObject.GetComponent<SpriteRenderer>().sprite = changeBackgroundScript.backgroundImages[changeBackgroundScript.firstBackground];
    }

    private void OnFadeInComplete()
    {
        changeBackgroundScript.secondBackground += 2;
        if (changeBackgroundScript.secondBackground == 9) changeBackgroundScript.secondBackground = 1;
        second.sprite = changeBackgroundScript.backgroundImages[changeBackgroundScript.secondBackground];
    }


}
