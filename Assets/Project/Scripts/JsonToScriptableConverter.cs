#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography.X509Certificates;

public enum ConversionType
{
    Items,
    Dialogs
}

[Serializable]
public class DialogRowData
{
    public int? id;  //int?는 null 허용 정수형으로, JSON에서 id가 누락될 수 있음을 나타냅니다.
    public string characterName;
    public string text;
    public int? nextId;
    public string protraitPath;
    public string choice1Text;
    public int? choice1NextId;
    public string choiceText;
    public int? choiceNextId;
}

[CreateAssetMenu(fileName = "DialogDataBase", menuName = "Scriptable Objects/Dialog DataBase")]
public class DialogDataBaseSO : ScriptableObject
{
    public List<DialogSO> dialogs = new List<DialogSO>();
}

public class JsonToScriptableConverter : EditorWindow
{
    // 변수 선언 (중복 선언 제거 및 이름 통일)
    private string jsonFilePath = "";                                   // JSON 파일 경로
    private string outputFolder = "Assets/ScriptableObjects/Items";     // 출력 폴더 경로
    private bool createDatabase = true;                                 // 데이터베이스 생성 여부
    private ConversionType conversionType = ConversionType.Items;

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

        //변환 타입 선택
        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type: ", conversionType);

        //타입에 따라 기본 출력 폴더 설정
        if (conversionType == ConversionType.Items && outputFolder == "Assets/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects/Items";
        }
        else if (conversionType == ConversionType.Dialogs && outputFolder == "Assets/ScriptableObjects/Items")
        {
            outputFolder = "Assets/ScriptableObjects/Dialogs";
        }

        // 3. 변환 실행 버튼
        if (GUILayout.Button("Convert to Scriptable Objects"))
        {
            // 실행 전 경로 유효성 검사 (오류 방지 핵심)
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a JSON file first!", "OK");
                return;
            }

            switch (conversionType)
            {
                case ConversionType.Items:
                    ConvertJsonToItemScriptableObjects();
                    break;
                case ConversionType.Dialogs:
                    ConvertDialogJsonToScriptableObjects();
                    break;
            }
        }
    }

    private void ConvertJsonToItemScriptableObjects()
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


    //대화 제이슨을 스크립터블 오브젝트로 변환
    private void ConvertDialogJsonToScriptableObjects()
    {

        //폴더생성
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON 파일 읽기
        string JsonText = File.ReadAllText(jsonFilePath);

        try
        {
            //JSON 파싱
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(JsonText);

            //대화 데이터 재구성
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();  
            List<DialogSO> createdDialogs = new List<DialogSO>();

            //1단계 : 대화 항목 생성
            foreach (var rowData in rowDataList)
            {

                if (!rowData.id.HasValue)
                {
                    continue; // id가 없는 행은 대화로 처리하지 않음
                }

                //id 있는 행을 대화로 처리
                DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();
                //데이터 복사
                dialogSO.id = rowData.id.Value;
                dialogSO.characterName = rowData.characterName;
                dialogSO.text = rowData.text;
                dialogSO.nextId = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                dialogSO.portraitPath = rowData.protraitPath;
                dialogSO.choices = new List<DialogChoiceSO>();
                //초상화 로드 (경로가 있을 경우)
                if (!string.IsNullOrEmpty(rowData.protraitPath))
                {
                    dialogSO.portrait = Resources.Load<Sprite>(rowData.protraitPath);

                    if (dialogSO.portrait == null)
                    {
                        Debug.LogWarning($"대화 {rowData.id}의 초상화를 찾을 수 없습니다.");
                    }
                }
                dialogMap[dialogSO.id] = dialogSO;
                createdDialogs.Add(dialogSO);
            }
            // 2단계 : 선택지 항목 처리 및 연결
            foreach (var rowData in rowDataList)
            {
                // id가 없고 choiceText가 있는 행은 선택지로 처리
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextId.HasValue)
                {
                    // 이전 행의 ID를 부모 ID로 사용 (연속되는 선택지의 경우)
                    int parentId = -1;

                    // 이 선택지 바로 위에 있는 대화 (id가 있는 항목)를 찾음
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }

                    // 부모 ID를 찾지 못했거나 부모 ID가 -1인 경우 (첫 번째 항목)
                    if (parentId == -1)
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText}의 부모 대화를 찾을 수 없습니다.");
                    }

                    if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextId = rowData.choiceNextId.Value;

                        // 선택지 에셋 저장
                        string choiceAssetPath = $"{outputFolder}/Choice_{parentId}_{parentDialog.choices.Count + 1}.asset";
                        AssetDatabase.CreateAsset(choiceSO, choiceAssetPath); 
                        EditorUtility.SetDirty(choiceSO);

                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText}를 연결할 대화 (ID : {parentId})를 찾을 수 없습니다.");
                    }
                }
            }
            // 3단계 대화 스크립터블 오브젝트 저장
            foreach (var dialog in createdDialogs)
            {
                string assetPath = $"{outputFolder}/Dialog_{dialog.id}.asset";
                AssetDatabase.CreateAsset(dialog, assetPath);
               
                //에셋 이름 저장
                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";
                EditorUtility.SetDirty(dialog);

            }

            //데이터 베이스 생성
            if (createDatabase && createdDialogs.Count > 0)
            {
                DialogDataBaseSO dataBase = ScriptableObject.CreateInstance<DialogDataBaseSO>();
                dataBase.dialogs = createdDialogs;
                AssetDatabase.CreateAsset(dataBase, $"{outputFolder}/DialogDatabase.asset");
                EditorUtility.SetDirty(dataBase);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Created {createdDialogs.Count} Dialog Scriptable Objects!", "OK");
        }
        catch(System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert JSON: {e.Message}", "OK");
            Debug.LogError($"JSON 변환 중 오류 발생: {e}");
        }
    }
}
#endif