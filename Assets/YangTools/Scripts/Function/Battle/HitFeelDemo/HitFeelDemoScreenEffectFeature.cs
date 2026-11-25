using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 仅为挂有 HitFeelDemoScreenEffects 的相机执行屏幕空间反馈
/// </summary>
public sealed class HitFeelDemoScreenEffectFeature : ScriptableRendererFeature
{
    // 屏幕反馈使用的着色器
    [SerializeField] private Shader screenEffectShader;

    // 渲染特性创建的屏幕效果材质
    private Material effectMaterial;
    // 注入渲染管线的屏幕效果通道
    private HitFeelDemoScreenEffectPass effectPass;

    /// <summary>
    /// 创建 Demo 屏幕效果材质与渲染 Pass
    /// </summary>
    public override void Create()
    {
        CoreUtils.Destroy(effectMaterial);
        effectMaterial = screenEffectShader != null ? CoreUtils.CreateEngineMaterial(screenEffectShader) : null;
        effectPass = new HitFeelDemoScreenEffectPass();
        effectPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        effectPass.ConfigureInput(ScriptableRenderPassInput.Color);
    }

    /// <summary>
    /// 仅为活动中的 Demo 相机注册屏幕效果 Pass
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (effectMaterial == null || effectPass == null)
        {
            return;
        }

        Camera camera = renderingData.cameraData.camera;
        if (camera == null || !camera.TryGetComponent(out HitFeelDemoScreenEffects effects) || !effects.IsPlaying)
        {
            return;
        }

        effectPass.Setup(effectMaterial, effects);
        renderer.EnqueuePass(effectPass);
    }

    /// <summary>
    /// 释放运行时创建的渲染材质
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(effectMaterial);
        effectMaterial = null;
        effectPass = null;
    }

    /// <summary>
    /// 执行一次 Demo 局部屏幕效果合成
    /// </summary>
    private sealed class HitFeelDemoScreenEffectPass : ScriptableRenderPass
    {
        // 绘制时传入的材质属性块
        private readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
        // 当前绘制使用的材质
        private Material material;
        // 当前相机的屏幕反馈参数来源
        private HitFeelDemoScreenEffects effects;

        /// <summary>
        /// 设置本帧使用的材质与相机效果状态
        /// </summary>
        public void Setup(Material sourceMaterial, HitFeelDemoScreenEffects sourceEffects)
        {
            material = sourceMaterial;
            effects = sourceEffects;
        }

        /// <summary>
        /// 将来源画面经过 Demo 屏幕效果后写回相机颜色缓冲
        /// </summary>
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null || effects == null || !effects.IsPlaying)
            {
                return;
            }

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            TextureHandle source = resourceData.activeColorTexture;
            TextureHandle destination = renderGraph.CreateTexture(source);
            properties.Clear();
            effects.WriteShaderProperties(properties);

            RenderGraphUtils.BlitMaterialParameters parameters = new RenderGraphUtils.BlitMaterialParameters(source, destination, material, 0)
            {
                propertyBlock = properties
            };
            RenderGraphUtils.AddBlitPass(renderGraph, parameters, "HitFeelDemo Screen Effects");
            resourceData.cameraColor = destination;
        }
    }
}
