using Mono.CSharp;
using UnityEngine;

public enum ActionType
{
    None,        // ����
    Move,
    Shoot,      // ��
    Pass,       // �н�
    Dribble,     // �帮��
    Block,      // ���
    Tackle,     // ��Ŭ
    Intercept,   // ���ͼ�Ʈ
    Save,       // �� ���
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
    Neutral,    // �Ϲ� �̵� ���� Ÿ��
    SpawnZone,
    GoalkeeperZone, // ��Ű�� �� ���� ����
    Occupied,   // ���� �÷��̾ ���� ���� Ÿ��
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
    // �׸��� ���� ���
    public static readonly Vector3 CELL_SIZE = new Vector3(3f, 0f, 3.1f);
    public static readonly Vector2 GRID_SIZE = new Vector2(16, 10);

    // Ÿ�� ���� ���
    public const int TeamAStartX = 3;  // A�� ���� ������ X��ǥ ����
    public const int TeamBStartX = 11; // B�� ���� ������ X��ǥ ����
    public const int MaxCharacterCount = 2;

    // Ÿ�� Ÿ�� ���� ���
    public const string GoalkeeperZoneName = "GoalkeeperZone";  // ��Ű�� ���� �̸�

    // ���� ��Ģ ���� ���
    public const int MaxScore = 3;  // �¸� ���� (3��)

    // ���� ����
    public const int BlockDistance = 1;
    public const int TackleDistance = 1;
}