using EditorAttributes;
using UnityEngine;

public class Jumpy : Jumper
{
    [SerializeField] Vector2 dtJumpRangePlayerDetected = new Vector2(0, 0.5f);

    [Header("Player Detection")]
    [SerializeField] Transform playerCheck;
    [SerializeField] float playerCheckRadius = 3;
    [SerializeField] LayerMask playerCheckLayer;
    [SerializeField, ReadOnly] bool playerDetected = false;

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        DrawPlayerCheck();
    }

    void DrawPlayerCheck()
    {
        if (playerCheck == null) 
            return;

        Gizmos.color = Color.yellow;
        GizmosExtension.DrawCircle(playerCheck.position, Vector3.forward, playerCheckRadius);
    }

    protected override void Reset()
    {
        base.Reset();
        playerCheck = transform;
    }

    protected override void FixedUpdate()
    {
        UpdatePlayerDetected();
        base.FixedUpdate();
    }

    void UpdatePlayerDetected()
    {
        float sqrRadius = playerCheckRadius * playerCheckRadius;
        Vector3 toPlayer = JLPlayerTest.Instance.transform.position - playerCheck.position;

        playerDetected = toPlayer.sqrMagnitude <= sqrRadius &&
                         !Physics2D.Raycast(playerCheck.position, toPlayer, toPlayer.magnitude, playerCheckLayer);
    }

    protected override void SetIsGrounded(bool isGrounded)
    {
        if (isGroundedLocked ||
            this.isGrounded == isGrounded)
            return;

        this.isGrounded = isGrounded;

        if (isGrounded) Invoke("Jump", (playerDetected ? dtJumpRangePlayerDetected : dtJumpRange).RandomInRange());
        else            CancelInvoke("Jump");
    }

    protected override void Jump()
    {
        if (!playerDetected)
            base.Jump();

        else {
            float angle = (JLPlayerTest.Instance.transform.position.x < transform.position.x ? jumpMaxAngle : -jumpMaxAngle) * Random.value;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
            rb.linearVelocity = direction * jumpSpeed;

            SetIsGrounded(false);
            LockIsGrounded();
            Invoke("UnlockIsGrounded", 0.1f);
        }
    }
}
