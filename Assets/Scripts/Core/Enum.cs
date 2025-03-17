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