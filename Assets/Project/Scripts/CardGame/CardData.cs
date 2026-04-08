using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/CardData")]

public class CardData : ScriptableObject 
{
   public enum CardType
   {
       Attack,
       Heal,
       Buff,
       Utility
    }

    public string cardName; // 카드의 이름
    public string description; // 카드의 설명
    public Sprite artwork; // 카드의 아트워크 이미지
    public int manaCost; // 카드의 마나 비용
    public int effectAmount; // 카드의 효과 수치 (예: 공격력, 회복량 등)
    public CardType cardType; // 카드의 유형을 나타내는 열거형

    public Color GetCardColor()
    {
        switch (cardType)
        {
            case CardType.Attack:
                return Color.red; // 공격 카드의 색상
            case CardType.Heal:
                return Color.green; // 회복 카드의 색상
            case CardType.Buff:
                return Color.blue; // 버프 카드의 색상
            case CardType.Utility:
                return Color.yellow; // 유틸리티 카드의 색상
            default:
                return Color.white; // 기본 색상
        }
    }
}
