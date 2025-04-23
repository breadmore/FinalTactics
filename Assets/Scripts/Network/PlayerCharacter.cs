using UnityEngine;
using Unity.Netcode;
using UnityEngine.Splines;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Services.Authentication;

public class PlayerCharacter : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> PlayerNickname = new(writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField]private TextMeshPro nameUIInstance;

    public Transform ballPosition;
    public ParticleSystem clickParticle;

    private PlayerCharacterAnim characterAnimator;

    private NetworkVariable<int> characterId = new NetworkVariable<int>();
    private NetworkVariable<int> stamina = new NetworkVariable<int>();
    public int Stamina => stamina.Value;
    
    public CharacterStat CharacterStat { get; private set; }

    private NetworkVariable<TeamName> team = new NetworkVariable<TeamName>();
    public TeamName Team => team.Value;

    private NetworkVariable<Vector2Int> gridPosition = new NetworkVariable<Vector2Int>();
    public Vector2Int GridPosition => gridPosition.Value;
    public int ShootChargeCount { get; private set; } = 0;

    private void Awake()
    {
        characterAnimator = GetComponent<PlayerCharacterAnim>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();


        PlayerNickname.OnValueChanged += (_, newVal) =>
        {
            SetNameTag(newVal.ToString(), true);
        };

        SetNameTag(PlayerNickname.Value.ToString(), true);


        characterId.OnValueChanged += OnCharacterIdChanged;
        SetCharacterStat();

        if (IsServer) gridPosition.OnValueChanged += OnGridPositionChanged;

        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        characterId.OnValueChanged -= OnCharacterIdChanged;

        if (IsServer) gridPosition.OnValueChanged -= OnGridPositionChanged; // 이벤트 구독 해제
        
    }

    public void Initialize(int characterId,TeamName team, Vector2Int gridPosition)
    {
        // 서버에서만 gridPosition 설정하도록 수정
        if (IsServer)
        {
            this.characterId.Value = characterId;
            this.team.Value = team;
            this.gridPosition.Value = gridPosition;
        }


    }

    public void SetNameTag(string playerName, bool isMyTeam)
    {
        nameUIInstance.text = playerName;
        nameUIInstance.color = isMyTeam ? Color.green : Color.red;
    }

    public void MoveToGridTile(GridTile targetTile)
    {
        GridTile currentTile = GridManager.Instance.GetGridTileAtPosition(GridPosition);

        int lostStamina = GridUtils.GetDistance(currentTile.gridPosition, targetTile.gridPosition);
        Debug.Log("Lost Stamina : " + lostStamina);
        currentTile.ClearOccupied();

        targetTile.SetOccupied(this);


        stamina.Value -= lostStamina;

        gridPosition.Value = targetTile.gridPosition;
        
    }

    private void OnGridPositionChanged(Vector2Int oldPos, Vector2Int newPos)
    {

        GridTile oldTile = GridManager.Instance.GetGridTileAtPosition(oldPos);
        oldTile.ClearOccupied();

        GridTile newTile = GridManager.Instance.GetGridTileAtPosition(newPos);
        newTile.SetOccupied(this);

        // 이동 시작 (부드럽게)
        Vector3 targetPosition = GridManager.Instance.GetNearestGridCenter(newTile.transform.position);
        StopAllCoroutines(); // 혹시 모를 중첩 방지
        StartCoroutine(SmoothMoveCoroutine(targetPosition));

        //transform.position = GridManager.Instance.GetNearestGridCenter(newTile.transform.position);
    }

    private IEnumerator SmoothMoveCoroutine(Vector3 targetPosition)
    {
        float duration = 0.8f; // 이동 시간
        float elapsed = 0f;

        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;

        // 이동 끝나면 idle
        PlayAnimationIdle();
        BallManager.Instance.dribbler = null;
    }


    // 서버 호출용
    [ServerRpc]
    private void UpdateGridTileServerRpc(Vector2Int currentTilePosition,Vector2Int targetTilePosition)
    {
        GridTile currentTile = GridManager.Instance.GetGridTileAtPosition(currentTilePosition);
        currentTile.ClearOccupied();

        GridTile targetTile = GridManager.Instance.GetGridTileAtPosition(targetTilePosition);
        targetTile.SetOccupied(this);
        SyncGridTileClientRpc(currentTilePosition,targetTilePosition);
    }

    [ClientRpc]
    private void SyncGridTileClientRpc(Vector2Int currentTilePosition, Vector2Int tilePosition)
    {
        if (IsHost) return; // 서버에서는 실행하지 않음

        GridTile currentTile = GridManager.Instance.GetGridTileAtPosition(currentTilePosition);
        currentTile.ClearOccupied();

        GridTile gridTile = GridManager.Instance.GetGridTileAtPosition(tilePosition);
        gridTile.SetOccupied(this);
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

    private void OnCharacterIdChanged(int oldId, int newId)
    {
        SetCharacterStat();

    }

    public void SetCharacterStat()
    {
        CharacterStat = LoadDataManager.Instance.characterDataReader.GetCharacterDataById(characterId.Value).characterStat;
        stamina.Value = CharacterStat.stamina;
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
