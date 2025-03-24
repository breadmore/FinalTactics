using Mono.CSharp;
using UnityEngine;

public enum AttackAction
{
    Shoot,      // ��
    Pass,       // �н�
    Dribble     // �帮��
}

public enum DefenseAction
{
    Block,      // ���
    Tackle,     // ��Ŭ
    Intercept   // ���ͼ�Ʈ
}

public enum GoalkeeperAction
{
    Save,       // �� ���
    Punch,      // ��Ī
    Catch       // �� ���
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

public enum PhaseType
{
    Placement,      // �÷��̾ ĳ���͸� Grid�� ��ġ�ϴ� �� (��ġ �Ϸ� �� ���� ����)
    Decision,       // �� �÷��̾ �ൿ�� �����ϴ� ��
    Execution,      // ��� ������ ���� �� �ൿ�� �����ϴ� ��
    Resolution,     // �ൿ ����� �ݿ��ϰ� ǥ���ϴ� ��

    TurnStart,      // �� ���� ���۵Ǿ����� �˸��� �� (����/����� ���� ��)
    TurnEnd,        // ���� ���� �������� �˸��� �� (���� ����, ���� ������ �Ѿ�� �� ó��)
    Pause,          // �Ͻ� ���� ���� (��Ʈ��ũ ���, UI ���� ��)
    GameEnd         // ���� ���� (���� ����)
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
}