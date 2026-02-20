using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillTreeNode))]
public class PrefabSelfSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        SkillTreeNode spawner = (SkillTreeNode)target;

        if (GUILayout.Button("Spawn Next Node"))
        {
            spawner.SpawnNextNode();
        }
        
        if (GUILayout.Button("Adjust to Scriptable Object"))
        {
            spawner.AdjustToScriptableObject();
        }
    }
}