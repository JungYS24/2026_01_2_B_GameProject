using UnityEngine;

[System.Serializable]
public class AdditionalEffect
{
    public CardData.AdditionalEffectType effectType; // 추가 효과 유형
    public int effectAmount; // 추가 효과의 수치 (예: 드로우할 카드 수, 마나 획득량 등)

    public string GetDescription()
    {
        switch (effectType)
        {
            case CardData.AdditionalEffectType.DrawCard:
                return $"카드 {effectAmount}장 뽑기.";
            case CardData.AdditionalEffectType.DiscardCard:
                return $"카드 {effectAmount}장 버리기.";
            case CardData.AdditionalEffectType.GainMana:
                return $"마나 {effectAmount} 획득.";
            case CardData.AdditionalEffectType.ReduceEnemyMana:
                return $"적의 마나 {effectAmount} 감소.";
            case CardData.AdditionalEffectType.ReduceCardCost:
                return $"다음 카드의 비용 {effectAmount} 감소.";
            default:
                return "추가 효과 없음.";
        }
    }
}
