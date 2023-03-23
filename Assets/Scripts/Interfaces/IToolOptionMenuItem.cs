using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IToolOptionMenuItem
{
    void Selected();
    void SelectedToggle();
    void Deselected();
    void CheckMaterials(bool bOptionActive);
}
