using UnityEngine;

public class DashEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float scaleSpeed = 3f;
    [SerializeField] private float maxScale = 2f;
    
    [Header("Trail")]
    [SerializeField] private int trailCount = 5;
    [SerializeField] private float trailSpacing = 0.1f;
    [SerializeField] private Color trailColor = new Color(0, 1, 1, 0.5f);
    
    private Material _material;
    private float _alpha = 1f;
    private Vector3 _targetScale;

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            _material = renderer.material;
            _material.color = trailColor;
        }
        
        _targetScale = transform.localScale * maxScale;
        
        CreateTrail();
    }

    private void Update()
    {
        _alpha -= fadeSpeed * Time.deltaTime;
        
        if (_material != null)
        {
            Color color = _material.color;
            color.a = _alpha;
            _material.color = color;
        }

        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, scaleSpeed * Time.deltaTime);

        if (_alpha <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void CreateTrail()
    {
        for (int i = 1; i <= trailCount; i++)
        {
            GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            trail.transform.position = transform.position - transform.forward * (i * trailSpacing);
            trail.transform.localScale = transform.localScale * (1f - (i * 0.15f));
            
            Renderer trailRenderer = trail.GetComponent<Renderer>();
            if (trailRenderer != null)
            {
                Material trailMat = new Material(_material);
                Color trailCol = trailColor;
                trailCol.a = _alpha * (1f - (i * 0.2f));
                trailMat.color = trailCol;
                trailRenderer.material = trailMat;
            }
            
            Destroy(trail.GetComponent<Collider>());
            Destroy(trail, 0.5f);
        }
    }

    private void OnDestroy()
    {
        if (_material != null)
        {
            Destroy(_material);
        }
    }
}
