using UnityEngine;
using System.Collections;
using System;

public enum ControlMethod
{
    Touchscreen, Keyboard, Accelerometer
}

[System.Obsolete]
public class Controller
{
    private ControlMethod _controlMethod;

    public Controller()
    {
        _controlMethod = ControlMethod.Keyboard; //default

        if (_controlMethod == ControlMethod.Touchscreen)
        {
            ConfigureTouchInput();
        }
    }

    private void ConfigureTouchInput()
    {
        //todo
    }

    public bool GetButtonDown(string buttonName)
    {
        bool isPressed = false;

        switch (_controlMethod)
        {
            case ControlMethod.Touchscreen:
                //todo
                break;
            case ControlMethod.Keyboard:
                isPressed = Input.GetButtonDown(buttonName);
                break;
            case ControlMethod.Accelerometer:
                //todo
                break;
            default:
                break;
        }

        return isPressed;
    }

    public bool GetButtonUp(string buttonName)
    {
        bool isPressed = false;

        switch (_controlMethod)
        {
            case ControlMethod.Touchscreen:
                //todo
                break;
            case ControlMethod.Keyboard:
                isPressed = Input.GetButtonUp(buttonName);
                break;
            case ControlMethod.Accelerometer:
                //todo
                break;
            default:
                break;
        }

        return isPressed;
    }

    public bool GetButton(string buttonName)
    {
        bool isPressed = false;

        switch (_controlMethod)
        {
            case ControlMethod.Touchscreen:
                //todo
                break;
            case ControlMethod.Keyboard:
                isPressed = Input.GetButton(buttonName);
                break;
            case ControlMethod.Accelerometer:
                //todo
                break;
            default:
                break;
        }

        return isPressed;
    }

    public float GetHorizontalDirection()
    {
        float directionValue = 0;

        switch (_controlMethod)
        {
            case ControlMethod.Touchscreen:
                break;
            case ControlMethod.Keyboard:
                directionValue = Input.GetAxis("Horizontal");
                break;
            case ControlMethod.Accelerometer:
                break;
            default:
                break;
        }

        return directionValue;
    }
}
