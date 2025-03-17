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