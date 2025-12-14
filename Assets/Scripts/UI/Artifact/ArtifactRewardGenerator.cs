using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArtifactRewardGenerator
{
    // 유물 데이터 들고 있는 역할
    private readonly ArtifactSO _artifactSO;

    // 스테이지별 보상 유물 등급 확률 딕셔너리
    private readonly Dictionary<int, float[]> _chapterProbabilities;

    // 최대 메인 스테이지 인덱스
    private const int _maxChapterIndex = 4;

    // 최대 시도 횟수 배수 -> 무한루프 방지용 -> 사실 없어도 무방함
    private const int _maxAttemptsMultiplier = 100;

    public ArtifactRewardGenerator()    // 생성자 함수
    {
        _artifactSO = Resources.Load<ArtifactSO>("DB/ArtifactSO");

        _chapterProbabilities = new Dictionary<int, float[]>
        {
            { 0, new float[] { 89.0f, 9.5f, 1.5f, 0f, 0f } },     
            { 1, new float[] { 69.5f, 26.0f, 3.5f, 1.0f, 0f } },  
            { 2, new float[] { 49.5f, 41.5f, 7.0f, 1.5f, 0.5f } },
            { 3, new float[] { 34.5f, 50.0f, 9.0f, 5.0f, 1.5f } },
            { 4, new float[] { 25.0f, 52.5f, 12.5f, 7.5f, 2.5f } }
        };
    }

    #region 랜덤 유물 생성
    // 랜덤 패시브 유물 생성 메서드
    public List<PassiveArtifactData> GetRandomPassiveArtifacts(int count, int chapter)
    {
        if (_artifactSO == null || _artifactSO.passiveArtifacts.Count == 0)
        {
            Debug.Log("패시브 유물 데이터 0개임. 데이터 임포트 문제 있어요.");
            return new List<PassiveArtifactData>();
        }

        int adjustedChapter = Mathf.Min(chapter, _maxChapterIndex);
        float[] probabilities = _chapterProbabilities[adjustedChapter];

        List<PassiveArtifactData> source = new List<PassiveArtifactData>(_artifactSO.passiveArtifacts);
        HashSet<PassiveArtifactData> selectedArtifacts = new HashSet<PassiveArtifactData>();

        int attempts = 0;
        int maxAttempts = count * _maxAttemptsMultiplier;

        while (selectedArtifacts.Count < count && attempts < maxAttempts)
        {
            attempts++;

            PassiveArtifactGrade selectedGrade = DetermineGradeByProbability(probabilities);

            List<PassiveArtifactData> artifactsOfGrade = source.Where(a => a.grade == selectedGrade).ToList();

            if (artifactsOfGrade.Count == 0)
            {
                Debug.LogWarning($"챕터 {adjustedChapter}에서 {selectedGrade} 등급 유물 없음.");
                continue;
            }

            PassiveArtifactData selectedArtifact = artifactsOfGrade[Random.Range(0, artifactsOfGrade.Count)];
            selectedArtifacts.Add(selectedArtifact);
        }

        if (attempts > maxAttempts)
        {
            Debug.LogError($"유물 선택 최대 시도 횟수 초과. 요청: {count}개, 선택: {selectedArtifacts.Count}개");
        }

        return selectedArtifacts.ToList();
    }

    // 랜덤 액티브 유물 생성 메서드
    public List<ActiveArtifactData> GetRandomActiveArtifacts(int count)
    {
        if (_artifactSO == null || _artifactSO.activeArtifacts == null)
        {
            Debug.LogError("액티브 유물 데이터 0개임. 데이터 임포트 문제 있어요.");
            return new List<ActiveArtifactData>();
        }

        List<ActiveArtifactData> source = new List<ActiveArtifactData>(_artifactSO.activeArtifacts);
        List<ActiveArtifactData> result = new List<ActiveArtifactData>();
        HashSet<int> usedIdx = new HashSet<int>();

        int actualCount = Mathf.Min(count, source.Count);

        while (result.Count < actualCount)
        {
            int randomIdx = Random.Range(0, source.Count);

            if (usedIdx.Contains(randomIdx)) continue;

            usedIdx.Add(randomIdx);
            result.Add(source[randomIdx]);
        }

        return result;
    }
    #endregion

    private PassiveArtifactGrade DetermineGradeByProbability(float[] probabilities)
    {
        float randomValue = Random.Range(0f, 100f);
        float cumulativeProbability = 0f;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];

            if (randomValue < cumulativeProbability)
            {
                return (PassiveArtifactGrade)i;
            }
        }

        return PassiveArtifactGrade.Common;
    }
}
