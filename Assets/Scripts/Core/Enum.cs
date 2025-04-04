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
    Save       // �� ���
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
    WaitingForPlayerReady,
    CharacterDataSelected,
    PlayerCharacterSelected,
    GridTileSelected,
    ActionSelected,
    GameStarted,
    GameFinished,
    WaitingForSpawnBall,
    WaitingForOtherPlayerAction,
    WaitingForActionEnd,

    // Test State
    TestState

}

public enum ActionCategory 
{ 
    Defense, 
    Common, 
    Offense 
}
public static class GameConstants
{
    // �׸��� ���� ���
    public static readonly Vector3 CELL_SIZE = new Vector3(3f, 0f, 3.1f);
    public static readonly Vector2 GRID_SIZE = new Vector2(14, 8);  // 14x8 �׸��� ũ��

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
    public const int TackleDistance = 2;
}