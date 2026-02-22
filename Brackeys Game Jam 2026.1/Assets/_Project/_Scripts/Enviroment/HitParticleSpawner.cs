using UnityEngine;

public class HitParticleSpawner : MonoBehaviour
{
    [SerializeField]private GameObject _ParticlePrefab;
    public Material particleMaterial;
    
    public void SpawnParticles(Sprite tileSprite, Vector3 position)
    {
        ParticleSystemRenderer particleRenderer = _ParticlePrefab.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
        particleMaterial.mainTexture = tileSprite.texture;
        particleRenderer.material = particleMaterial;
        Instantiate(_ParticlePrefab, position, Quaternion.identity);
    }
}
