using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
   public CardData cardData; // 카드 데이터를 참조하는 변수
    public int cardIndex; //손패에서의 인덱스 번호

    public MeshRenderer cardRenderer; // 카드의 MeshRenderer 컴포넌트
    public TextMeshPro nameText; // 카드 이름을 표시하는 TextMeshPro 컴포넌트
    public TextMeshPro costText; // 카드 비용을 표시하는 TextMeshPro 컴포넌트
    public TextMeshPro attackText; // 카드 공격력을 표시하는 TextMeshPro 컴포넌트 
    public TextMeshPro descriptionText; // 카드 설명을 표시하는 TextMeshPro 컴포넌트

    //카드 상태
    private bool isDragging = false; 
    private Vector3 originalPosition; // 드래그 전 원본위치 저장

    //레이어 마스크
    public LayerMask enemyLayer;
    public LayerMask playerLayer;


    public void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");

        SetupCard(cardData);
    }

    //카드 데이터 설정
    public void SetupCard(CardData data)
    {
        cardData = data;

        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.manaCost.ToString();
        if (attackText != null) attackText.text = data.effectAmount.ToString();
        if (descriptionText != null) descriptionText.text = data.description;

        // 카드 텍스처 설정 (Sprite에서 Texture를 가져올 때)
        if (cardRenderer != null && data.artwork != null)
        {
            Material cardMaterial = cardRenderer.material;
            cardMaterial.mainTexture = data.artwork.texture;
        }
        Debug.Log(data.cardName + " 카드를 설정합니다!");
    }

    private void OnMouseDown()
    {
        //드래그 시작 시 원래 위치 저장
        originalPosition = transform.position;
        isDragging = true;
        Debug.Log("마우스 클릭됨!");
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            // 카메라로부터 카드까지의 거리를 계산하여 입력
            mousePos.z = Camera.main.WorldToScreenPoint(originalPosition).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // Z값은 기존의 위치를 유지하되, X와 Y만 마우스 월드 좌표로 업데이트
            transform.position = new Vector3(worldPos.x, worldPos.y, originalPosition.z);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //카드 사용 판정
        bool cardUsed = false;

        //적 위에 드롭 했는지 검사
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            Debug.Log("적에게 카드를 사용했습니다!");
            //적에게 공격 효과 적용
            CharacterStats enemyStatus = hit.collider.GetComponent<CharacterStats>();
            if (enemyStatus != null)
            {
                if (cardData.cardType == CardData.CardType.Attack)//카드 효과에 따라 다르게 적용
                {
                    enemyStatus.TakeDamage(cardData.effectAmount); //공격카드면 데미지 추가
                    Debug.Log(enemyStatus.characterName + "에게 " + cardData.effectAmount + "의 피해를 입혔습니다!");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 적에게 사용할 수 없습니다");
                }
            }
                
        }
        //아군 위에 드롭 했는지 검사
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            CharacterStats playerStats = hit.collider.GetComponent<CharacterStats>();

            if (playerStats != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    //힐카드면 회복하기
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 플레이어의 체력을 {cardData.effectAmount} 회복했습니다. ");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 플레이어에게 사용할 수 없습니다. ");
                }
            }
        }

        //카드를 사용하지 않았다면 원래 위치로 돌아감
        if (!cardUsed)
        {
            transform.position = originalPosition;
        }
        else
        {
            Destroy(gameObject); //카드 사용 후 카드 오브젝트 제거 (필요에 따라 수정)
        }
    }
}
