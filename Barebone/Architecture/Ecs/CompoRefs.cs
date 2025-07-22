namespace Barebone.Architecture.Ecs;

/// <summary>
/// Includes ref fields to 2 components.
/// </summary>
public ref struct CompoRefs<TC1, TC2>(ref TC1 compo1, ref TC2 compo2)
{
    public readonly ref TC1 Compo1 = ref compo1;
    public readonly ref TC2 Compo2 = ref compo2;
}