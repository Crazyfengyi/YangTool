namespace NoitaStyleSpells
{
    public interface IProjectileModifier
    {
        void Modify(ProjectileRuntimeConfig config, CastContext context);
    }
}
