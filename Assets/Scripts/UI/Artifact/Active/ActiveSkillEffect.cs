using UnityEngine;

/// 모든 액티브 스킬 효과의 기반이 되는 추상 클래스
public abstract class ActiveSkillEffect
{
	 protected ActiveArtifactLevelData levelData; // 레벨 데이터 참조 

	/// 이 스킬의 실제 효과를 실행합니다.
	public abstract void Execute(ActiveArtifactLevelData levelData);
}