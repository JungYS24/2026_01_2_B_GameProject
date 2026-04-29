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

        //3D 텍스트 업데[이트
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

        //카드 설명 텍스트에 추가효과 설명 추가
        if(descriptionText != null)
        {
           descriptionText.text = data.description + data.GetAdditionalEffectDescription();
        }
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
       isDragging = false;

        //버린 카드 더미 근처 드롭 했는지 검사
        if (CardManager.Instance != null)
        {
            float disToDiscard = Vector3.Distance(transform.position, CardManager.Instance.discardPosition.position);
            if (disToDiscard < 2.0f)
            {
               CardManager.Instance.DiscardCard(cardIndex);  //마나 소모 없이 카드 버리기
                return;
            }
        }

        //카드 사용 로직 (마나 체크)
        if (CardManager.Instance.playerStats != null && CardManager.Instance.playerStats.currentMana < cardData.manaCost)
        {
            Debug.Log($"마나가 부족합니다! (필요 : {cardData.manaCost} , 현재 : {CardManager.Instance.playerStats?.currentMana ?? 0})");
            transform.position = originalPosition;
            return;
        }

        //레이캐스트로 타겟 감지
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //카드 사용 판정 지역변수
        bool cardUsed = false;

        //적 위에 드롭 했는지 검사
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();        //적에게 공격 효과 적용

            if (enemyStats != null)
            {
                if (cardData.cardType == CardData.CardType.Attack)
                {
                    enemyStats.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 적에게 {cardData.effectAmount} 데미지를 입혔습니다.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 적에게 사용 할 수 없습니다. ");
            }
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
           if(CardManager.Instance.playerStats != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    CardManager.Instance.playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 플레이어가 {cardData.effectAmount} 만큼 회복했습니다.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 플레이어에게 사용 할 수 없습니다. ");
            }
        }

        if(!cardUsed)
        {
            transform.position = originalPosition;
                if(CardManager.Instance != null)
                CardManager.Instance.ArrangeHand(); //카드를 사용하지 않았다면 원래 위치로 되돌리기

                return;
        }

        //카드 사용시 마나소모
        CardManager.Instance.playerStats.UseMana(cardData.manaCost);
        Debug.Log($"{cardData.manaCost} 마나를 사용했습니다. (남은 마나 : {CardManager.Instance.playerStats.currentMana})");

        //추가효과가 있는 경우 처리
        if (cardData.additionalEffects != null && cardData.additionalEffects.Count > 0)
        {
            ProcessAdditionalEffectsAndDiscard();
        }
        else
        {
            //카드 사용 후 버리기
            if (CardManager.Instance != null)
            {
                CardManager.Instance.DiscardCard(cardIndex);
            }
        }

    }

    public void ProcessAdditionalEffectsAndDiscard()
    {
        //카드 데이터 및 인덱스 보존
        CardData cardDataCopy = cardData;
        int cardIndexCopy = cardIndex;

        //추가 효과 적용
        foreach (var effect in cardDataCopy.additionalEffects)
        {
            switch (effect.effectType)
            {
                case CardData.AdditionalEffectType.DrawCard:
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (CardManager.Instance != null)
                        {
                            CardManager.Instance.DrawCard();
                        }
                    }
                    Debug.Log($"{effect.effectAmount} 장의 카드를 드로우 했습니다.");
                    break;

                case CardData.AdditionalEffectType.DiscardCard: //카드 버리기 구현 (랜덤버리기)
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (CardManager.Instance != null && CardManager.Instance.handCards.Count > 0)
                        {
                            int randomIndex = UnityEngine.Random.Range(0, CardManager.Instance.handCards.Count);

                            Debug.Log($"랜덤 카드 버리기 : 선택된 인덱스 {randomIndex} , 현재 손패 크기 : {CardManager.Instance.handCards.Count}");

                            if (randomIndex < CardManager.Instance.handCards.Count)
                            {
                                CardManager.Instance.DiscardCard(randomIndex); //선택된 카드 버리기

                                if (randomIndex < cardIndexCopy)
                                {
                                    cardIndexCopy--;
                                    Debug.Log($"버린 카드 인덱스 {randomIndex} 가 현재 카드 인덱스 {cardIndexCopy + 1} 보다 작아서 현재 카드 인덱스를 1 감소시킵니다. (현재 카드 인덱스 : {cardIndexCopy})");
                                }
                            }
                        }
                    }
                    // break는 for 루프가 끝난 뒤에 위치해야 모든 장수를 버립니다.
                    Debug.Log($"{effect.effectAmount} 장의 카드를 버렸습니다.");
                    break;

                case CardData.AdditionalEffectType.GainMana:

                    if (CardManager.Instance.playerStats != null)
                    {
                        CardManager.Instance.playerStats.GainMana(effect.effectAmount);
                        Debug.Log($"마나를 {effect.effectAmount} 획득 했습니다.");
                    }
                    break;

                case CardData.AdditionalEffectType.ReduceEnemyMana:

                    if (CardManager.Instance.enemyStats != null)
                    {
                        CardManager.Instance.enemyStats.UseMana(effect.effectAmount);
                        Debug.Log($"적이 마나를 {effect.effectAmount} 잃었습니다.");
                    }
                    break;
            }
        }
    }
}
