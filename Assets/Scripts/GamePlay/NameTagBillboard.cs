using UnityEngine;

public class NameTagBillboard : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        // ī�޶� ���ϰ� ȸ��
        transform.forward = mainCam.transform.forward;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(Vector3.Dot(transform.right, mainCam.transform.right)) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
}
