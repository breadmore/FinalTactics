using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera;
    public CinemachineCamera cinemachineCamera; // Cinemachine Virtual Camera
    public float moveSpeed = 10f; // ī�޶� �̵� �ӵ�
    public float edgeThreshold = 50f; // ȭ�� �� ���� ���� (�ȼ� ����)
    public float smoothTime = 0.2f; // ī�޶� �̵� �ε巯��
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private bool isMovingToTarget = false;

    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // ����Ŭ�� �ν� �ð� ����

    private void Start()
    {
        targetPosition = cinemachineCamera.transform.position; // virtualCamera�� �ʱ�ȭ
        if (cinemachineCamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera is not assigned!");
        }
    }

    private void Update()
    {
        if (cinemachineCamera == null) return;

        HandleMouseMovement();
        HandleDoubleClick();
        SmoothMoveToTarget();
    }

    private void HandleMouseMovement()
    {
        if (isMovingToTarget) return; // ��ǥ ��ġ�� �̵� ���̸� ���� ��Ȱ��ȭ

        Vector3 moveDirection = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector3 forward = cinemachineCamera.transform.forward;
        Vector3 right = cinemachineCamera.transform.right;

        if (mousePos.y >= screenHeight - edgeThreshold) moveDirection += new Vector3(forward.x, 0, forward.z);
        if (mousePos.y <= edgeThreshold) moveDirection -= new Vector3(forward.x, 0, forward.z);
        if (mousePos.x <= edgeThreshold) moveDirection -= new Vector3(right.x, 0, right.z);
        if (mousePos.x >= screenWidth - edgeThreshold) moveDirection += new Vector3(right.x, 0, right.z);

        cinemachineCamera.transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    private void HandleDoubleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float timeSinceLastClick = Time.time - lastClickTime;
            lastClickTime = Time.time;

            if (timeSinceLastClick <= doubleClickThreshold)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.Log("hit !!" + hit.collider.name);
                        PlayerCharacter playerCharacter = hit.collider.GetComponent<PlayerCharacter>();
                        if (playerCharacter != null)
                        {
                            GameManager.Instance.OnPlayerCharacterSelected(playerCharacter);
                            MoveCameraTo(playerCharacter.transform.position); // Ÿ�� ��ġ�� ī�޶� �̵�
                            cinemachineCamera.Follow = playerCharacter.transform; // Ÿ���� ���󰡵��� ����
                        }
                    }
                }
            }
        }
    }



    private void MoveCameraTo(Vector3 position)
    {
        // Ÿ���� ���� ��ǥ���� ī�޶��� ���ο� x ��ġ�� ���
        Vector3 newPosition = new Vector3(position.x + 4.5f, cinemachineCamera.transform.position.y, position.z);

        // ���ο� ��ġ�� targetPosition�� �Ҵ�
        targetPosition = newPosition;

        isMovingToTarget = true;
    }



    private void SmoothMoveToTarget()
    {
        if (!isMovingToTarget) return;

        // ī�޶� �� ������ targetPosition���� �̵��ϵ��� ��
        cinemachineCamera.transform.position = Vector3.Lerp(cinemachineCamera.transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // ���� ��ǥ ��ġ�� �����ϸ� �̵��� ���ߵ��� ����
        if (Vector3.Distance(cinemachineCamera.transform.position, targetPosition) < 0.1f)
        {
            isMovingToTarget = false;
            cinemachineCamera.transform.position = targetPosition; // ��Ȯ�� targetPosition�� ����
        }
    }

}
