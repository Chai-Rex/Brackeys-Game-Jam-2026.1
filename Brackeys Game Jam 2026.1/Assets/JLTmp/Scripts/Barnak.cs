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
    int hitsCount = 0;

    [SerializeField] float eatingTime = 5;
    [SerializeField] float recoveringTime = 5;

    [Space(15)]
    [SerializeField] string notifyAkOnCatch = "Vine_Catch";
    [SerializeField] string notifyAkOnStartEating = "Vine_StartEating";
    [SerializeField] string notifyAkOnStopEating = "Vine_StopEating";
    [SerializeField] string notifyAkOnHit = "Vine_Hit";
    [SerializeField] string notifyAkOnLastHit = "Vine_LastHit";
    [SerializeField] string notifyAkOnStopRecovering = "Vine_StopRecovering";
    [SerializeField] string notifyAkCaughtProgress = "Pull_Tension";

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

    void OnDisable()
    {
        if (state == State.Caught)
            ReleaseTarget();

        if (state == State.Recovering)
        {
            CancelInvoke("StopRecovering");
            StopRecovering();
        }

        else if (state == State.Eating)
        {
            CancelInvoke("StopEating");
            StopEating();
        }
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

        if (notifyAkOnCatch != "")
            AkUnitySoundEngine.PostEvent(notifyAkOnCatch, gameObject);
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
        {
            shakeCaughtTarget.transform.AddYPosition(caughtSpeed * Time.fixedDeltaTime);

            if (notifyAkCaughtProgress != "")
            {
                float progress = 1 - vineLength / maxVineLength;
                AkUnitySoundEngine.SetRTPCValue(notifyAkCaughtProgress, progress * 100, gameObject);
            }
        }

        else {
            state = State.Eating;

            caughtTarget.transform.SetParent(shakeCaughtTarget.transform.parent);
            shakeCaughtTarget.transform.SetParent(transform);
            shakeCaughtTarget.transform.localPosition = Vector3.zero;
            shakeCaughtTarget.Target = null;

            hitsCount = 0;
            caughtTarget.OnBarnakEat(this, null);
            caughtTarget = null;
            animator.SetInteger("state", 2);
            
            if (notifyAkOnStartEating != "")
                AkUnitySoundEngine.PostEvent(notifyAkOnStartEating, gameObject);

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

        if (notifyAkOnStopEating != "" &&
            gameObject.activeInHierarchy)
            AkUnitySoundEngine.PostEvent(notifyAkOnStopEating, gameObject);
    }

    public void HitToRelease()
    {
        hitsCount++;

        if (hitsCount < hitsToRelease)
        {
            if (notifyAkOnHit != "")
                AkUnitySoundEngine.PostEvent(notifyAkOnHit, gameObject);
            
            shakeCaughtTarget.Impact(hitShakeAmplitude);
        }

        else {        
            if (notifyAkOnLastHit != "")
                AkUnitySoundEngine.PostEvent(notifyAkOnLastHit, gameObject);

            else if (notifyAkOnHit != "")
                AkUnitySoundEngine.PostEvent(notifyAkOnHit, gameObject);

            ReleaseTarget();
        } 
    }

    public void ReleaseTarget()
    {
        if (state != State.Caught)
            return;

        state = State.Recovering;

        caughtTarget.transform.SetParent(shakeCaughtTarget.transform.parent);
        shakeCaughtTarget.transform.SetParent(gameObject.activeInHierarchy ? transform : null);
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
        
        if (notifyAkOnStopRecovering != "" &&
            gameObject.activeInHierarchy)
            AkUnitySoundEngine.PostEvent(notifyAkOnStopRecovering, gameObject);
    }
}



public interface IBarnakTarget
{
    public void OnBarnakCaught(Barnak barnak);
    public void OnBarnakRelease(Barnak barnak);
    public void OnBarnakEat(Barnak barnak, GroundedBarnak groundedBarnak);
    public Transform transform { get; }
    public float BarnakTargetRadius { get; }
}