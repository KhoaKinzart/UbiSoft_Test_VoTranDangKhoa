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
        // Kiểm tra xem có chạm vào trứng không
        var eggData = other.GetComponent<CollectibleVisualData>();
        if (eggData != null)
        {
            // Báo cho PresentationController biết để ẩn ngay lập tức (Lạc quan)
            _presentationController.OnOptimisticCollect(eggData.ID);
        }
    }
}