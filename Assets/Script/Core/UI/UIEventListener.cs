using UnityEngine;
using UnityEngine.EventSystems;
public class UIEventListener : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public delegate void VoidDelegate(GameObject go);
    public VoidDelegate OnClick;
    public VoidDelegate OnDown;
    public VoidDelegate OnEnter;
    public VoidDelegate OnExit;
    public VoidDelegate OnUp;
    public VoidDelegate OnUpdateSelect;

    public object parameter;

    private float dragThreshold = 10f;

    static public UIEventListener Get(GameObject go)
    {
        UIEventListener listener = go.GetComponent<UIEventListener>();
        if (listener == null) listener = go.AddComponent<UIEventListener>();
        return listener;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClick != null)
        {
            //这个事件滑动也会触发，没找到什么好办法，只能自己做一层了
            float dragDistance = Vector2.Distance(eventData.pressPosition, eventData.position);
            if (dragDistance > dragThreshold)
            {
                return;
            }
            OnClick(eventData.pointerEnter);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (OnDown != null) OnDown(gameObject);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (OnEnter != null) OnEnter(gameObject);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (OnExit != null) OnExit(gameObject);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (OnUp != null) OnUp(gameObject);
    }
    public void OnUpdateSelected(BaseEventData eventData)
    {
        if (OnUpdateSelect != null) OnUpdateSelect(gameObject);
    }
}
