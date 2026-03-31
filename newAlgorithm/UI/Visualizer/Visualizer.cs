using magisterDiplom.Model.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace magisterDiplom.UI.Visualizer
{
    internal abstract class Visualizer
    {

        private Configuration config;
        public abstract void VisualizeConfig();



    }
}
