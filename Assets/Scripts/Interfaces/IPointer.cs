using UnityEngine;
using System.Collections;

public interface IPointer
{
    bool VariablePointerActive { get; set; }
    bool FreezePointerState { get; set; }
    PointerStates CurrentPointerState { get; }
    Vector3 PointerPosition { get; }
    Vector3 PointerStartPosition { get; }
    GameObject PointerEndGameObject { get; }
    Transform PointerLineRendererStartTransform { get; }
    bool ValidTargetPosition { get; }
    bool Active { get; set; }
    bool ToggleDistance { get; set; }
    bool ValidRaycastTarget { get; }
    RaycastHit PointerHit { get; }
    Color ObjectOriginalColour { get; set; }
    bool HighlightingActive { get; set; }

    void SnapToSurface();
    void SnapPointerState(PointerStates newPointerState);
    void LockPointerLength(bool lockStatus);
    void SetLayerMask(int layerMaskIndex);
    void SetPointerColour(Color newColour);
    void SetPointerColourDefault();
    void SetPointerWidth(float startWidth, float endWidth);
    void SetPointerWidthDefault();
    void SetPointerEndSize(float newScale);
    void SetPointerEndSizeDefault();
    void RemoveHighlight();
    void ChangeLayer(int newLayerMaskNumber);
}
