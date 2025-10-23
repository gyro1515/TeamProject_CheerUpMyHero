using UnityEngine;
using UnityEditor; // 에디터 스크립트를 위해 필수!

public class ObjectArranger
{
    // 원하는 간격을 여기에서 수정하세요.
    private static readonly Vector3 g_spacing = new Vector3(5.0f, 0, 0); // X축으로 3씩 간격

    // "Tools/Arrange Selected Objects" 라는 메뉴 아이템을 생성합니다.
    [MenuItem("Tools/Arrange Selected Objects")]
    private static void ArrangeObjects()
    {
        // 현재 에디터에서 선택된 모든 오브젝트의 Transform을 가져옵니다.
        Transform[] selectedTransforms = Selection.transforms;

        if (selectedTransforms.Length <= 1)
        {
            Debug.LogWarning("오브젝트를 2개 이상 선택해야 합니다.");
            return;
        }

        // 정렬 기준이 될 첫 번째 오브젝트의 위치
        Vector3 startPosition = selectedTransforms[0].position;

        // "Undo" (Ctrl+Z) 기능을 위해 변경 사항을 기록합니다.
        // 이렇게 하면 배치 후 되돌리기가 가능합니다.
        Undo.RecordObjects(selectedTransforms, "Arrange Selected Objects");

        // 모든 선택된 오브젝트를 순회하며 위치를 재설정합니다.
        for (int i = 0; i < selectedTransforms.Length; i++)
        {
            // i=0 일 때는 (0,0,0) * 0 이므로 startPosition
            // i=1 일 때는 startPosition + (간격 * 1)
            // i=2 일 때는 startPosition + (간격 * 2)
            selectedTransforms[i].position = startPosition + (g_spacing * i);
        }
    }
}