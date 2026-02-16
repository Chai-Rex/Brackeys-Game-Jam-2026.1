using UnityEngine;


public class PickRandomSpriteOrderOnAwake : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] int[] orders;

    void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        if (spriteRenderer &&
            orders.Length > 0)
            spriteRenderer.sortingOrder = orders.PickRandom();
    }
}
