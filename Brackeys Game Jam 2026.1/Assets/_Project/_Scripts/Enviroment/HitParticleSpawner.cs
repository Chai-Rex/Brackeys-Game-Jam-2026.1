using UnityEngine;

public class HitParticleSpawner : MonoBehaviour
{
    [SerializeField]private GameObject _ParticlePrefab;
    public Material particleMaterial;
    
    public void SpawnParticles(Sprite tileSprite, Vector3Int cellPosition)
    {
        ParticleSystemRenderer particleRenderer = _ParticlePrefab.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
        particleMaterial.mainTexture = tileSprite.texture;
        particleRenderer.material = particleMaterial;
        Instantiate(_ParticlePrefab, new Vector3(cellPosition.x + 0f, cellPosition.y + 0f, 0f), Quaternion.identity);
    }
}
