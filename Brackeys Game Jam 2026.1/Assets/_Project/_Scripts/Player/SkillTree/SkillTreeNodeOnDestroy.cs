
using System;
using UnityEngine;

[ExecuteAlways]
public class SkillTreeNodeOnDestroy : MonoBehaviour
{

    public SkillTreeNode parent;

    private void OnValidate()
    {
        parent = GetComponent<SkillTreeNode>();
    }


    private void OnDestroy()
    {
        for (int i = parent.connectedConnectors.Count-1; i>-1 ; i--)
        {
            if (parent.connectedConnectors[i])
            {
                parent.connectedConnectors[i].DestroyReferences();
            }
        }
    }
}