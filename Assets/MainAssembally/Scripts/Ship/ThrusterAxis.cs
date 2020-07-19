using System;

[Flags]
public enum ThrusterAxis : ushort
{
    x       = 0b00001,
    y       = 0b00010,
    z       = 0b00100,
    plus    = 0b01000,
    minus   = 0b10000,
    xPlus   = x | plus,
    xMinus  = x | minus,
    yPlus   = y | plus,
    yMinus  = y | minus,
    zPlus   = z | plus,
    zMinus  = z | minus
}
