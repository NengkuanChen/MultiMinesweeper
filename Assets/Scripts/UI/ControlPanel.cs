using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UI
{
    public class ControlPanel : MonoBehaviour, IPointerClickHandler, IScrollHandler, IDragHandler
    {

        
        // public void OnDragBegin()
        // {
        //     cachedMousePosition = Input.mousePosition;
        // }
        //
        
        public void OnDrag()
        {
            // var dragDelta = Input.mousePositionDelta;
            
            // EventUtility.TriggerNow(this, new OnPlayerDraggedEventArgs(dragDelta));
            // cachedMousePosition = Input.mousePosition;
        }

        public void OnClick()
        {
            // EventUtility.TriggerNow(this, new OnPlayerClickedEventArgs());
        }
        
        public void OnScroll()
        {
            // EventUtility.TriggerNow(this, new OnPlayerScrolledEventArgs(Input.mouseScrollDelta.y));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // //check if drag or click
            // if (eventData.dragging)
            // {
            //     return;
            // }
            //
            // if (eventData.button == PointerEventData.InputButton.Left)
            // {
            //     EventUtility.TriggerNow(this, new OnPlayerClickedEventArgs());
            // }
            // else if (eventData.button == PointerEventData.InputButton.Right)
            // {
            //     EventUtility.TriggerNow(this, new OnPlayerRightClickedEventArgs());
            // }
        }

        public void OnScroll(PointerEventData eventData)
        {
            EventUtility.TriggerNow(this, new OnPlayerScrolledEventArgs(eventData.scrollDelta.y));
        }

        public void OnDrag(PointerEventData eventData)
        {
            // EventUtility.TriggerNow(this, new OnPlayerDraggedEventArgs(eventData.delta));
        }
    }
}