using UnityEngine;

[ExecuteAlways]
public class SkillNodeConnector : MonoBehaviour
{
    public RectTransform target;
    public RectTransform from;

    private RectTransform rect;
    public float thickness = 10f;

    public SkillTreeNode fromSTN, toSTN;

    private void OnEnable()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!target) return;

        Vector2 direction = target.position - rect.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rect.rotation = Quaternion.Euler(0, 0, angle);
        
        // Midpoint position
        rect.position = (from.position + target.position) / 2f;
        
        
        Vector3 posA = from.localPosition;
        Vector3 posB = target.localPosition;
        
        
        // Direction
        Vector3 dir = (posB - posA);
        
        //Debug.Log($"{posB}-{posA}={dir}");
        
        // Length
        float length = dir.magnitude;
        
        rect.sizeDelta = new Vector2(length, thickness);
    }

    public void DestroyReferences()
    {
        fromSTN.nextNodes.Remove(toSTN);
        fromSTN.connectedConnectors.Remove(this);

        toSTN.precedingNodes.Remove(fromSTN);
        toSTN.connectedConnectors.Remove(this);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this)
                DestroyImmediate(gameObject);
        };
#endif
        
    }
}