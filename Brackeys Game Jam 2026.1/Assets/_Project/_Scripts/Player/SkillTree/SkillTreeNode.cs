using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeNode : MonoBehaviour
{
    [SerializeField] private SkillUpgradeSO skillUpgrade;
    
    private bool unlocked=false, activated=false;
    public List<SkillTreeNode> precedingNodes;
    public List<SkillTreeNode> nextNodes;
    
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color buttonDefault;

    [SerializeField] private GameObject connectorUIPrefab;
    public Transform connectorParent;
    public List<SkillNodeConnector> connectedConnectors;


    private void Awake()
    {
        _nameText.text = skillUpgrade.SkillUpgradeName;
    }

    private void Start()
    {
        IsThisUnlocked();
    }

    private void IsThisUnlocked()
    {
        unlocked = true;
        ChangeButtonColor(buttonDefault);

        foreach (var node in precedingNodes)
        {
            if (!node.activated)
            {
                unlocked = false;
                ChangeButtonColor(Color.grey);
                return;
            }
        }
    }

    private void CheckUnlocks()
    {
        foreach (var node in nextNodes)
        {
            node.IsThisUnlocked();
        }
    }

    private void ChangeButtonColor(Color changeTo)
    {
        buttonImage.color = changeTo;
    }

    public void OnClicked()
    {
        //Debug.Log($"Clicked unlocked:{unlocked} activated{activated}");
        if (unlocked && !activated)
        {
            ActivateUpgrade();
        }

    }

    public static event Action<SkillUpgradeSO> upgradeActivation, upgradeDrill;
    private void ActivateUpgrade()
    {
        if(activated)return;
        activated = true;

        //check if the scriptable object has the drill upgrade
        bool hasDrillUpgrade 
            = skillUpgrade.SkillUpgrades.Any
                (u => u.SkillUpgradeEnum == SkillUpgradeEnum.DrillDurationNeeded);

        //Send signal
        if (hasDrillUpgrade)
        {
            upgradeDrill?.Invoke(skillUpgrade);
        }
        else
        {
            upgradeActivation?.Invoke(skillUpgrade);
        }
        //
        
        ChangeButtonColor(Color.cyan);
        CheckUnlocks();
    }


    #region Editor Functions
    
    public void SpawnNextNode()
    {
#if UNITY_EDITOR
        GameObject prefabRoot =
            UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

        if (prefabRoot == null)
        {
            Debug.LogWarning("This object is not a prefab instance.");
            return;
        }

        Transform thisParent = this.transform.parent;
        GameObject clone = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefabRoot,thisParent);

        clone.transform.position = transform.position + Vector3.right * 150f;
        clone.transform.rotation = transform.rotation;

        SkillTreeNode cloneSTN = clone.GetComponent<SkillTreeNode>();
        
        //add this node to the new node's preceding list
        cloneSTN.precedingNodes.Add(this);
        //add new node to next nodes of this node
        nextNodes.Add(cloneSTN);
        //spawn connector UI
        GameObject connectorUI =  (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(connectorUIPrefab, connectorParent);
        SkillNodeConnector skillNodeConnector = connectorUI.GetComponent<SkillNodeConnector>();
        
        skillNodeConnector.target = clone.GetComponent<RectTransform>();
        skillNodeConnector.from = GetComponent<RectTransform>();

        skillNodeConnector.fromSTN = this;
        skillNodeConnector.toSTN = cloneSTN;
        
        connectedConnectors.Add(skillNodeConnector);
        cloneSTN.connectedConnectors.Add(skillNodeConnector);
        cloneSTN.connectorParent = connectorParent;
#endif
    }

    public void AdjustToScriptableObject()
    {
#if UNITY_EDITOR
        if (skillUpgrade == null) return;
        
        _nameText.text = skillUpgrade.SkillUpgradeName;
#endif
    }

    #endregion
}
