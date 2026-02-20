using System.Linq;
using UnityEngine;

public class ActiveArroundCameraManager : MonoBehaviour
{
    [SerializeField] float radius = 10;
    float sqrRadius;

    void OnDrawGizmosSelected()
    {
        DrawRadius();
    }

    void DrawRadius()
    {
        if (!Camera.main)
            return;

        Gizmos.color = Color.yellow;
        GizmosExtension.DrawCircle((Vector2)Camera.main.transform.position, radius);
    }

    void OnValidate()
    {
        sqrRadius = radius * radius;
    }

    void Awake()
    {
        sqrRadius = radius * radius;
    }

    void Update()
    {        
        if (!Camera.main)
            return;
            
        foreach (ActiveArroundCamera aac in ActiveArroundCamera.set.ToArray())
            aac.gameObject.SetActive(((Vector2)aac.transform.position - (Vector2)Camera.main.transform.position).sqrMagnitude < sqrRadius);
    }
}
