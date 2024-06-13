using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Since the focus ring is not working as expected with the TextFields, we need to create a custom focus ring
    /// </summary>
    public class CustomFocusRing
    {
        private List<VisualElement> m_FocusRing;
        private int m_CurrentFocusIndex;
        private FocusController m_FocusController;
        
        public CustomFocusRing(VisualElement root, VisualElement[] focusableElements)
        {
            m_FocusRing = new List<VisualElement>();
            // Initialize the focusable elements
            m_FocusRing.AddRange(focusableElements);
            // Get the focus controller
            m_FocusController = root.focusController;
            // Set the initial focus
            FocusElement(m_CurrentFocusIndex);
        }
        
        /// <summary>
        /// Try to find the next focusable element 
        /// </summary>
        public void Next()
        {
            int nextElementIndex = -1;
            
            for (int i = 0; i < m_FocusRing.Count; i++)
            {
                m_CurrentFocusIndex = (m_CurrentFocusIndex + 1) % m_FocusRing.Count;
                if (m_FocusRing[m_CurrentFocusIndex].style.display != DisplayStyle.None)
                {
                    nextElementIndex = m_CurrentFocusIndex;
                    break;
                }
            }
        
            if (nextElementIndex != -1)
            {
                FocusElement(nextElementIndex);
            }
        }

        /// <summary>
        /// Try to find the previous focusable element
        /// </summary>
        public void Previous()
        {
            int previousElementIndex = -1;
            
            for (int i = 0; i < m_FocusRing.Count; i++)
            {
                m_CurrentFocusIndex = (m_CurrentFocusIndex - 1 + m_FocusRing.Count) % m_FocusRing.Count;
                if (m_FocusRing[m_CurrentFocusIndex].style.display != DisplayStyle.None)
                {
                    previousElementIndex = m_CurrentFocusIndex;
                    break;
                }
            }
        
            if (previousElementIndex != -1)
            {
                FocusElement(previousElementIndex);
            }
        }
        
        /// <summary>
        /// Focus the element at the given index
        /// </summary>
        private void FocusElement(int index)
        {
            if (m_FocusController.focusedElement != m_FocusRing[index])
            {
                m_FocusRing[index].Focus();
            }
        }
    }
}