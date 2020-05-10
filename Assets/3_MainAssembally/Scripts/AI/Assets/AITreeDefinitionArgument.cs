using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class AITreeDefinitionArgument
{
    [SerializeField]
    private EnumWrapper typeWrapper = null;
    public Enum type
    {
        get => typeWrapper?.value;
        set => typeWrapper = new EnumWrapper(value);
    }
    public string value = "";

    public AITreeDefinitionArgument()
    {

    }

    public AITreeDefinitionArgument(Enum type, string value)
    {
        this.type = type;
        this.value = value;
    }
}