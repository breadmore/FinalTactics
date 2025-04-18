using Unity.Netcode;
using UnityEngine;

public class CharacterSlotParent : BaseLayoutGroupParent<CharacterSlotChild>
{
    private int characterCount = 0;
    public CharacterSlotChild selectedChild;
    private void OnEnable()
    {
        characterCount = 0;
    }
    private void Start()
    {
        CreateChild(8);
        for (int i = 0; i < 8; i++)
        {
            InitChild(i);
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.CharacterDataSelected && Input.GetMouseButtonDown(0))
        {
            Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Grid"))
                {
                    if (GameManager.Instance.SelectedCharacterData != null)
                    {
                        GameManager.Instance.OnGridTileSelected(hit.collider.GetComponent<GridTile>());
                        // 배치 불가능할 경우 return
                        if (!GameManager.Instance.SelectedGridTile.CanPlaceCharacter()) return;
                        GameManager.Instance.thisPlayerBrain.SpawnPlayer(GameManager.Instance.SelectedGridTile);
                        selectedChild.gameObject.SetActive(false);
                        // spawn 성공시 아래 작업
                        characterCount++;

                        CheckCharacterLimit(); // 캐릭터 수 제한 확인
                    }
                    else
                    {
                        Debug.LogError("No character selected!");
                    }
                }
                else
                {
                    Debug.LogError("No Grid Selected");
                }
            }
        }
    }

    private void InitChild(int index)
    {
        if (LoadDataManager.Instance.characterSlotBackgrounds == null || LoadDataManager.Instance.characterDataReader == null)
        {
            Debug.LogError("Data is not assigned!");
            return;
        }

        CharacterData characterData = LoadDataManager.Instance.characterDataReader.DataList[index];

        childList[index].SetCharacterData(characterData);
        childList[index].SetcharacterSprite(LoadDataManager.Instance.characterSlotBackgrounds.GetBackground(index));
    }

    private void CheckCharacterLimit()
    {
        if (characterCount >= GameConstants.MaxCharacterCount)
        {
            gameObject.SetActive(false);
        }
    }
}
