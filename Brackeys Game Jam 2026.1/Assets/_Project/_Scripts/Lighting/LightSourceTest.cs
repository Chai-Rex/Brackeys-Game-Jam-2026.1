using UnityEngine;

public class LightSourceTest : MonoBehaviour {

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        PlayerLightSource light = gameObject.GetComponent<PlayerLightSource>();

        // Equip a torch
        light.AddLightSource("torch", radius: 6f, diffusion: 2.5f);

        //// Unequip torch
        //light.RemoveLightSource("torch");

        //// Mining helmet (permanent until removed)
        //light.AddLightSource("mining_helmet", radius: 4f, diffusion: 1.5f);

        //// Night vision potion (30 seconds)
        //light.AddBuff(radiusBonus: 10f, duration: 30f, diffusionBonus: 3f);
    }

}
