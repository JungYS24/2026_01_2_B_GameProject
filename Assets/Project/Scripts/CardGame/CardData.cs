using NUnit.Framework;
using System.Collections.Generic;
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


    //추가 효과 리스트
    public List<AdditionalEffect> additionalEffects = new List<AdditionalEffect>();

    public enum AdditionalEffectType //추가 효과 유형을 나타내는 열거형
    {
        None,
        DrawCard, // 카드 드로우 효과
        DiscardCard, // 카드 버리기 효과
        GainMana, // 마나 획득 효과
        ReduceEnemyMana, // 적의 마나 감소 효과
        ReduceCardCost, // 다음 카드 비용 감소 효과
    }

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

    //추가 효과 정보를 문자열로 변환
    public string GetAdditionalEffectDescription()
    {
        if (additionalEffects.Count == 0)
            return "";

        string result = "\n";

        foreach (var effect in additionalEffects)
        {
            result += effect.GetDescription() + "\n";
        }

        return result;
    }
}
