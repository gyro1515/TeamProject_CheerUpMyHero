using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactUIPresent
{
    private readonly ArtifactManager _model;
    private readonly UIArtifactInventoryPanel _inventoryPanelView;
    private readonly UIArtifactEquipPanel _equipPanelView;
    private readonly UIArtifactStatPanel _statPanelView;

    private void HandleArtifactChanged()
    {

    }

    private void HandleEquipRequest(ArtifactData artifact, int slotIndex)
    {
        _model.EquipArtifact(artifact, slotIndex);
        _inventoryPanelView.gameObject.SetActive(false);
    }

    private void HandleUnEquipRequest(ArtifactData artifact)
    {
        for (int i = 0; _model.EquippedArtifacts.Count > 0; i++)
        {
            if (_model.EquippedArtifacts[i] == artifact)
            {
                _model.UnEquipArtifact(i);
                break;
            }
        }
    }

    private void HandleSortRequest()
    {

    }
}
