using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "DialogDatabaseSO", menuName = "Dialog System/DialogDatabaseSO")]
public class DialogDatabaseSO : ScriptableObject
{
    public List<DialogSO> dialogs = new List<DialogSO>();

    private Dictionary<int, DialogSO> dialogsById; // 캐싱을 위한 딕셔너리

    public void Initailize()
    {
        dialogsById = new Dictionary<int, DialogSO>();

        foreach (var dialog in dialogs)
        {
            if (dialog != null)
            {
                // 중복 ID 방지를 위해 확인 후 추가
                if (!dialogsById.ContainsKey(dialog.id))
                {
                    dialogsById[dialog.id] = dialog;
                }
            }
        }
    }

    public DialogSO GetDialogById(int id)
    {
        if (dialogsById == null)
        {
            Initailize();
        }

        if (dialogsById.TryGetValue(id, out DialogSO dialog))
        {
            return dialog;
        }
        return null;
    }

    // --- 에디터 전용 기능 (빌드 시 자동 제외) ---
#if UNITY_EDITOR
    [ContextMenu("Load All Dialogs From Folder")]
    public void LoadAllDialogs()
    {
        dialogs.Clear();

        // 1. 특정 경로(Assets/ScriptableObjects/Dialogs) 지정
        string folderPath = "Assets/ScriptableObjects/Dialogs";

        // 해당 폴더 내에서만 DialogSO 타입의 에셋을 찾음
        string[] guids = AssetDatabase.FindAssets("t:DialogSO", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogSO dialog = AssetDatabase.LoadAssetAtPath<DialogSO>(path);

            if (dialog != null)
            {
                dialogs.Add(dialog);
            }
        }

        // 2. ID 순서대로 정렬
        dialogs.Sort((a, b) => a.id.CompareTo(b.id));

        // 3. 변경 사항 저장
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log($"[DialogDatabase] {folderPath} 경로에서 {dialogs.Count}개의 DialogSO를 로드했습니다.");
    }
#endif
}