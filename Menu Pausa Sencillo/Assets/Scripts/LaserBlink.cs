
using System.Collections;
using UnityEngine;

public class LaserBlink : MonoBehaviour
{
    [Header("Activación")]
    [SerializeField] private float onTime = 2f;
    [SerializeField] private float offTime = 1f;

    [Header("Movimiento")]
    [SerializeField] private Vector3 movementOffset = new Vector3(0f, 0f, 3f);
    [SerializeField] private float moveSpeed = 3f;

    private Collider laserCollider;
    private Renderer laserRenderer;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool laserActive;

    private void Awake()
    {
        laserCollider = GetComponent<Collider>();
        laserRenderer = GetComponent<Renderer>();

        startPosition = transform.position;
        endPosition = startPosition + movementOffset;
    }

    private void Start()
    {
        StartCoroutine(LaserCycle());
    }

    private void Update()
    {
        if (!laserActive)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            endPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, endPosition) < 0.01f)
        {
            Vector3 temp = startPosition;
            startPosition = endPosition;
            endPosition = temp;
        }
    }

    private IEnumerator LaserCycle()
    {
        while (true)
        {
            SetLaser(true);
            yield return new WaitForSeconds(onTime);

            SetLaser(false);
            yield return new WaitForSeconds(offTime);
        }
    }

    private void SetLaser(bool active)
    {
        laserActive = active;
        laserRenderer.enabled = active;
        laserCollider.enabled = active;
    }
}
