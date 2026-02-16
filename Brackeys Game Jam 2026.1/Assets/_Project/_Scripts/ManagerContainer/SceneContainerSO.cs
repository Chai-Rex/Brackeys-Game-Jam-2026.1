using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

////////////////////////////////////////////////////////////
/// Scene Container with Cached Interface Lists
////////////////////////////////////////////////////////////

[CreateAssetMenu(fileName = "SceneContainerSO", menuName = "ScriptableObjects/Containers/SceneContainerSO")]
public class SceneContainerSO : BaseManagerContainerSO {

    [Header("Scene Info")]
    [SerializeField] private string _iSceneName;

    ////////////////////////////////////////////////////////////
    /// Utility Methods
    ////////////////////////////////////////////////////////////
    public string GetSceneName() => _iSceneName;
}
