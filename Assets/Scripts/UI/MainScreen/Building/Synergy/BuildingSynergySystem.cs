using System.Collections.Generic;

public enum BuildingSynergyType
{
    // 인접 (Adjacency)
    Farm_Barracks,
    Barracks_Mine,
    Barracks_LumberMill,
    Mine_LumberMill,
    Farm_Mine,
    Farm_LumberMill,

    // 라인 (Line)
    Farm_Line,
    LumberMill_Line,
    Mine_Line,
    Barracks_Line,

    // 블록 (Block)
    Specialized_Block, // 전문 기술 단지
    Balanced_Block     // 균형 발전 지구
}

public class DetectedSynergy
{
    public BuildingSynergyType Type { get; }
    public List<(int x, int y)> TilePositions { get; } // 시너지를 구성하는 타일 좌표들

    public DetectedSynergy(BuildingSynergyType type, List<(int x, int y)> positions)
    {
        Type = type;
        TilePositions = positions;
    }
}
