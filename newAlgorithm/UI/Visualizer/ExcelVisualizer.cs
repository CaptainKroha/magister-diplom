using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;

namespace magisterDiplom.UI.Visualizer
{
    internal class ExcelVisualizer : Visualizer
    {

        /// <summary>
        /// Объект для работы с Excel
        /// </summary>
        private Excel.Application excelApplication;

        /// <summary>
        /// Владка для работы с Excel
        /// </summary>
        private Excel.Worksheet excelSheet;

        /// <summary>
        /// Владка для работы с Excel
        /// </summary>
        private Excel.Worksheet metaDataSheet;

        public override void VisualizeConfig()
        {
            throw new NotImplementedException();
        }

    }
}
