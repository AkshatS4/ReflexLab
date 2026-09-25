using UnityEngine;
using UnityEngine.EventSystems;

public class CircleTarget : Game1TargetBase, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ResolveHit();
        }
    }

    protected override void OnHit()
    {
    }

    protected override void OnExpired()
    {
    }

    protected override void OnWrongKey()
    {
    }
}