﻿using UnityEngine;

namespace CMF
{
    //This camera input class is an example of how to get input from a connected mouse using Unity's default input system;
    //It also includes an optional mouse sensitivity setting;
    //这个摄像头输入类是一个如何使用Unity的默认输入系统从连接的鼠标获得输入的例子;
    //它还包括一个可选的鼠标灵敏度设置;
    public class CameraMouseInput : CameraInput
    {
        //Mouse input axes;
        public string mouseHorizontalAxis = "Mouse X";
        public string mouseVerticalAxis = "Mouse Y";

        //Invert input options;
        public bool invertHorizontalInput = false;
        public bool invertVerticalInput = false;

        //Use this value to fine-tune mouse movement;
        //All mouse input will be multiplied by this value;
        public float mouseInputMultiplier = 0.01f;

        public override float GetHorizontalCameraInput()
        {
            //Get raw mouse input;
            float _input = Input.GetAxisRaw(mouseHorizontalAxis);

            //Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
            //由于原始的鼠标输入已经是基于时间的，我们需要在将输入传递给摄像机控制器之前更正这一点;
            if (Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                _input /= Time.deltaTime;
                _input *= Time.timeScale;
            }
            else
                _input = 0f;

            //Apply mouse sensitivity;
            _input *= mouseInputMultiplier;

            //Invert input;
            if (invertHorizontalInput)
                _input *= -1f;

            return _input;
        }

        public override float GetVerticalCameraInput()
        {
            //Get raw mouse input;
            float _input = -Input.GetAxisRaw(mouseVerticalAxis);

            //Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
            if (Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                _input /= Time.deltaTime;
                _input *= Time.timeScale;
            }
            else
                _input = 0f;

            //Apply mouse sensitivity;
            _input *= mouseInputMultiplier;

            //Invert input;
            if (invertVerticalInput)
                _input *= -1f;

            return _input;
        }
    }
}
