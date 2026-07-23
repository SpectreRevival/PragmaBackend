using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class OverrideInput : IEquatable<OverrideInput>, IInterchangeable<OverrideInput, Packets.OverrideInput>
{
    [SetsRequiredMembers]
    public OverrideInput(bool isAxis, int indexMap, string inputName, double axisScale, string keyName, bool shift, bool ctrl, bool alt, bool cmd)
    {
        IsAxis = isAxis;
        IndexMap = indexMap;
        InputName = inputName ?? throw new ArgumentNullException(nameof(inputName));
        AxisScale = axisScale;
        KeyName = keyName ?? throw new ArgumentNullException(nameof(keyName));
        Shift = shift;
        Ctrl = ctrl;
        Alt = alt;
        Cmd = cmd;
    }

    [PgName("is_axis")]
    public required bool IsAxis { get; set; }
    [PgName("index_map")]
    public required int IndexMap { get; set; }
    [PgName("input_name")]
    public required string InputName { get; set; }
    [PgName("axis_scale")]
    public required double AxisScale { get; set; }
    [PgName("key_name")]
    public required string KeyName { get; set; }
    [PgName("shift")]
    public required bool Shift { get; set; }
    [PgName("ctrl")]
    public required bool Ctrl { get; set; }
    [PgName("alt")]
    public required bool Alt { get; set; }
    [PgName("cmd")]
    public required bool Cmd { get; set; }

    public static OverrideInput FromPacket(Packets.OverrideInput inst)
    {
        return new OverrideInput(inst.BIsAxis, inst.IndexMap, inst.InputName, inst.AxisScale, inst.KeyName, inst.BShift, inst.BCtrl, inst.BAlt, inst.BCmd);
    }

    public virtual bool Equals(OverrideInput? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (IsAxis == other.IsAxis
            && IndexMap == other.IndexMap
            && InputName == other.InputName
            && AxisScale == other.AxisScale
            && KeyName == other.KeyName
            && Shift == other.Shift
            && Ctrl == other.Ctrl
            && Alt == other.Alt
            && Cmd == other.Cmd));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(IsAxis);
        hash.Add(IndexMap);
        hash.Add(InputName);
        hash.Add(AxisScale);
        hash.Add(KeyName);
        hash.Add(Shift);
        hash.Add(Ctrl);
        hash.Add(Alt);
        hash.Add(Cmd);
        return hash.ToHashCode();
    }

    public Packets.OverrideInput ToPacket()
    {
        return new Packets.OverrideInput()
        {
            BIsAxis = IsAxis,
            IndexMap = IndexMap,
            InputName = InputName,
            AxisScale = AxisScale,
            KeyName = KeyName,
            BShift = Shift,
            BCtrl = Ctrl,
            BAlt = Alt,
            BCmd = Cmd
        };
    }
}
