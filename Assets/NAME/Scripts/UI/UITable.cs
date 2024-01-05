using UnityEngine;

namespace Unity.NAME.UI
{
    // The component that is used to display the Objectives, the Notification and the game messages like a list
    // When a new one is created, the previous ones move down to make room for the new one

    public class UITable : MonoBehaviour
    {
        [Tooltip("How much space should there be between items?")]
        public float Offset;

        [Tooltip("Add new the new items below existing items.")]
        public bool Down;

        public void UpdateTable(GameObject newItem)
        {
            if (newItem != null)
                newItem.GetComponent<RectTransform>().localScale = Vector3.one;

            float height = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = transform.GetChild(i).GetComponent<RectTransform>();
                Vector2 size = child.sizeDelta;
                height += Down ? -(1 - child.pivot.y) * size.y : (1 - child.pivot.y) * size.y;
                if (i != 0)
                    height += Down ? -Offset : Offset;

                Vector2 newPos = Vector2.zero;

                newPos.y = height;
                newPos.x = 0;//-child.pivot.x * size.x * hi.localScale.x;
                child.anchoredPosition = newPos;
            }
        }
    }
}
