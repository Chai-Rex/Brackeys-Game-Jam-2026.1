using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;


public class MosquitoTarget : MonoBehaviour
{
    static HashSet<MosquitoTarget> set = new();
    static HashSet<MosquitoTarget> rbSet = new();
    static HashSet<MosquitoTarget> noRbSet = new();

    static public IEnumerable<MosquitoTarget> Targets => set;
    static public IEnumerable<MosquitoTarget> RbTargets => rbSet;
    static public IEnumerable<MosquitoTarget> NoRbTargets => noRbSet;

    static public int TargetsCount => set.Count;
    static public int RbTargetsCount => rbSet.Count;
    static public int NoRbTargetsCount => noRbSet.Count;


    [SerializeField] new Rigidbody2D rigidbody2D;
    [SerializeField] float radius = 0.6f;


    public Rigidbody2D Rigidbody2D => rigidbody2D;
    public float Radius => radius;
    public float SqrRadius => radius * radius;


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
        Gizmos.color = Color.red;
        GizmosExtension.DrawCircle(transform.position, Vector3.forward, radius);
    }

    void OnEnable()
    {
        set.Add(this);
        (rigidbody2D ? rbSet : noRbSet).Add(this);
    }

    void OnDisable()
    {        
        set.Remove(this);
        (rigidbody2D ? rbSet : noRbSet).Remove(this);
    }
}
