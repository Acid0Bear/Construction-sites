using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlatformMovementCompensator : MonoBehaviour
{
    private Coroutine Routine;
    [SerializeField]
    private float ForceAmplifier = 10f;
    public bool CompensateWhenVehicleIsMoving { private get; set; }
    public float Speed { private get; set; }
    private Vector2 Initial; 
    public Vector2 Target { private get; set; }
    private List<GameObject> ObjectsOnPlatform = new List<GameObject>();
    private Vector2 Cur => transform.position;
    private void Awake()
    {
        Routine = StartCoroutine(MovementRoutine());
        Initial = transform.position;
        Target = Initial;
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    IEnumerator MovementRoutine()
    {
        while (true)
        {
            if (Cur != Target)
            {
                Vector2 newPlatformPos = Vector2.MoveTowards(Cur, Target, Time.deltaTime * Speed);
                Vector3 Diff = newPlatformPos - (Vector2)transform.position;
                transform.position = newPlatformPos;
                //This loop compensates difference in positions in order to remove jiggle
                foreach (GameObject obj in ObjectsOnPlatform)
                {
                    if ((!CompensateWhenVehicleIsMoving && Controller.M_Controller.Directional == 0 || CompensateWhenVehicleIsMoving) && obj.tag != "Objective")
                        obj.GetComponent<Rigidbody2D>().AddForce((Vector2)Diff * ForceAmplifier, ForceMode2D.Impulse);
                    else if (obj.tag == "Objective")
                        obj.transform.position += Diff;
                }
            }
            yield return null;
        }
    }

    private GameObject GetHighestParent(GameObject obj)
    {
        return (obj.transform.parent != null) ? GetHighestParent(obj.transform.parent.gameObject) : obj;
    }

    private void OnTriggerEnter2D(Collider2D collision)
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
