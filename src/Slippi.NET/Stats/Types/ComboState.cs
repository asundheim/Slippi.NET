using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slippi.NET.Stats.Types;
public record class ComboState
{
    public ComboType? Combo { get; set; }
    public MoveLandedType? Move { get; set; }
    public required int ResetCounter { get; set; }
    public int? LastHitAnimation { get; set; }
    public string? Event { get; set; }
}
