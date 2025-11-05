using System.Collections.Generic;
using UnityEngine;
using TMPro; 


[RequireComponent(typeof(TextMeshProUGUI))] 
public class LoadingText : MonoBehaviour
{
    [Header("랜덤 문구 목록")]
    [Tooltip("여기에 표시할 팁이나 문구들을 여러 개 입력하세요.")]
    [TextArea(3, 5)]
    [SerializeField] private List<string> loadingTips = new List<string>();

    private TextMeshProUGUI _loadingTextComponent;

    private void Awake()
    {
        // 이 오브젝트에 붙어있는 TextMeshProUGUI 컴포넌트를 가져옵니다.
        _loadingTextComponent = GetComponent<TextMeshProUGUI>();
    }

    // 이 오브젝트가 활성화될 때 (즉, 로딩창이 켜질 때) 호출됩니다.
    private void OnEnable()
    {
        SetRandomText();
    }

    // 텍스트 컴포넌트의 내용을 목록에서 랜덤하게 하나 뽑아서 변경합니다.
    private void SetRandomText()
    {
        if (_loadingTextComponent == null)
        {
            Debug.LogError("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // 목록이 비어있거나 설정되지 않았는지 확인
        if (loadingTips == null || loadingTips.Count == 0)
        {
            Debug.LogWarning("랜덤 로딩 팁 목록이 비어있습니다. 기본 텍스트를 사용합니다.");
            return;
        }

        // 0부터 (목록 개수 - 1) 사이의 랜덤한 인덱스(순번)를 뽑습니다.
        int randomIndex = Random.Range(0, loadingTips.Count);

        // 텍스트 컴포넌트의 내용을 뽑힌 문구로 변경합니다.
        _loadingTextComponent.text = loadingTips[randomIndex];
    }
}