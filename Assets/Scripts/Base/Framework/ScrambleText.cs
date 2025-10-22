using System;
using System.Collections;
using System.Collections.Generic; 
using System.Text;
using TMPro; 
using UnityEngine;

public class ScrambleText : MonoBehaviour
{
    [Tooltip("스크램블 효과에 사용할 문자들")]
    [SerializeField] string scrambleCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*<>?";

    private TextMeshProUGUI tmpText;
    private string targetString; // 최종 목표 텍스트
    private Coroutine scrambleCoroutine; // 현재 실행 중인 코루틴


    public void StartScramble(TextMeshProUGUI textMPUGUI, string _targetString, float duration, Action afterScrambleAction = null)
    {
        // 이미 실행 중인 스크램블이 있다면 중지
        if (scrambleCoroutine != null)
        {
            StopCoroutine(scrambleCoroutine);
            // 기존거 다시 세팅
            if (tmpText) tmpText.text = targetString;
        }
        tmpText = textMPUGUI;
        targetString = _targetString;
        scrambleCoroutine = StartCoroutine(ScrambleEffect(duration, afterScrambleAction));
    }

    // 무작위 문자 하나를 반환
    private char GetRandomCharacter()
    {
        int index = UnityEngine.Random.Range(0, scrambleCharacters.Length);
        return scrambleCharacters[index];
    }

    // 스크램블 효과를 처리하는 코루틴
    private IEnumerator ScrambleEffect(float duration, Action afterScrambleAction = null)
    {
        int length = targetString.Length;
        if (length == 0)
        {
            tmpText.text = "";
            yield break; // 처리할 텍스트가 없음
        }

        // StringBuilder를 사용하면 매 프레임 new string()을 생성하는 비용을 줄일 수 있음
        StringBuilder stringBuilder = new StringBuilder(length);

        // 아직 공개되지 않은 문자의 인덱스 리스트
        List<int> indicesToReveal = new List<int>(length);

        // 1. 초기화: StringBuilder는 무작위 문자로 채우고, 인덱스 리스트는 0부터 length-1까지 채우기
        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(GetRandomCharacter());
            indicesToReveal.Add(i);
        }

        // 2. 인덱스 리스트를 무작위로 섞기 (Fisher-Yates Shuffle)
        // 문자가 공개되는 순서에 해당
        for (int i = 0; i < indicesToReveal.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, indicesToReveal.Count);
            int temp = indicesToReveal[i];
            indicesToReveal[i] = indicesToReveal[j];
            indicesToReveal[j] = temp;
        }

        // 3. 코루틴 루프 시작
        float elapsedTime = 0f;
        int charsRevealed = 0; // 현재까지 공개된 문자 수

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            // 4. 시간에 따라 공개되어야 할 총 문자 수 계산
            int targetCharsRevealed = (int)(progress * length);

            int newCharsToReveal = targetCharsRevealed - charsRevealed;

            if (newCharsToReveal > 0)
            {
                // 5. 새로 공개할 문자들을 섞인 인덱스 리스트에서 가져와 고정
                for (int i = 0; i < newCharsToReveal; i++)
                {
                    if (indicesToReveal.Count == 0) break; // 모든 문자가 공개됨

                    int indexToReveal = indicesToReveal[0];
                    indicesToReveal.RemoveAt(0); // 리스트에서 제거

                    // 해당 인덱스의 문자를 실제 텍스트로 고정
                    stringBuilder[indexToReveal] = targetString[indexToReveal];
                }
                charsRevealed = targetCharsRevealed;
            }

            // 6. 아직 공개되지 않은 나머지 문자들은 계속 무작위로 변경
            foreach (int index in indicesToReveal)
            {
                stringBuilder[index] = GetRandomCharacter();
            }

            // 7. TextMeshPro 텍스트 업데이트
            tmpText.text = stringBuilder.ToString();

            yield return null; // 다음 프레임까지 대기
        }

        // 8. 완료: 최종 텍스트로 확실하게 설정
        tmpText.text = targetString;
        scrambleCoroutine = null; // 코루틴 완료
        afterScrambleAction?.Invoke(); // 완료 후 액션 호출
    }
}