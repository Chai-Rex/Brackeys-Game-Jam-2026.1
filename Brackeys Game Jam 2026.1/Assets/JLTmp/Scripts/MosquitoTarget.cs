using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;


public class MosquitoTarget : MonoBehaviour
{
    static HashSet<MosquitoTarget> set = new();
    static public IEnumerable<MosquitoTarget> Targets => set;
    static public int TargetsCount => set.Count;

    [SerializeField] new Rigidbody2D rigidbody2D;
    [SerializeField] float minSpeed = 1f;
    [SerializeField] float outRadius = 0.6f;
    [SerializeField] float inRadius = 0.5f;
    [SerializeField] MyEvent onStartMoving;
    [SerializeField, ReadOnly] bool isMoving;

    public Rigidbody2D Rigidbody2D => rigidbody2D;
    public float OutRadius => outRadius;
    public float InRadius => inRadius;
    public float SqrOutRadius => outRadius * outRadius;
    public float SqrInRadius => inRadius * inRadius;
    public bool IsMoving => isMoving;
    public MyEvent OnStartMoving => onStartMoving;

    void Reset()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void OnDrawGizmosSelected()
    {
        DrawRadius();
    }

    void DrawRadius()
    {
        Gizmos.color = Color.orange;
        GizmosExtension.DrawCircle(transform.position, Vector3.forward, outRadius);

        Gizmos.color = Color.red;
        GizmosExtension.DrawCircle(transform.position, Vector3.forward, inRadius);
    }

    void OnEnable()
    {
        set.Add(this);
        
        isMoving = rigidbody2D && rigidbody2D.linearVelocity.sqrMagnitude > minSpeed * minSpeed;
        
        if (isMoving)
            onStartMoving.Invoke();
    }

    void OnDisable()
    {        
        set.Remove(this);
    }

    void FixedUpdate()
    {
        SetIsMoving(rigidbody2D && rigidbody2D.linearVelocity.sqrMagnitude > minSpeed * minSpeed);
    }

    void SetIsMoving(bool isMoving)
    {
        if (this.isMoving == isMoving)
            return;

        this.isMoving = isMoving;

        if (isMoving)
            onStartMoving.Invoke();
    }
}
