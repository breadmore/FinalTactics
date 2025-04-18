using Mono.CSharp;
using UnityEngine;

public enum ActionType
{
    None,        // 안함
    Move,
    Shoot,      // 슛
    Pass,       // 패스
    Dribble,     // 드리블
    Block,      // 블록
    Tackle,     // 태클
    Intercept,   // 인터셉트
    Save,       // 슛 방어
    Length
}
public enum ShootOption { Cancel, Charge, Shoot }

public enum CharacterType
{
    Striker,
    Defender,
    Goalkeeper,
    None
}

public enum CharacterStat2
{
    Reaction,
    Accuracy,
    Defense,
}

public enum LoadSceneType
{
    Intro,
    Loading,
    InGame,
    None
}

public enum TileType
{
    Neutral,    // 일반 이동 가능 타일
    SpawnZone,
    GoalkeeperZone, // 골키퍼 앞 제한 구역
    Occupied,   // 현재 플레이어가 점유 중인 타일
}

public enum TeamName
{
    TeamA,
    TeamB
}

public enum GameState
{
    // Selected State
    CharacterDataSelected,
    PlayerCharacterSelected,
    GridTileSelected,
    ActionSelected,

    // Game Flow State
    GameStarted,
    GameFinished,

    // Waiting State
    WaitingForPlayerReady,
    WaitingForSpawnBall,
    WaitingForOtherPlayerAction,
    WaitingForActionEnd,
    WaitingForReset,

    // Test State
    TestState

}
public enum ActionCategory 
{ 
    Common, 
    Offense,
    Defense,
    Keeper,
}
public static class GameConstants
{
    // 그리드 관련 상수
    public static readonly Vector3 CELL_SIZE = new Vector3(3f, 0f, 3.1f);
    public static readonly Vector2 GRID_SIZE = new Vector2(16, 10);

    // 타일 관련 상수
    public const int TeamAStartX = 3;  // A팀 시작 구역의 X좌표 기준
    public const int TeamBStartX = 11; // B팀 시작 구역의 X좌표 기준
    public const int MaxCharacterCount = 2;

    // 타일 타입 관련 상수
    public const string GoalkeeperZoneName = "GoalkeeperZone";  // 골키퍼 구역 이름

    // 게임 규칙 관련 상수
    public const int MaxScore = 3;  // 승리 조건 (3골)

    // 스탯 관련
    public const int BlockDistance = 1;
    public const int TackleDistance = 1;
}