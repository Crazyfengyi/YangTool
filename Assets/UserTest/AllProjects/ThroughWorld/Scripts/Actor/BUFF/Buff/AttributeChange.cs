/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-27 
*/
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 属性更改
/// </summary>
public class AttributeChange : BuffBase
{
    //角色属性更改列表
    private List<BuffAttributeChangeInfo> buffAttributeChangeInfo;

    public AttributeChange(GameActor _creator, GameActor _target, int configId) : base(_creator, _target, configId)
    {
    }
    public override void Init(BuffConfig buffConfig)
    {
        base.Init(buffConfig);
        buffAttributeChangeInfo = buffConfig.buffAttributeChangeInfo;
    }
    protected override void OnAwake()
    {
    }
    protected override void OnStart()
    {
        TakeEffect();
    }
    protected override void OnRefresh()
    {
    }
    protected override void OnOverlay()
    {
    }
    protected override void OnReplace()
    {
    }
    protected override void OnRemove()
    {
        LoseEffect();
    }
    protected override void OnDestroy()
    {
    }
    /// <summary>
    /// 生效
    /// </summary>
    public override void TakeEffect()
    {
        if (target is RoleBase role)
        {
            GameUIManager.Instance.AddScoreShow(role.transform.position, $"{skillDescribe}", Color.green);
            for (int i = 0; i < buffAttributeChangeInfo.Count; i++)
            {
                BuffAttributeChangeInfo item = buffAttributeChangeInfo[i];
                role.RoleAttributeChange(item.roleAttribute, item.attributeValueType, item.value);
            }
        }
    }
    /// <summary>
    /// 失效
    /// </summary>
    public override void LoseEffect()
    {
        if (target is RoleBase role)
        {
            //失效时移除的BUFF
            //for (int i = 0; i < buffAttributeChangeInfo.Count; i++)
            //{
            //    BuffAttributeChangeInfo item = buffAttributeChangeInfo[i];
            //    role.RoleAttributeChange(item.roleAttribute, item.attributeValueType, -item.value);
            //}
        }
    }
}