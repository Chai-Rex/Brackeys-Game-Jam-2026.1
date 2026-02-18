using UnityEngine;

public class RandomSpriteOrderLevel : MonoBehaviour
{
    [SerializeField] Vector2Int lvlRange = new Vector2Int(-5, 5);
    [SerializeField] int orderPerLvl = 100;
    [SerializeField] int addOrder = 0;

    void Awake()
    {
        int lvl = lvlRange.RandomInRange();
        int addOrder = lvl * orderPerLvl + this.addOrder;

        foreach (SpriteRenderer sp in GetComponentsInChildren<SpriteRenderer>(true))
            sp.sortingOrder += addOrder;
    }
}
