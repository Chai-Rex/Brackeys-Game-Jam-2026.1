using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerManager", menuName = "ScriptableObjects/Managers/PlayerManager")]
public class PlayerManager : ScriptableObject {

    // Blackboard

    // game object in scene

    public async void Initialize() {
        await Task.Yield(); // placeholder for any async initialization tasks
    }

    public void Update() {


        // no idea
    }

    public void FixedUpdate() {

        // physics updates

    }

    public void CleanUp() {

    }

    public void Pause() {

    }

    public void Resume() {

    }
}
