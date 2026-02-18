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
    [SerializeField] Shake shakeCaughtTarget;
    [SerializeField] int hitsToRelease = 5;
    [SerializeField] float hitShakeAmplitude = 0.5f;
    [SerializeField] float hitShakeNoise = 10f;
    int hitsCount = 0;

    [SerializeField] float eatingTime = 5;
    [SerializeField] float recoveringTime = 5;

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
        shakeCaughtTarget.transform.SetParent(target.transform.parent);
        shakeCaughtTarget.transform.position = target.transform.position;
        target.transform.SetParent(shakeCaughtTarget.transform);
        shakeCaughtTarget.Target = target.transform;

        hitsCount = 0;
        target.OnBarnakCaught(this);        
        animator.SetInteger("state", 1);
    }

    void SetVineLength(float length)
    {
        if (vineLength == length)
            return;

        vineLength = length;
        vine.SetYLocalPosition(-length);
        trigger.offset = new(0, -length/2f);
        trigger.size = new(trigger.size.x, length);
    }

    void UpdateVineLength()
    {
        float targetLength;

        if (state == State.Caught)
            targetLength = transform.position.y - caughtTarget.transform.position.y;

        else if (state == State.Eating ||
                 state == State.Recovering)
            targetLength = 0;

        else targetLength = maxVineLength;

        SetVineLength(Mathf.Lerp(vineLength, targetLength, vineSpeed * Time.fixedDeltaTime));
    }

    void UpdateCaught()
    {
        if (transform.position.y - caughtTarget.transform.position.y > caughtTarget.BarnakTargetRadius)
            shakeCaughtTarget.transform.AddYPosition(caughtSpeed * Time.fixedDeltaTime);

        else {
            state = State.Eating;

            caughtTarget.transform.SetParent(shakeCaughtTarget.transform.parent);
            shakeCaughtTarget.transform.SetParent(transform);
            shakeCaughtTarget.transform.localPosition = Vector3.zero;
            shakeCaughtTarget.Target = null;

            hitsCount = 0;
            caughtTarget.OnBarnakEat(this);
            caughtTarget = null;
            animator.SetInteger("state", 2);
            Invoke("StopEating", eatingTime);
        }
    }

    void StopEating()
    {
        if (state != State.Eating)
            return;

        state = State.Waiting;
        trigger.enabled = true;
        animator.SetInteger("state", 0);
    }

    public void HitToRelease()
    {
        hitsCount++;

        if (hitsCount < hitsToRelease)
        {
            shakeCaughtTarget.AddAmplitude += hitShakeAmplitude;
            shakeCaughtTarget.AddNoise += hitShakeNoise;
        }

        else ReleaseTarget();
    }

    void ReleaseTarget()
    {
        if (state != State.Caught)
            return;

        state = State.Recovering;

        caughtTarget.transform.SetParent(shakeCaughtTarget.transform.parent);
        shakeCaughtTarget.transform.SetParent(transform);
        shakeCaughtTarget.transform.localPosition = Vector3.zero;
        shakeCaughtTarget.Target = null;

        hitsCount = 0;
        caughtTarget.OnBarnakRelease(this);
        caughtTarget = null;
        animator.SetInteger("state", 0);
        Invoke("StopRecovering", recoveringTime);
    }

    void StopRecovering()
    {
        if (state != State.Recovering)
            return;

        state = State.Waiting;
        trigger.enabled = true;
        animator.SetInteger("state", 0);
    }
}



public interface IBarnakTarget
{
    public void OnBarnakCaught(Barnak barnak);
    public void OnBarnakEat(Barnak barnak);
    public void OnBarnakRelease(Barnak barnak);
    public Transform transform { get; }
    public float BarnakTargetRadius { get; }
}