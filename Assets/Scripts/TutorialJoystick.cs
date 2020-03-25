using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityStandardAssets.CrossPlatformInput
{
    public class TutorialJoystick : MonoBehaviour
    {
        public enum AxisOption
        {
            // Options for which axes to use
            Both, // Use both
            OnlyHorizontal, // Only horizontal
            OnlyVertical // Only vertical
        }

        public int MovementRange = 100;
        public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
        public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
        public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

        Vector3 m_StartPos;
        bool m_UseX; // Toggle for using the x axis
        bool m_UseY; // Toggle for using the Y axis
        CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
        CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

        void OnEnable()
        {
            CreateVirtualAxes();
        }

        void Start()
        {
            //m_StartPos = transform.position;
        }

        void UpdateVirtualAxes(Vector3 value)
        {
            var delta = m_StartPos - value;
            delta.y = -delta.y;
            delta /= MovementRange;
            if (Mathf.Abs(delta.x) < 0.4f) delta.x /= 2f;
            //update virtual axis
            m_HorizontalVirtualAxis.Update(-delta.x);
        }

        public void SetAxisLeft()
        {
            m_HorizontalVirtualAxis.Update(-1);
        }

        public void SetAxisRight()
        {
            m_HorizontalVirtualAxis.Update(1);
        }

        public void SetAxisCenter()
        {
            m_HorizontalVirtualAxis.Update(0);
        }

        void CreateVirtualAxes()
        {
            m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis, true);
        }


        //public void OnDrag(PointerEventData data)
        //{
        //    Vector3 newPos = Vector3.zero;

        //    if (m_UseX)
        //    {
        //        int delta = (int)(data.position.x - m_StartPos.x);
        //        delta = Mathf.Clamp(delta, -MovementRange, MovementRange);
        //        newPos.x = delta;
        //    }

        //    if (m_UseY)
        //    {
        //        int delta = (int)(data.position.y - m_StartPos.y);
        //        delta = Mathf.Clamp(delta, -MovementRange, MovementRange);
        //        newPos.y = delta;
        //    }
        //    transform.position = new Vector3(m_StartPos.x + newPos.x, m_StartPos.y + newPos.y, m_StartPos.z + newPos.z);
        //    UpdateVirtualAxes(transform.position);
        //}


        //public void OnPointerUp(PointerEventData data)
        //{
        //    transform.position = m_StartPos;
        //    UpdateVirtualAxes(m_StartPos);
        //}


        //public void OnPointerDown(PointerEventData data) { }

        void OnDisable()
        {
            m_HorizontalVirtualAxis.Remove();
        }
    }
}