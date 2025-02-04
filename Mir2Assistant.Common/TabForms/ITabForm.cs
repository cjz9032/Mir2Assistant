using Mir2Assistant.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.TabForms;

public interface ITabForm
{
    public string Title { get; }
    public MirGameInstanceModel? GameInstance { get; set; }
}
