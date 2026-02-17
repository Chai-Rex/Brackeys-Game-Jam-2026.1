using EditorAttributes;
using UnityEngine;

public class Barnak : MonoBehaviour
{
    [SerializeField, ReadOnly] BarnakState state = BarnakState.Waiting;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (state != BarnakState.Waiting ||
            !collision.TryGetComponentInParentOrInChildren(out IBarnakTarget target))
            return;

        state = BarnakState.Eating;
        target.transform.SetXPosition(transform.position.x);
        target.OnTriggerBarnak();
    }
}

public enum BarnakState
{
    Waiting,
    Eating,
    Recovering
}

public interface IBarnakTarget
{
    public void OnTriggerBarnak();
    public Transform transform { get; }
}