using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ItemDataLoader : MonoBehaviour
{
    // 이미지 내 변수명과 일치하게 선언
    public string jsonFileName = "ItemData";
    public List<ItemData> itemlist = new List<ItemData>();

    void Start()
    {
        LoadItemData();
    }

    // 한글 인코딩을 도와 주는 함수를 만든다.
    private string EncodeKorean(string text)
    {
        if (string.IsNullOrEmpty(text)) return ""; //텍스트가 NULL 값이면 함수를 끝낸다.

        byte[] bytes = Encoding.Default.GetBytes(text); //string 을 Byte 배열로 변환한 후
        return Encoding.UTF8.GetString(bytes);          //인코딩을 UTF8로 바꾼다.
    }

    // LoadItemData 함수를 만든다.
    void LoadItemData()
    {
        // TextAsset 형태로 Json 파일을 로딩한다.
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);

        if (jsonFile != null)
        {
            // 원본 텍스트에서 UTF8로 변환 처리
            byte[] bytes = Encoding.Default.GetBytes(jsonFile.text);
            string currnetText = Encoding.UTF8.GetString(bytes);

            // 변환 된 텍스트 사용
            itemlist = JsonConvert.DeserializeObject<List<ItemData>>(currnetText);

            Debug.Log($"로드된 아이템 수 : {itemlist.Count}");

            foreach (var item in itemlist)
            {
                // 변수명 itemName, description 에 맞게 출력
                Debug.Log($"아이템 : {EncodeKorean(item.itemName)}, 설명 : {EncodeKorean(item.description)}");
            }
        }
        else
        {
            Debug.LogError($"JSON 파일을 찾을 수 없습니다. : {jsonFileName}");
        }
    }
}