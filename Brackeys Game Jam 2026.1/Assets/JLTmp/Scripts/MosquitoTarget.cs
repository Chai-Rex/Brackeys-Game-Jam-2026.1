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
    [SerializeField] Vector2 size = new Vector2(1, 1);


    public Rigidbody2D Rigidbody2D => rigidbody2D;
    public Vector2 Size => size;
    public bool PointInRect(Vector2 position) => position.x >= transform.position.x - size.x / 2 &&
                                                 position.x <= transform.position.x + size.x / 2 &&
                                                 position.y >= transform.position.y - size.y / 2 &&
                                                 position.y <= transform.position.y + size.y / 2;


    void Reset()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void OnDrawGizmosSelected()
    {
        DrawRect();
    }

    void DrawRect()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, size);
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
