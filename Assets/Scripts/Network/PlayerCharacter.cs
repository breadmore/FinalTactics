using UnityEngine;
using Unity.Netcode;
using UnityEngine.Splines;
using System.Collections;

public class PlayerCharacter : NetworkBehaviour
{
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
            Debug.Log("Id : " + characterId);
            this.characterId.Value = characterId;
            this.team.Value = team;
            this.gridPosition.Value = gridPosition;
        }


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
