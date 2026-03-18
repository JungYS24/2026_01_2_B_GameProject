#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class JsonToScriptableConverter : EditorWindow
{
    // 변수 선언 (중복 선언 제거 및 이름 통일)
    private string jsonFilePath = "";                                   // JSON 파일 경로
    private string outputFolder = "Assets/ScriptableObjects/Items";     // 출력 폴더 경로
    private bool createDatabase = true;                                 // 데이터베이스 생성 여부

    [MenuItem("Tools/JSON to Scriptable Objects")]
    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects");
    }

    void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. 파일 선택 버튼
        if (GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }

        // 2. 현재 선택된 경로 표시 및 설정값들
        EditorGUILayout.LabelField("Selected JSON File:", jsonFilePath);
        EditorGUILayout.Space();

        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Database Asset", createDatabase);
        EditorGUILayout.Space();

        // 3. 변환 실행 버튼
        if (GUILayout.Button("Convert to Scriptable Objects"))
        {
            // 실행 전 경로 유효성 검사 (오류 방지 핵심)
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a JSON file first!", "OK");
                return;
            }

            if (!File.Exists(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Selected file does not exist!", "OK");
                return;
            }

            ConvertJsonToScriptableObjects();
        }
    }

    private void ConvertJsonToScriptableObjects()
    {
        // 출력 폴더 생성
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }

        try
        {
            // JSON 파일 읽기
            string jsonText = File.ReadAllText(jsonFilePath);

            // JSON 파싱
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            if (itemDataList == null || itemDataList.Count == 0)
            {
                Debug.LogWarning("JSON data is empty or invalid.");
                return;
            }

            List<ItemSO> createdItems = new List<ItemSO>();

            foreach (ItemData itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                // 데이터 복사
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                // 열거형 변환
                if (System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"아이템 {itemData.itemName}의 유효하지 않은 타입 : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                // 아이콘 로드 (Assets/Resources/ 경로 기준)
                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    // 확장자 중복 방지를 위해 경로 확인 필요 (.png가 이미 포함되어 있는지 등)
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon == null)
                    {
                        Debug.LogWarning($"아이템 {itemData.nameEng}의 아이콘을 찾을 수 없습니다: {itemData.iconPath}");
                    }
                }

                // 에셋 파일 생성 경로 설정
                string assetName = $"Item_{itemData.id.ToString("D4")}_{itemData.nameEng}";
                string assetPath = $"{outputFolder}/{assetName}.asset";

                // 파일 저장
                AssetDatabase.CreateAsset(itemSO, assetPath);

                // 내부 에셋 이름 지정 (문법 수정됨)
                itemSO.name = assetName;
                createdItems.Add(itemSO);

                EditorUtility.SetDirty(itemSO);
            }

            // 데이터베이스 생성 로직
            if (createDatabase && createdItems.Count > 0)
            {
                ItemDataBaseSO dataBase = ScriptableObject.CreateInstance<ItemDataBaseSO>();
                dataBase.items = createdItems;

                AssetDatabase.CreateAsset(dataBase, $"{outputFolder}/ItemDatabase.asset");
                EditorUtility.SetDirty(dataBase);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Successfully created {createdItems.Count} assets!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert JSON: {e.Message}", "OK");
            Debug.LogError($"JSON 변환 중 오류 발생: {e}");
        }
    }
}
#endif