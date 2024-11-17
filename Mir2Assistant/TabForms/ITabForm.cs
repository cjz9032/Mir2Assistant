using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.TabForms
{
    public interface ITabForm
    {
        public string Title { get; }
        public AssiastantForm? AssiastantForm { get; set; }
    }
}
