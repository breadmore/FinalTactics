using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CameraManager : Singleton<CameraManager>
{
    [Header("Camera References")]
    public Camera mainCamera;
    public CinemachineCamera cinemachineCamera; // 기본 카메라
    public CinemachineCamera actionCamera;      // 액션용 카메라 (우선순위 낮게 설정해두기)

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float edgeThreshold = 50f;
    public float smoothTime = 0.2f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private bool isMovingToTarget = false;

    [Header("Double Click")]
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f;

    [Header("Initial Setup")]
    public List<TeamCameraProfile> teamCameraProfiles;
    private TeamName playerTeam;

    public Vector3 initialCameraPosition { get; private set; }

    [Header("Action Camera Settings")]
    public float actionCameraDuration = 2f;

    private PlayerCharacter focusPlayer;

    private void Start()
    {
        if (cinemachineCamera == null)
            Debug.LogError("CinemachineCamera is not assigned!");

        if (actionCamera == null)
            Debug.LogWarning("Action camera is not assigned. Action focus will not work.");
    }

    private void Update()
    {
        if (cinemachineCamera == null) return;

        HandleMouseMovement();
        HandleDoubleClick();
        SmoothMoveToTarget();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetCameraToInitial(GameManager.Instance.thisPlayerBrain.GetMyTeam());

            if (InGameUIManager.Instance.gameDataPanel.IsVisible())
                InGameUIManager.Instance.gameDataPanel.Close();
        }
    }

    #region === 초기 위치 세팅 ===
    public void SetInitialCameraPosition(TeamName team)
    {
        var profile = teamCameraProfiles.FirstOrDefault(p => p.team == team);
        if (profile != null)
        {
            cinemachineCamera.transform.position = profile.cameraPosition;
            cinemachineCamera.transform.rotation = Quaternion.Euler(profile.cameraRotation);
            initialCameraPosition = profile.cameraPosition;
            playerTeam = profile.team;
        }
        else
        {
            Debug.LogWarning("No camera profile found for team: " + team);
        }
    }

    private void ResetCameraToInitial(TeamName team)
    {
        MoveCameraTo(initialCameraPosition);
    }
    #endregion

    #region === 마우스 이동 / 더블클릭 ===
    private void HandleMouseMovement()
    {
        if (isMovingToTarget || !Application.isFocused) return;

        Vector3 moveDirection = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        Vector3 forward = cinemachineCamera.transform.forward;
        Vector3 right = cinemachineCamera.transform.right;

        if (mousePos.y >= Screen.height - edgeThreshold) moveDirection += new Vector3(forward.x, 0, forward.z);
        if (mousePos.y <= edgeThreshold) moveDirection -= new Vector3(forward.x, 0, forward.z);
        if (mousePos.x <= edgeThreshold) moveDirection -= new Vector3(right.x, 0, right.z);
        if (mousePos.x >= Screen.width - edgeThreshold) moveDirection += new Vector3(right.x, 0, right.z);

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
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Player"))
                {
                    PlayerCharacter character = hit.collider.GetComponent<PlayerCharacter>();
                    if (character != null)
                    {
                        GameManager.Instance.OnPlayerCharacterSelected(character);
                        FocusCameraOnCharacter(character);
                        InGameUIManager.Instance.gameDataPanel.OpenWithCharacterData(character);
                    }
                }
            }
        }
    }
    #endregion

    #region === 일반 카메라 이동 ===
    private void MoveCameraTo(Vector3 position)
    {
        Vector3 newPosition = new Vector3(position.x, cinemachineCamera.transform.position.y, position.z);
        targetPosition = newPosition;
        isMovingToTarget = true;
    }

    private void SmoothMoveToTarget()
    {
        if (!isMovingToTarget) return;

        cinemachineCamera.transform.position = Vector3.Lerp(cinemachineCamera.transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(cinemachineCamera.transform.position, targetPosition) < 0.1f)
        {
            cinemachineCamera.transform.position = targetPosition;
            isMovingToTarget = false;
        }
    }
    #endregion

    #region === 액션 카메라 연출 ===
    public void PlayActionCamera(PlayerCharacter targetCharacter)
    {
        if (actionCamera == null) return;

        actionCamera.Follow = targetCharacter.transform;
        actionCamera.LookAt = targetCharacter.transform;
        actionCamera.Priority = 20; // 기본 카메라보다 높게

        StartCoroutine(ResetActionCameraAfterDelay());
    }

    private IEnumerator ResetActionCameraAfterDelay()
    {
        yield return new WaitForSeconds(actionCameraDuration);
        actionCamera.Priority = 5; // 다시 비활성화
    }

    private void FocusCameraOnCharacter(PlayerCharacter character)
    {
        if(focusPlayer != null && focusPlayer.clickParticle != null)
        {

            focusPlayer.clickParticle.gameObject.SetActive(false);
            focusPlayer = null;
        }

        focusPlayer = character;

        if (focusPlayer.clickParticle != null)
        {
            focusPlayer.clickParticle.gameObject.SetActive(true);
        }

        Vector3 characterPosition = character.transform.position;

        // 팀에 따라 x축 오프셋 설정
        float xOffset = 6f;
        if (playerTeam == TeamName.TeamB)
        {
            xOffset *= -1f; // 반대 방향
        }

        // 카메라 위치는 캐릭터 기준으로 xOffset만큼 옮긴 위치

        Vector3 targetPosition = new Vector3(
            characterPosition.x + xOffset,
            cinemachineCamera.transform.position.y, // 기존 높이 유지
            characterPosition.z
        );

        MoveCameraTo(targetPosition);
    }

    //Focus Off
    private void TurnEndSetting()
    {
        Debug.Log("Turn End!");
        if (focusPlayer != null && focusPlayer.clickParticle != null)
        {
            focusPlayer.clickParticle.gameObject.SetActive(false);
            focusPlayer = null;
           
        }
    }


    public void RegisterTurnCallbacks(TurnManager turnManager)
    {
        turnManager.OnTurnEnd += TurnEndSetting;
    }
    #endregion
}
