using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Umbrelloid : MonoBehaviour
{
    [SerializeField] float randomAcceleration = 1.5f;
    [SerializeField] float noiseScale = 0.5f;
    Vector2 noiseOffset;
    [SerializeField] float friction = 0.5f;

    [Space(15)]
    [SerializeField] float groundRepulsion = 1f;
    [SerializeField] float groundRepulsionRadius = 1.4f;
    [SerializeField] float groundRaycastLength = 3f;
    [SerializeField] LayerMask groundLayer;



    [Space(15)]
    [SerializeField] float rotateWithAcceleration = 1;
    // [SerializeField] SpriteRenderer spriteRenderer;
    // [SerializeField] Sprite spriteOpened;
    // [SerializeField] Sprite spriteClosed;

    Rigidbody2D rb;


    void OnDrawGizmosSelected()
    {
        DrawGroundRepulsion();
    }

    void DrawGroundRepulsion()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundRaycastLength);
        GizmosExtension.DrawCircle(transform.position, groundRepulsionRadius);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        noiseOffset = RandomExtension.RandomVector2() * 1000;
    }

    void FixedUpdate()
    {
        Vector2 randomAccelerationApplied = ApplyRandomAcceleration();
        transform.SetZEuler(randomAccelerationApplied.x * rotateWithAcceleration);
        GroundRepulsion();
        ApplyFriction();
    }

    Vector2 ApplyRandomAcceleration()
    {
        Vector2 noiseValue = new(Mathf.PerlinNoise1D(noiseOffset.x + Time.time * noiseScale) * 2f - 0.9305f, 
                                 Mathf.PerlinNoise1D(noiseOffset.y + Time.time * noiseScale) * 2f - 0.9305f);

        Vector2 acceleration = randomAcceleration * noiseValue;
        rb.linearVelocity += acceleration * Time.fixedDeltaTime;
        return acceleration;
    }

    void ApplyFriction()
    {
        rb.linearVelocity *= 1 - friction * Time.fixedDeltaTime;
    }

    Vector2 GroundRepulsion()
    {
        
        GroundData groundData = GroundDataGrid.Instance.GetData(transform.position);

        if (groundData.HasHit &&
            groundData.GroundDistance < groundRepulsionRadius)
        {
            Vector2 acceleration = groundRepulsion * groundData.OutDirection;
            rb.linearVelocity += acceleration * Time.fixedDeltaTime;
            return acceleration;
        }


        else if (Physics2D.Raycast(transform.position, Vector2.down, groundRaycastLength, groundLayer))
        {
            Vector2 acceleration = groundRepulsion * Vector2.up;
            rb.linearVelocity += acceleration * Time.fixedDeltaTime;
            return acceleration;
        }
        
        return Vector2.zero;
    }
}
