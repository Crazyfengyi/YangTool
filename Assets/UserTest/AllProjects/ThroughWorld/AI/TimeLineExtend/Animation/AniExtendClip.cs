/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-06-09 
*/
using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using Sirenix.OdinInspector;

namespace TimelineExtend
{
    public class AniExtendClip : AnimationPlayableAsset
    {
        [InfoBox("请只更改最后4个属性和第一个属性,无视其他属性")]
        [LabelText("目标动画")]
        /// <summary>
        /// 目标动画
        /// </summary>
        public AnimationClip defaultAni;
        [LabelText("备用动画名称")]
        /// <summary>
        /// 备用动画
        /// </summary>
        public string reserveAniName;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            RoleBase role = go.GetComponent<RoleBase>();
            if (role)
            {
                bool haveThisAni = false;
                RuntimeAnimatorController runtimeAniController = role.Animator.runtimeAnimatorController;
                AnimationClip[] clips = runtimeAniController.animationClips;

                if (defaultAni != null)
                {
                    for (int i = 0; i < clips.Length; i++)
                    {
                        if (clips[i] == defaultAni)
                        {
                            haveThisAni = true;
                            break;
                        }
                    }
                }

                if (haveThisAni)
                {
                    clip = defaultAni;
                }
                else
                {
                    //尝试找到备用动画
                    for (int i = 0; i < clips.Length; i++)
                    {
                        if (clips[i].name.Equals(reserveAniName))
                        {
                            clip = clips[i];
                            break;
                        }
                    }
                }
            }

            return base.CreatePlayable(graph, go);
        }
    }
}