using UnityEngine;

public class ChangeBackground : MonoBehaviour
{
    public Sprite[] backgroundImages;
    public int firstBackground;
    public int secondBackground;
    public Animator animator;
    public SpriteRenderer first;
    public SpriteRenderer second;

    private bool isFirst;

    public void Init()
    {
        isFirst = true;
        firstBackground = 0;
        secondBackground = -1;
        first.sprite = backgroundImages[firstBackground];
        animator.SetTrigger("FadeIn");
    }

    public void End()
    {
        isFirst = true;
        secondBackground = 8;
        animator.SetTrigger("FadeOut");
    }

    public void Change()
    {
        if (isFirst) animator.SetTrigger("FadeOut");
            else animator.SetTrigger("FadeIn");
        isFirst = !isFirst;
    }
}