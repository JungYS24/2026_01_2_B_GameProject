using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    // 엑셀에서 가져올 핵심 데이터
    public int id;
    public string itemName;
    public string description;
    public string nameEng;
    public string itemTypeString;

    // 프로그램 로직에서 사용할 데이터 (직렬화 제외)
    [NonSerialized]
    public ItemType itemType;
    public int price;
    public int power;
    public int level;
    public bool isStackable;
    public string iconPath;

    // 문자열을 열거형으로 변환 하는 메서드
    public void InitializeEnums()
    {
        if (Enum.TryParse(itemTypeString, out ItemType parsedType))
        {
            itemType = parsedType;
        }
        else
        {
            Debug.LogError($"아이템 '{itemName}' 에 유효하지 않은 아이템 타입 : {itemTypeString}");
            // 기본값 설정
            itemType = ItemType.Consumable;
        }
    }
}