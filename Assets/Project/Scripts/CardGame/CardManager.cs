using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<CardData> deckCards = new List<CardData>();
    public List<CardData> handCards = new List<CardData>();
    public List<CardData> discardCards = new List<CardData>();

    public GameObject cardPrefab; // 카드 프리팹을 참조하는 변수
    public Transform deckPosition; // 덱 위치를 참조하는 변수
    public Transform handPosition; // 손패 위치를 참조하는 변수
    public Transform discardPosition; // 버리는 카드 위치를 참조하는 변수

    public List <GameObject> cardObjects = new List<GameObject>();

    public CharacterStats playerStats;
    public CharacterStats enemyStats;
    private static CardManager instance;

    public static CardManager Instance
    {
        get
        {
            if (instance == null) instance = new CardManager();
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // 중복된 CardManager 제거
        }
    }


    void Start()
    {
       ShuffleDeck(); // 게임 시작 시 덱을 섞음

        // 2. 시작 시 카드 3장을 드로우합니다.
        for (int i = 0; i < 3; i++)
        {
            DrawCard();
        }
        ArrangeHand();
    }

    // Update is called once per frame
    void Update()
    {
       if (Input.GetKeyUp(KeyCode.D))
       {
           DrawCard();
       }

         if (Input.GetKeyUp(KeyCode.F))
         {
              ReturnDiscardsToDeck();
        }

         ArrangeHand(); // 매 프레임마다 손패 위치 업데이트
    }

    public void ShuffleDeck() // 덱을 섞는 메서드
    {
        List<CardData> tempDeck = new List<CardData>(deckCards); // 임시 덱 리스트를 생성하여 기존 덱의 카드를 복사
        deckCards.Clear();

        while (tempDeck.Count > 0) // 랜덤하게 섞기
        {
            int randomIndex = Random.Range(0, tempDeck.Count);
            deckCards.Add(tempDeck[randomIndex]);
            tempDeck.RemoveAt(randomIndex);
        }

        Debug.Log("덱을 섞었습니다: " + deckCards.Count + "장");
    }



    public void DrawCard()
    {
        // 1. 손패 제한 체크
        if (handCards.Count >= 6)
        {
            Debug.Log("손패가 가득 찼습니다!");
            return;
        }

        // 2. 덱이 비었을 때 체크
        if (deckCards.Count == 0)
        {
            Debug.Log("덱이 비어있습니다! 버린 카드를 섞거나 드로우를 중단합니다.");
            return; // 덱이 없으면 여기서 멈춤
        }

        // 3. 카드 종류가 모두 똑같은 문제 해결: 0번 카드를 가져오고 '제거'
        CardData cardData = deckCards[0];
        deckCards.RemoveAt(0); 

        // 4. 손패 데이터 리스트에 추가
        handCards.Add(cardData);

        GameObject cardObj = Instantiate(cardPrefab, handPosition.position, Quaternion.identity);

        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            cardDisplay.SetupCard(cardData);
            cardDisplay.cardIndex = cardObjects.Count;
            cardObjects.Add(cardObj);
        }

        // 6. 즉시 위치 갱신
        ArrangeHand();

        Debug.Log($"카드를 뽑았습니다: {cardData.cardName} (남은 덱: {deckCards.Count})");
    }

    public void ArrangeHand() //손에 있는 카드 재정렬
    {
        if (handCards.Count == 0) return;

        //손패 배치를 위한 변수
        float cardWidth = 1.2f;
        float spacing = cardWidth + 1.8f;
        float totalWidth = (cardObjects.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        //각 카드 위치 설정
        for (int i = 0; i < cardObjects.Count; i++)
        {
            if (cardObjects[i] != null)
            {
                //드래그 중인 카드는 건너뛰기
                CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();

                if (display != null && display.isDragging)
                    continue;

                //목표 위치 계산
                Vector3 targetPosition = handPosition.transform.position +
                         (handPosition.transform.right * (startX + (i * spacing)));

                //부드러운 이동
                cardObjects[i].transform.position = Vector3.Lerp(cardObjects[i].transform.position, targetPosition, Time.deltaTime * 10f);
            }
        }
    }

    public void DiscardCard(int handIndex) //카드 버리기(디스카드)
    {
        if (handIndex < 0 || handIndex >= handCards.Count)
        {
            Debug.Log("유효하지 않은 카드 인덱스 입니다");
            return;
        }

        CardData cardData = handCards[handIndex]; //손패에서 카드 가져오기
        handCards.RemoveAt(handIndex);

        discardCards.Add(cardData); //버린 카드 더미에 추가

        if (handIndex < cardObjects.Count) //해당 카드 게임 오브젝트 제거
        {
            Destroy(cardObjects[handIndex]);
            cardObjects.RemoveAt(handIndex);
        }

        for (int i = 0; i < cardObjects.Count; i++) //카드 인덱스 재설정
        {
            CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();
            if (display != null) display.cardIndex = i;
        }

        ArrangeHand(); //손패 위치 업데이트
        Debug.Log("카드를 버렸습니다. " + cardData.cardName);
    }

    //버린 카드를 덱으로 되돌리고 섞기
    public void ReturnDiscardsToDeck()
    {
        if (discardCards.Count == 0)
        {
            Debug.Log("버린 카드가 없습니다!");
            return;
        }

        deckCards.AddRange(discardCards); //버린 카드 더미를 덱으로 이동
        discardCards.Clear(); //버린 카드 더미 초기화
        ShuffleDeck(); //덱 섞기

        Debug.Log("버린 카드" + deckCards.Count + "장을 덱으로 되돌리고 섞었습니다.");
    }
}
