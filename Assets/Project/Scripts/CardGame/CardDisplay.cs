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
    public bool isDragging = false;
    private Vector3 originalPosition; // 드래그 전 원본위치 저장
    private float zDistance;

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
        originalPosition = transform.position;
        // 카메라에서 카드까지의 월드 거리(Z값 차이)를 미리 저장
        zDistance = Camera.main.WorldToScreenPoint(transform.position).z;
        isDragging = true;
        Debug.Log("마우스 클릭됨!");
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = zDistance; // 저장된 거리 사용
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            transform.position = new Vector3(worldPos.x, worldPos.y, originalPosition.z);
        }
    }

    private void OnMouseUp()
    {
        if (CardManager.Instance.playerStats == null || CardManager.Instance.playerStats.currentMana < cardData.manaCost) //마나 검사
        {
            Debug.Log($"마나가 부족합니다.! (필요 : {cardData.manaCost} , 현재 : {CardManager.Instance.playerStats.currentMana})");
            transform.position = originalPosition;
            return;
        }

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
        // 아군 위에 드롭 했는지 검사
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            CharacterStats playerStats = hit.collider.GetComponent<CharacterStats>();

            if (playerStats != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    // 힐카드면 회복하기
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
        else if (CardManager.Instance != null)
        {
            //버린 카드 더미 근처에 드롭 했는지 검사
            float distToDiscard = Vector3.Distance(transform.position, CardManager.Instance.discardPosition.position);
            if (distToDiscard < 2.0f)
            {
                //카드를 버리기
                CardManager.Instance.DiscardCard(cardIndex);
                return;
            }
        }

        //카드를 사용하지 않으면 원래 위치로 되돌리기
        if(!cardUsed)
        {
            transform.position = originalPosition;
            CardManager.Instance.ArrangeHand(); 
            Debug.Log("카드가 원래 위치로 돌아갑니다.");
        }
        else
        {
            //카드를 사용했다면 버린카드 더미로 이동
            if (CardManager.Instance != null)
            CardManager.Instance.DiscardCard(cardIndex);
           
            //카드 사용시 마나소모
            CardManager.Instance.playerStats.UseMana(cardData.manaCost);
            Debug.Log($"{cardData.cardName} 카드를 사용하여 플레이어의 마나를 {cardData.manaCost} 소모했습니다. (남은 마나 : {CardManager.Instance.playerStats.currentMana})");
        }
    }
}