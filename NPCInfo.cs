using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlugin;

internal class NPCInfo
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsTownNPC { get; set; }
}
