using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MadElevator : MonoBehaviour
{
    [SerializeField] Transform target1, target2;
    bool t1ToT2 = true;

    [SerializeField, Min(0.01f)] float travelTime = 1;
    [SerializeField] AnimationCurve travelCurve;

    [SerializeField, Min(0)] int nbTravels = 10;
    [SerializeField] float waitingTime = 3;
    [SerializeField] float shakeRadius = 8;
    [SerializeField] float shakeAmplitude = 5;
    [SerializeField] float shakeNoise = 5;

    Collider2D[] triggers;

    void OnDrawGizmosSelected()
    {
        DrawShakeRadius();
    }

    void DrawShakeRadius()
    {
        Gizmos.color = Color.yellow;
        GizmosExtension.DrawCircle(transform.position, shakeRadius);
    }

    void Awake()
    {
        triggers = GetComponents<Collider2D>().Where((c) => c.isTrigger).ToArray();
    }

    void OnEnable()
    {
        transform.position = target1.position;
        t1ToT2 = true;
        StartCoroutine(TravelLoop());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
    }

    IEnumerator TravelLoop()
    {
        while (true)
        {
            foreach (Collider2D trigger in triggers)
                trigger.enabled = false;
            
            yield return new WaitForSeconds(waitingTime);

            foreach (Collider2D trigger in triggers)
                trigger.enabled = true;
                
            int travelCount = 0;
            float t = 0;

            while (travelCount < nbTravels)
            {
                yield return new WaitForEndOfFrame();

                t += Time.deltaTime;

                if (t >= travelTime)
                {
                    t -= travelTime;
                    travelCount++;
                    t1ToT2 = !t1ToT2;
                    ShakeCamera.Instance.Impact((Vector2)transform.position, shakeRadius, shakeAmplitude, shakeNoise);

                    if (travelCount == nbTravels)
                    {
                        transform.position = (t1ToT2 ? target1 : target2).position;
                        break;
                    }
                }

                Transform start = t1ToT2 ? target1 : target2;
                Transform end = t1ToT2 ? target2 : target1;

                float progress = t / travelTime;

                transform.position = Vector3.Lerp(start.position, end.position, travelCurve.Evaluate(progress));
            } 
        }
    }
}
