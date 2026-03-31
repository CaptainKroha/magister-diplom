using newAlgorithm.Model;
using Microsoft.Office.Interop.Excel;
using System;
using System.Reflection;

namespace newAlgorithm.Service
{

    /// <summary>
    /// Класс ответственный за отображения расписания в Excel
    /// </summary>
    public class Visualizer
    {
        Application oXL;
        _Workbook oWB;
        _Worksheet oSheet;

        private int deep;
        private int count;

        public Visualizer(int deep = 0, int count = 0)
        {
            this.deep = deep;
            this.count = count;
            
            // Запуск Excel и получение объекта приложения
            oXL = new Application();
            oXL.Visible = true;
            oWB = oXL.Workbooks.Add(Missing.Value);
            oSheet = (_Worksheet)oWB.Sheets[1];
            InitExcelList(count);
        }


        public void InitExcelList(int count)
        {
            oSheet.Columns.ColumnWidth = 3;
            oSheet.Rows.RowHeight = 15;
            oSheet.Cells.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            oSheet.Cells.VerticalAlignment = XlVAlign.xlVAlignCenter;

            for (int i = 1; i <= count; i++)
            {
                oSheet.Cells[1, i] = i;
                oSheet.Cells[1, i].Interior.ColorIndex = 2 + i;
            }
        }

        public void TestDrawExcel(int _level, int _columnIndex, int _count, int _color)
        {
            try
            {
                ////Add table headers going cell by cell.
                //oSheet.Cells[1, 1] = 1;
                //oSheet.Cells[1, 2] = "Last Name";
                //oSheet.Cells[1, 3] = "Full Name";
                //oSheet.Cells[1, 4] = "Salary";
                //oSheet.Cells[1, 4].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);

                //for (int i =3; i < 57; i++)
                //{
                //    oSheet.Cells[i, 1].Interior.ColorIndex = i;
                //}

                int rowIndex = 3 * _level;
                int lastColumnIndex = 0;
                int columnIndex = _columnIndex + 1;

                oSheet.Cells[rowIndex - 1, columnIndex].HorizontalAlignment = XlHAlign.xlHAlignLeft;
                oSheet.Cells[rowIndex, columnIndex].Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                oSheet.Cells[rowIndex, columnIndex].Borders[XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlMedium;
                oSheet.Cells[rowIndex - 1, columnIndex] = columnIndex - 1;

                for (int i = 1; i <= _count; i++)
                {
                    if (i == _count)
                    {
                        lastColumnIndex = columnIndex;
                    }
                    oSheet.Cells[rowIndex, columnIndex] = 1;
                    oSheet.Cells[rowIndex, columnIndex++].Interior.ColorIndex = 2 + _color;
                }

                //oSheet.Cells[rowIndex + 1, lastColumnIndex] = lastColumnIndex;

            }
            catch (Exception) {}
        }


        public void Visualize(TreeDimMatrix tnMatrix, Matrix timeProcessing, RMatrix rMatrix)
        {
            foreach (TreeDimMatrixNode node in tnMatrix.treeDimMatrix)
            {
                int count = node.time;
                int type = rMatrix[node.fromDataType].dataType;

                int value = timeProcessing[node.device-1, type-1];

                TestDrawExcel(node.device, count, value, type);
            }
        }

        public void Close()
        {
            oXL.Quit();
        }

        /// <summary>
        /// Создает новый лист в Excel окне
        /// </summary>
        public void CreateExcelAppList(int deep, int count)
        {
            this.deep = deep;
            this.count = count;// Запуск Excel и получение объекта приложения// Получение нового листа
            oSheet = (_Worksheet)oWB.Sheets.Add(After: oWB.Sheets[oWB.Sheets.Count]);
            InitExcelList(count);
        }
        
        #region Неиспользуемые функции

        #endregion
    }
}
