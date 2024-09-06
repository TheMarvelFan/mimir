using System.Numerics;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

public class WorldBossState : IBencodable
{
    public int Id { get; init; }
    public int Level { get; init; }
    public BigInteger CurrentHp { get; init; }
    public long StartedBlockIndex { get; init; }
    public long EndedBlockIndex { get; init; }

    public IValue Bencoded =>
        List
            .Empty.Add(Id.Serialize())
            .Add(Level.Serialize())
            .Add(CurrentHp.Serialize())
            .Add(StartedBlockIndex.Serialize())
            .Add(EndedBlockIndex.Serialize());

    public WorldBossState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        Id = l[0].ToInteger();
        Level = l[1].ToInteger();
        CurrentHp = l[2].ToBigInteger();
        StartedBlockIndex = l[3].ToLong();
        EndedBlockIndex = l[4].ToLong();
    }
}
