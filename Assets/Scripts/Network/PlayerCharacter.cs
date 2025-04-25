using UnityEngine;
using Unity.Netcode;
using UnityEngine.Splines;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Services.Authentication;
using Cysharp.Threading.Tasks;
using WebSocketSharp;
using System;

public class PlayerCharacter : NetworkBehaviour
{
    public static event Action<PlayerCharacter> OnSpawned;

    public NetworkVariable<FixedString32Bytes> PlayerNickname = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<int> characterId = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<int> stamina = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<TeamName> team = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<Vector2Int> gridPosition = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);

    public int Stamina => stamina.Value;
    public TeamName Team => team.Value;
    public Vector2Int GridPosition => gridPosition.Value;

    [SerializeField]private TextMeshPro nameUIInstance;
    public Transform ballPosition;
    public ParticleSystem clickParticle;
    private PlayerCharacterAnim characterAnimator;
    public CharacterStat CharacterStat { get; private set; }
    public int ShootChargeCount { get; private set; } = 0;

    private void Awake()
    {
        characterAnimator = GetComponent<PlayerCharacterAnim>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsSpawned) OnSpawned?.Invoke(this);

        if (IsServer)
        {
            gridPosition.OnValueChanged += OnGridPositionChanged;
        }

            PlayerNickname.OnValueChanged += HandlePlayerNicknameChange;

        characterId.OnValueChanged += HandleCharacterIdChange;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            gridPosition.OnValueChanged -= OnGridPositionChanged;
        }


    }

    public void Initialize(int characterId, string nickname, TeamName team, Vector2Int gridPosition)
    {
        this.characterId.Value = characterId;
        this.PlayerNickname.Value = nickname;
        this.team.Value = team;
        this.gridPosition.Value = gridPosition;
        stamina.Value = LoadDataManager.Instance.characterDataReader
                      .GetCharacterDataById(characterId).characterStat.stamina;
    }


    public void UpdateNameTag(FixedString32Bytes playerName)
    {
        bool isMyTeam = IsOwner || (Team == GetLocalTeam());
        nameUIInstance.text = playerName.ToString();
        nameUIInstance.color = isMyTeam ? Color.green : Color.red;
    }

    public void HandlePlayerNicknameChange(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        UpdateNameTag(newName);
    }

    private TeamName GetLocalTeam()
    {
        return GameManager.Instance?.thisPlayerBrain?.GetMyTeam() ?? TeamName.None;
    }

    public void MoveToGridTile(GridTile targetTile)
    {
        if (!IsServer) return;

        GridTile currentTile = GridManager.Instance.GetGridTileAtPosition(GridPosition);

        int lostStamina = GridUtils.GetDistance(currentTile.gridPosition, targetTile.gridPosition);
        stamina.Value -= lostStamina;
        gridPosition.Value = targetTile.gridPosition;
    }

    private async void OnGridPositionChanged(Vector2Int oldPos, Vector2Int newPos)
    {
        try
        {
            GridTile newTile = GridManager.Instance.GetGridTileAtPosition(newPos);
            GridTile oldTile = GridManager.Instance.GetGridTileAtPosition(oldPos);

            if (newTile == null || oldTile == null) return;

            // 새로운 타일 점유 (이동 시작 전)
            newTile.SetOccupiedClientRpc(NetworkObjectId);

            // 이동 실행
            await SmoothMoveAsync(GridManager.Instance.GetNearestGridCenter(newTile.transform.position));

            // 이전 타일 해제 (이동 완료 후)
            oldTile.ClearOccupiedClientRpc(NetworkObjectId);
        }
        finally
        {
            PlayAnimationIdle();
            if (IsOwner)
            {
                BallManager.Instance.dribbler = null;
            }
        }
    }

    private async UniTask SmoothMoveAsync(Vector3 targetPosition)
    {
        float duration = 0.8f; // 이동 시간
        float elapsed = 0f;

        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            await UniTask.Yield();
        }

        transform.position = targetPosition;

        // 이동 끝나면 idle
        PlayAnimationIdle();

        BallManager.Instance.dribbler = null;
    }

    public void ChargeShoot()
    {
        if (ShootChargeCount < 3)
            ShootChargeCount++;
    }

    public void ResetShootCharge()
    {
        ShootChargeCount = 0;
    }

    public void ResetPlayerCharacter()
    {
        ResetShootCharge();
    }

    private void HandleCharacterIdChange(int oldId, int newId)
    {
        SetCharacterStat(newId);
    }

    private void SetCharacterStat(int characterId)
    {
        try
        {
            var data = LoadDataManager.Instance.characterDataReader
                     .GetCharacterDataById(characterId);
            if (data != null)
            {
                CharacterStat = data.characterStat;
                Debug.Log(CharacterStat.speed);
                Debug.Log(CharacterStat.shoot);
                Debug.Log(CharacterStat.tackle);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Character stat load failed: {e.Message}");
        }
    }

    public int GetCharacterId()
    {
        return characterId.Value;
    }

    public void OnActionFailed()
    {
        Debug.Log("Character ["+ characterId + "] Action Failed");
    }

    #region Animation Methods
    public void PlayAnimationPass() => characterAnimator.PlayAnimationPass();
    public void PlayAnimationMove() => characterAnimator.PlayAnimationMove();
    public void PlayAnimationDribble() => characterAnimator.PlayAnimationDribble();
    public void PlayAnimationShoot() => characterAnimator.PlayAnimationShoot();
    public void PlayAnimationTackle() => characterAnimator.PlayAnimationTackle();
    public void PlayAnimationBlock() => characterAnimator.PlayAnimationBlock();
    public void PlayAnimationReceive() => characterAnimator.PlayAnimationReceive();
    public void PlayAnimationIdle() => characterAnimator.PlayAnimationIdle();
    public void PlayAnimationTrip() => characterAnimator.PlayAnimationTrip();
    #endregion
}
