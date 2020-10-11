using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class InputManager
{
    public static int GetInputTouchCount()
    {
        return Input.touchCount;
    }

    public static bool GetMouseButtonUp()
    {
        //抬起
        if (PlatformManager.IsEditor())
        {
            return Input.GetMouseButtonUp(0);
        }
        else
        {
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
        }
    }

    public static bool GetMouseButtonDown()
    {
        //按下
        if (PlatformManager.IsEditor())
        {
            return Input.GetMouseButtonDown(0);
        }
        else
        {
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        }
    }

    public static bool GetMouseButtonTouch()
    {
        //拖拽
        if (PlatformManager.IsEditor())
        {
            return Input.GetMouseButton(0);
        }
        else
        {
            return Input.touchCount > 0;
        }
    }

    public static bool GetMouseButton()
    {
        //拖拽
        if (PlatformManager.IsEditor())
        {
            return Input.GetMouseButton(0);
        }
        else
        {
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved;
        }
    }

    public static Vector2 GetMousePosition()
    {
        //当前鼠标坐标
        if (PlatformManager.IsEditor())
        {
            return Input.mousePosition;
        }
        else
        {
            if (Input.touchCount > 0)
                return Input.GetTouch(0).position;
        }
        return Vector2.zero;
    }
}

