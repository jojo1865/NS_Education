using System;

namespace NS_Education.Tools.ExcelBuild
{
    [Flags]
    public enum BorderDirection
    {
        Top = 0x01,
        Bottom = 0x02,
        Left = 0x04,
        Right = 0x08
    }
}