using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestructibleTileBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision Detected");
        if (collision.gameObject.CompareTag("Drill"))
        {
            bool isDrilling = collision.gameObject.GetComponent<DrillHandler>().isDrilling = true;
            if(isDrilling == true)
            {
                StartCoroutine(DrillDelay(collision, isDrilling));
            }
        }   
    }

    private void DrillBreak(Collider2D col)
    {
        Vector3Int position = col.gameObject.GetComponent<Tilemap>().WorldToCell(col.gameObject.transform.position);
        col.gameObject.GetComponent<Tilemap>().SetTile(position, null);
    }

    private System.Collections.IEnumerator DrillDelay(Collider2D col, bool isDrilling)
    {
        Debug.Log("Wait function started");
        float startTime = Time.time;
        while (Time.time - startTime < 1f)
        {
            if (isDrilling == false)
            {
                Debug.Log("Interupted Wait");
                startTime = Time.time;
                yield break;
            }
            yield return null; //or WaitForEndOfFrame() etc
        }
        DrillBreak(col);
        Debug.Log("Wait function completed");
    }
}
