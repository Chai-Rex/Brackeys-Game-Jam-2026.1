using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Animator))]
public class GroundedBarnak : MonoBehaviour
{
    [SerializeField, ReadOnly] bool isEating;
    [SerializeField] float eatingTime = 5;

    Collider2D trigger;
    Animator animator;


    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isEating", isEating);
        trigger = GetComponent<Collider2D>();
        trigger.enabled = true;
    }

    void OnDisable()
    {
        if (isEating)
            StopEating();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isEating ||
            !collision.TryGetComponentInParentOrInChildren(out IBarnakTarget target))
            return;

        isEating = true;
        trigger.enabled = false;
        animator.SetBool("isEating", isEating);
        target.OnBarnakEat(null, this);
        Invoke("StopEating", eatingTime);
    }

    void StopEating()
    {
        if (!isEating)
            return;

        isEating = false;
        trigger.enabled = true;
        animator.SetBool("isEating", isEating);
    }
}
