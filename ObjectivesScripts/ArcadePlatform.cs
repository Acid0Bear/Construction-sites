using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadePlatform : MonoBehaviour
{
    [SerializeField]
    private float ForceAmplifier = 10f;

    [SerializeField]
    private Transform ToPos = null;
    [SerializeField]
    private float Speed = 5f;
    [SerializeField]
    private float SpeedWithObjectOnIt = 5f;
    [SerializeField]
    private float SleepTimer = 5f;
    [SerializeField]
    private bool CompensateWhenVehicleIsMoving = false;

    private Coroutine Routine;
    private Vector2 Initial, Target;
    private Vector2 Cur => transform.position;
    private List<GameObject> ObjectsOnPlatform = new List<GameObject>();
    private bool Pause;
    private void Awake()
    {
        Routine = StartCoroutine(MovementRoutine());
        Initial = transform.position;
        Target = ToPos.position;
    }
    IEnumerator MovementRoutine()
    {
        float timer = SleepTimer;
        while (true)
        {
            if(Cur != Target && !Pause)
            {
                float spd = (ObjectsOnPlatform.Count == 0) ? Speed : SpeedWithObjectOnIt;
                Vector2 newPlatformPos = Vector2.MoveTowards(Cur, Target, Time.deltaTime * spd);
                Vector3 Diff = newPlatformPos - (Vector2)transform.position;
                transform.position = newPlatformPos;
                //This loop compensates difference in positions in order to remove jiggle
                foreach(GameObject obj in ObjectsOnPlatform)
                {
                    if ((!CompensateWhenVehicleIsMoving && Controller.M_Controller.Directional == 0 || CompensateWhenVehicleIsMoving) && obj.tag != "Objective")
                        obj.GetComponent<Rigidbody2D>().AddForce((Vector2)Diff * ForceAmplifier,ForceMode2D.Impulse);
                    else if(obj.tag == "Objective")
                        obj.transform.position += Diff;
                }
            }
            else if (Pause)
            {
                if (timer >= 0)
                    timer -= Time.deltaTime;
                else
                {
                    timer = SleepTimer;
                    Pause = false;
                }
            }
            else
            {
                var tmp = Target;
                Target = Initial;
                Initial = tmp;
                Pause = true;
            }
            yield return null;
        }
    }

    private GameObject GetHighestParent(GameObject obj)
    {
        return (obj.transform.parent != null) ? GetHighestParent(obj.transform.parent.gameObject) : obj;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!ObjectsOnPlatform.Contains(GetHighestParent(collision.gameObject)))
            ObjectsOnPlatform.Add(GetHighestParent(collision.gameObject));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (ObjectsOnPlatform.Contains(GetHighestParent(collision.gameObject)))
            ObjectsOnPlatform.Remove(GetHighestParent(collision.gameObject));
    }
}
