using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;

namespace MahjongReader;

// Client::UI::AddonEmj
//   Component::GUI::AtkUnitBase
[Addon("Emj")]
[StructLayout(LayoutKind.Explicit, Size = 0x290)]
public struct AddonEmj {
    [FieldOffset(0x0)] public AtkUnitBase AtkUnitBase;
}
