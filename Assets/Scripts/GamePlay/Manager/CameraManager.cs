using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera;
    public CinemachineCamera cinemachineCamera; // Cinemachine Virtual Camera
    public float moveSpeed = 10f; // 카메라 이동 속도
    public float edgeThreshold = 50f; // 화면 끝 감지 범위 (픽셀 단위)
    public float smoothTime = 0.2f; // 카메라 이동 부드러움
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private bool isMovingToTarget = false;

    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // 더블클릭 인식 시간 간격

    private Vector3 initialCameraPosition;
    private void Start()
    {
        targetPosition = cinemachineCamera.transform.position; // virtualCamera로 초기화
        if (cinemachineCamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera is not assigned!");
        }

        initialCameraPosition = cinemachineCamera.transform.position;
    }

    private void Update()
    {
        if (cinemachineCamera == null) return;

        HandleMouseMovement();
        HandleDoubleClick();
        SmoothMoveToTarget();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetCameraPosition();
        }
    }

    private void ResetCameraPosition()
    {
        MoveCameraTo(initialCameraPosition);
    }

    private void HandleMouseMovement()
    {
        if (isMovingToTarget || !Application.isFocused) return; // 창이 비활성화되었으면 이동 X

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
                            MoveCameraTo(playerCharacter.transform.position); // 타겟 위치로 카메라 이동
                            cinemachineCamera.Follow = playerCharacter.transform; // 타겟을 따라가도록 설정
                        }
                    }
                }
            }
        }
    }



    private void MoveCameraTo(Vector3 position)
    {
        // 타겟의 월드 좌표에서 카메라의 새로운 x 위치를 계산
        Vector3 newPosition = new Vector3(position.x + 4.5f, cinemachineCamera.transform.position.y, position.z);

        // 새로운 위치를 targetPosition에 할당
        targetPosition = newPosition;

        isMovingToTarget = true;
    }



    private void SmoothMoveToTarget()
    {
        if (!isMovingToTarget) return;

        // 카메라가 딱 지정된 targetPosition으로 이동하도록 함
        cinemachineCamera.transform.position = Vector3.Lerp(cinemachineCamera.transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 만약 목표 위치에 도달하면 이동을 멈추도록 설정
        if (Vector3.Distance(cinemachineCamera.transform.position, targetPosition) < 0.1f)
        {
            isMovingToTarget = false;
            cinemachineCamera.transform.position = targetPosition; // 정확히 targetPosition에 도달
        }
    }

}
