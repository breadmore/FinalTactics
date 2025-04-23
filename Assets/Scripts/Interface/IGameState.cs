using System.Threading.Tasks;

public interface IGameState
{
    void HandleAllPlayersReady();
    void EnterState();
    void ExitState();
    void UpdateState();

    void OnCharacterDataSelected(CharacterData characterData);
    void OnGridTileSelected(GridTile gridTile);
    void OnPlayerCharacterSelected(PlayerCharacter playerCharacter);
    void OnActionSelected(ActionData actionData);
    void OnActionOptionSelected(ActionOptionData actionOptionData);


}
