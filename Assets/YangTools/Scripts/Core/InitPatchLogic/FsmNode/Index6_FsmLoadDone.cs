using System;
using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;

namespace GameMain
{
    public class FsmLoadDone : IStateNode
    {
        public void OnCreate(StateMachine machine)
        {
        }

        public void OnEnter()
        {
            //AudioMgr.Instance.PlayBgm(GameConstant.BGM, 0.2f);
        }
    
        public void OnExit()
        {

        }

        public void OnUpdate()
        {

        }
    }
}