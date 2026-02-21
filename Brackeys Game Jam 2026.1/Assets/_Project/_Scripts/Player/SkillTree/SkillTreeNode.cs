using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeNode : MonoBehaviour
{
    [SerializeField] private SkillUpgradeSO skillUpgrade;
    
    private bool unlocked=false, activated=false;
    
    [Header("Nodes")]
    public List<SkillTreeNode> precedingNodes;
    public List<SkillTreeNode> nextNodes;
    
    [Header("Button")]
    [SerializeField] private TextMeshProUGUI _nameText, resourceCostText;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color buttonDefault;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite movementIcon, drillIcon, healthIcon;
    
    [Header("Connector UI")]
    [SerializeField] private GameObject connectorUIPrefab;
    public Transform connectorParent;
    public List<SkillNodeConnector> connectedConnectors;


    private void Awake()
    {
        AdjustToScriptableObject();
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
        timer = 0f;
        isLerping = true;
        startColor = buttonImage.color;
        endColor = changeTo;
        //buttonImage.color = changeTo;
    }

    private bool isLerping = false;
    private float timer = 0f;
    private Color startColor, endColor;
    private float duration = 0.5f;
    private void Update()
    {
        if (!isLerping) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        buttonImage.color = Color.Lerp(startColor, endColor, t);

        if (t >= 1f)
            isLerping = false;
    }

    public void OnClicked()
    {
        //Debug.Log($"Clicked unlocked:{unlocked} activated{activated}");
        if (unlocked && !activated)
        {
            ActivateUpgrade();
        }

    }

    public static event Action<SkillUpgradeSO> upgradeMovement, upgradeDrill, upgradeHealth;
    private void ActivateUpgrade()
    {
        if(activated)return;
        activated = true;
        

        //Send event assuming the SO classifications are accurate and the SOs only have that type of upgrade
        switch (skillUpgrade.classification)
        {
            case UpgradeGeneralClassification.Drill:
                upgradeDrill?.Invoke(skillUpgrade);
                break;
            case UpgradeGeneralClassification.Health:
                upgradeHealth?.Invoke(skillUpgrade);
                break;
            case UpgradeGeneralClassification.Movement:
                upgradeMovement?.Invoke(skillUpgrade);
                break;
        }
        //
        
        ChangeButtonColor(Color.cyan);
        CheckUnlocks();
    }

    public void AdjustToScriptableObject()
    {
        if (skillUpgrade == null) return;
        
        _nameText.text = skillUpgrade.SkillUpgradeName;
        
        switch (skillUpgrade.classification)
        {
            case UpgradeGeneralClassification.Drill:
                iconImage.sprite = drillIcon;
                break;
            case UpgradeGeneralClassification.Health:
                iconImage.sprite = healthIcon;
                break;
            case UpgradeGeneralClassification.Movement:
                iconImage.sprite = movementIcon;
                break;
        }
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

    

    #endregion
}
