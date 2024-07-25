using Bencodex.Types;

namespace Lib9c.Models.Item;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Aura"/>
/// </summary>
public record Aura : Equipment
{
    public Aura(IValue bencoded) : base(bencoded)
    {
    }
}
