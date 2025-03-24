using Mono.CSharp;
using UnityEngine;

public enum AttackAction
{
    Shoot,      // 슛
    Pass,       // 패스
    Dribble     // 드리블
}

public enum DefenseAction
{
    Block,      // 블록
    Tackle,     // 태클
    Intercept   // 인터셉트
}

public enum GoalkeeperAction
{
    Save,       // 슛 방어
    Punch,      // 펀칭
    Catch       // 공 잡기
}

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
    Main,
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

public enum PhaseType
{
    Placement,      // 플레이어가 캐릭터를 Grid에 배치하는 턴 (배치 완료 시 게임 시작)
    Decision,       // 각 플레이어가 행동을 결정하는 턴
    Execution,      // 모든 결정이 끝난 후 행동을 실행하는 턴
    Resolution,     // 행동 결과를 반영하고 표시하는 턴

    TurnStart,      // 새 턴이 시작되었음을 알리는 턴 (버프/디버프 적용 등)
    TurnEnd,        // 현재 턴이 끝났음을 알리는 턴 (상태 정리, 다음 턴으로 넘어가기 전 처리)
    Pause,          // 일시 정지 상태 (네트워크 대기, UI 연출 등)
    GameEnd         // 게임 종료 (승패 판정)
}


public static class GameConstants
{
    // 그리드 관련 상수
    public static readonly Vector3 CELL_SIZE = new Vector3(3f, 0f, 3.1f);
    public static readonly Vector2 GRID_SIZE = new Vector2(14, 8);  // 14x8 그리드 크기

    // 타일 관련 상수
    public const int TeamAStartX = 3;  // A팀 시작 구역의 X좌표 기준
    public const int TeamBStartX = 11; // B팀 시작 구역의 X좌표 기준
    public const int MaxCharacterCount = 2;

    // 타일 타입 관련 상수
    public const string GoalkeeperZoneName = "GoalkeeperZone";  // 골키퍼 구역 이름

    // 게임 규칙 관련 상수
    public const int MaxScore = 3;  // 승리 조건 (3골)
}