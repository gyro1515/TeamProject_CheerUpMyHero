using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
    [SerializeField] private GachaSequenceController sequenceController;

    public void OnEnvelopeAnimationFinished()
    {
        if (sequenceController != null)
        {
            sequenceController.OnEnvelopeAnimationFinished();
        }
        else
        {
            Debug.LogError("AnimationEventForwarder에 GachaSequenceController가 연결되지 않았습니다!");
        }
    }
}