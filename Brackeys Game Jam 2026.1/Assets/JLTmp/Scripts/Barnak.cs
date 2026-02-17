using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Animator))]
public class Barnak : MonoBehaviour
{
    [SerializeField, ReadOnly] State state = State.Waiting;
    [SerializeField] Transform vine;
    [SerializeField] float vineSpeed = 5;
    float maxVineLength;
    float vineLength;

    [SerializeField] float caughtSpeed = 1;

    BoxCollider2D trigger;
    Animator animator;
    IBarnakTarget caughtTarget;

    public enum State
    {
        Waiting,
        Caught,
        Eating,
        Recovering
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        trigger = GetComponent<BoxCollider2D>();
        trigger.enabled = true;

        maxVineLength = -vine.localPosition.y;
        SetVineLength(maxVineLength);
    }

    void FixedUpdate()
    {
        if (state == State.Caught)
            UpdateCaught();

        UpdateVineLength();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (state != State.Waiting ||
            !collision.TryGetComponentInParentOrInChildren(out IBarnakTarget target))
            return;

        state = State.Caught;

        trigger.enabled = false;

        caughtTarget = target;
        target.transform.SetXPosition(transform.position.x);
        target.OnTriggerBarnak();
        
        animator.SetInteger("state", 1);
    }

    void SetVineLength(float length)
    {
        length = Mathf.Clamp(length, 0, maxVineLength);

        if (vineLength == length)
            return;

        vineLength = length;
        vine.SetYLocalPosition(-length);
        trigger.offset = new(0, -length/2f);
        trigger.size = new(trigger.size.x, length);
    }

    void UpdateVineLength()
    {
        SetVineLength(Mathf.Lerp(
            vineLength, 
            caughtTarget != null ? transform.position.y - caughtTarget.transform.position.y : maxVineLength, 
            vineSpeed * Time.fixedDeltaTime));
    }

    void UpdateCaught()
    {
        if (state != State.Caught)
            return;

        if (transform.position.y - caughtTarget.transform.position.y > caughtTarget.BarnakTargetRadius)
            caughtTarget.transform.AddYPosition(caughtSpeed * Time.fixedDeltaTime);

        else {
            
        }
    }
}



public interface IBarnakTarget
{
    public void OnTriggerBarnak();
    public Transform transform { get; }
    public float BarnakTargetRadius { get; }
}