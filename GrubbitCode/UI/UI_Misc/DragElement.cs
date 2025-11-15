using UnityEngine;
using UnityEngine.EventSystems;

namespace Grubbit
{
    public class DragElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool constrainToScreen;
        public Transform objectToDrag;
        protected Vector2 dragOffset;
        protected RectTransform rect;
        protected Canvas canvas;

        protected virtual void Awake()
        {
            if (!objectToDrag)
            {
                objectToDrag = transform;
            }
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            var objectToDragPos = objectToDrag.position;
            dragOffset = new Vector2(objectToDragPos.x - eventData.position.x, objectToDragPos.y - eventData.position.y);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            objectToDrag.position = eventData.position + dragOffset;

            if (constrainToScreen)
            {
                if (!rect.IsFullyVisibleFrom(canvas))
                {
                    var rectX = rect.GetWidth() / 2;
                    var rectY = rect.GetHeight() / 2;
                    var canvasPixelRect = canvas.pixelRect;
                    var objectToDragPos = objectToDrag.position;

                    var restrictedPos = new Vector2
                    {
                        x = Mathf.Clamp(objectToDragPos.x, canvasPixelRect.x + rectX, canvasPixelRect.width - rectX),
                        y = Mathf.Clamp(objectToDragPos.y, canvasPixelRect.y + rectY, canvasPixelRect.height - rectY)
                    };

                    objectToDragPos = restrictedPos;
                    objectToDrag.position = objectToDragPos;
                }
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            // Nothing here yet
        }
    }
}