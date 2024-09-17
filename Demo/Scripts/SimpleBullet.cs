using UnityEngine;

public class SimpleBullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;

    public SimpleEnemy currentTarget;
    public int damages;

    private void Update()
    {
        if (!currentTarget)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget.GetPosition(), Time.deltaTime * speed);
        if (Vector3.Distance(transform.position, currentTarget.GetPosition()) <= .1f)
        {
            currentTarget.TakeDamages(damages);
            Destroy(gameObject);
        }
    }
}