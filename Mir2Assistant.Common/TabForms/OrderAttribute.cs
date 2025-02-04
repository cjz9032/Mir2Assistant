using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.TabForms;

[System.AttributeUsage(System.AttributeTargets.Class)]
public class OrderAttribute : System.Attribute
{
    public OrderAttribute(int _order)
    {
        Order = _order;
    }

    public int Order { get; set; }

}
