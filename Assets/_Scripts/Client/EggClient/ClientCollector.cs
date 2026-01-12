using UnityEngine;

public class ClientCollector : MonoBehaviour
{
    private PresentationController _presentationController;

    private void Start()
    {
        _presentationController = FindObjectOfType<PresentationController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var eggData = other.GetComponent<CollectibleVisualData>();
        if (eggData != null)
        {
            _presentationController.OnOptimisticCollect(eggData.ID);
        }
    }
}