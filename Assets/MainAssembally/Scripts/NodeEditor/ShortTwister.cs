using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ShortTwister
{
    private const ulong KEY = 2147483647;
    private ulong delta;

    public ShortTwister(ulong delta)
    {
        this.delta = delta;
    }

    public ulong GetNext(ulong number)
    {
        return (KEY + delta) % KEY;
    }
}