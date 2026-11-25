namespace NoitaStyleSpells
{
    /// <summary>
    /// 约定法杖修饰器对运行时弹体配置的修改入口
    /// </summary>
    public interface IProjectileModifier
    {
        /// <summary>
        /// 修改本次生成使用的运行时弹体配置
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        void Modify(ProjectileRuntimeConfig config, CastContext context);
    }
}
