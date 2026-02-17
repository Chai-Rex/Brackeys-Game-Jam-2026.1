using UnityEngine;

public class RandomIncrOrDecrementSpriteOrder : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] spriteRenderers;
    [SerializeField] int value = 1;

    void Reset()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    void Awake()
    {
        int value = RandomExtension.FlipCoin() ? this.value : -this.value;

        foreach (SpriteRenderer sp in spriteRenderers)
            if (sp) sp.sortingOrder += value;
    }
}
