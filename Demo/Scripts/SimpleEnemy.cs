using UnityEngine;

public class SimpleEnemy : MonoBehaviour, ISpatialEntity
{
    public SpatialCell CurrentCell { get; set; }

    [SerializeField] private SpatialGrid grid;
    [SerializeField] private Vector2 minMaxWander;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int health = 100;

    private Vector3 _targetPos;
    private bool _isDead;

    private void Start()
    {
        SelectNewTargetPosition();
    }

    private void Update()
    {
        grid.RefreshEntityPosition(this);
        
        MoveToPosition();
        if (Vector3.Distance(transform.position, _targetPos) <= .1f)
        {
            SelectNewTargetPosition();
        }
    }

    public void TakeDamages(int amount)
    {
        if (_isDead)
        {
            return;
        }
        
        health -= amount;
        if (health <= 0)
        {
            _isDead = true;
            CurrentCell?.RemoveEntity(this);
            Destroy(gameObject);
        }
    }

    private void MoveToPosition()
    {
        transform.LookAt(_targetPos);
        transform.position = Vector3.MoveTowards(transform.position, _targetPos, moveSpeed * Time.deltaTime);
    }

    private void SelectNewTargetPosition()
    {
        _targetPos = new Vector3(Random.Range(minMaxWander.x, minMaxWander.y), 0f, Random.Range(minMaxWander.x, minMaxWander.y));
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public string GetDebugName()
    {
        return name;
    }
}