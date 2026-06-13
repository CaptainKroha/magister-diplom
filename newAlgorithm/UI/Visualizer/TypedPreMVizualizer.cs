using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using magisterDiplom.Fabric;
using magisterDiplom.HierarchicalGameModel;
using newAlgorithm;

namespace magisterDiplom.UI.Visualizer
{
    internal class TypedPreMVizualizer : ExcelVisualizer
    {
        public TypedPreMVizualizer(Config config) : base(config)
        {
        }

        public void Initialize(TypedPreMConfiguration typedPreMConfig)
        {
            excelApplication = new Excel.Application
            {
                Visible = true
            };

            Workbook mainWorkbook = excelApplication.Workbooks.Add(Type.Missing);
            while (mainWorkbook.Worksheets.Count < 3)
                mainWorkbook.Worksheets.Add();

            VisualizeConfig((Excel.Worksheet)excelApplication.Worksheets.get_Item(1), typedPreMConfig, 1, 1);

            excelSheet = (Excel.Worksheet)excelApplication.Worksheets.get_Item(2);
            excelSheet.Activate();

            excelSheet.Name = "Результаты";

            excelSheet.Cells[displayRowNumber, displayColumnNumber + 0] = "Номер состава";
            excelSheet.Cells[displayRowNumber, displayColumnNumber + 1] = "Критерий f1";
            excelSheet.Cells[displayRowNumber, displayColumnNumber + 2] = "Критерий f2";

            excelSheet.Rows.AutoFit();
            excelSheet.Columns.AutoFit();

            metaDataSheet = (Excel.Worksheet)excelApplication.Worksheets.get_Item(3);

            metaDataSheet.Name = "Промежуточные данные";
        }

        private void VisualizeConfig(Worksheet worksheet, TypedPreMConfiguration typedPreMConfig, int row = 1, int col = 1)
        {
            Range r;

            base.VisualizeConfig(worksheet, typedPreMConfig, row, col);

            int lastUsedRow = worksheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell).Row;
            row = lastUsedRow + 2;

            {
                worksheet.Cells[row, col] = "Кол-во типов ПТО:";
                worksheet.Cells[row, col].Font.Bold = true;
                worksheet.Cells[row, col + 1] = $"{typedPreMConfig.PreMaintenceTypesCount}";
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + 1]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;
                row += 2;
            }

            {
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + 1]];
                r.Merge(true);
                r.Columns.AutoFit();
                worksheet.Cells[row, col] = "Затраты на бездействие";
                worksheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                worksheet.Cells[row, col].Font.Bold = true;
                worksheet.Cells[row + 1, col + 1] = "Затраты";
                for (int device = 0; device < typedPreMConfig.InactionCosts.Count; device++)
                {
                    worksheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    worksheet.Cells[row + device + 2, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    worksheet.Cells[row + device + 2, col + 1] = $"{typedPreMConfig.InactionCosts[device]}";
                }
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row + typedPreMConfig.InactionCosts.Count + 1, col + 1]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;
                row += typedPreMConfig.InactionCosts.Count + 3;
            }

            {
                int matrixRows = typedPreMConfig.PreMaintenanceCosts.GetLength(0);
                int matrixCols = typedPreMConfig.PreMaintenanceCosts.GetLength(1);

                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + matrixCols]];
                r.Merge(true);
                r.Columns.AutoFit();
                worksheet.Cells[row, col] = "Затраты на ПТО";
                worksheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                worksheet.Cells[row, col].Font.Bold = true;

                for (int preMType = 0; preMType < matrixCols; preMType++)
                    worksheet.Cells[row + 1, col + preMType + 1] = $"Тип ПТО {preMType + 1}";

                for (int device = 0; device < matrixRows; device++)
                {
                    worksheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    worksheet.Cells[row + device + 2, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    for (int preMType = 0; preMType < matrixCols; preMType++)
                        worksheet.Cells[row + device + 2, col + preMType + 1] = $"{typedPreMConfig.PreMaintenanceCosts[device, preMType]}";
                }

                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row + matrixRows + 1, col + matrixCols]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;
                row += matrixRows + 3;
            }

            {
                int matrixRows = typedPreMConfig.PreMaintenanceDurations.GetLength(0);
                int matrixCols = typedPreMConfig.PreMaintenanceDurations.GetLength(1);

                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + matrixCols]];
                r.Merge(true);
                r.Columns.AutoFit();
                worksheet.Cells[row, col] = "Длительность ПТО (по типам)";
                worksheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                worksheet.Cells[row, col].Font.Bold = true;

                for (int preMType = 0; preMType < matrixCols; preMType++)
                    worksheet.Cells[row + 1, col + preMType + 1] = $"Тип ПТО {preMType + 1}";

                for (int device = 0; device < matrixRows; device++)
                {
                    worksheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    worksheet.Cells[row + device + 2, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    for (int preMType = 0; preMType < matrixCols; preMType++)
                        worksheet.Cells[row + device + 2, col + preMType + 1] = $"{typedPreMConfig.PreMaintenanceDurations[device, preMType]}";
                }

                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row + matrixRows + 1, col + matrixCols]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;
                row += matrixRows + 3;
            }

            worksheet.Name = "Начальные параметры";
        }

        public void VisualizeTypedPreMResult(
            int compositionNumber,
            TypedPreMShedule.TypedPreMaintenceSecondLevelOutput secondLevelOutput,
            SecondLevel secondLevel,
            List<List<int>> matrixA,
            TypedPreMConfiguration typedPreMConfig,
            ref int helpRowNumber,
            bool isSuccessfully = true
        ) {
            if (isSuccessfully)
            {
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 0] = $"{compositionNumber}";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 1] = $"{secondLevel.Makespan}";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 2] = $"{secondLevelOutput.F2_Criteria}";

                VisualizeTypedPreMData(compositionNumber, matrixA, secondLevelOutput, secondLevel, typedPreMConfig, ref helpRowNumber);
            }
            else if (Form1.showND)
            {
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 0] = $"{compositionNumber}";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 1] = "#N/A";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 2] = "#N/A";

                VisualizeTypedPreMData(compositionNumber, matrixA, secondLevelOutput, secondLevel, typedPreMConfig, ref helpRowNumber, false);
            }
        }

        private void VisualizeTypedPreMData(
            int compositionNumber,
            List<List<int>> matrixA,
            TypedPreMShedule.TypedPreMaintenceSecondLevelOutput secondLevelOutput,
            SecondLevel secondLevel,
            TypedPreMConfiguration typedPreMConfig,
            ref int row,
            bool isSuccessfully = true
        ) {

            int col = 2;

            int startRow = row;

            int nextRow = row;

            int maxJobCount = 0;

            int maxBatchCount = 0;

            int batchSize = 0;

            Excel.Range r;

            List<List<int>> matrixY = null;

            for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
            {
                maxBatchCount = Math.Max(maxBatchCount, matrixA[dataType].Count);
                batchSize += matrixA[dataType].Count;
                for (int batchIndex = 0; batchIndex < matrixA[dataType].Count; batchIndex++)
                    maxJobCount = Math.Max(maxJobCount, matrixA[dataType][batchIndex]);
            }

            VisualizeUpperLevel(matrixA, ref row);

            nextRow = Math.Max(nextRow, row);

            col += maxBatchCount + 2;

            row = startRow;

            if (isSuccessfully) {

                matrixY = BuildTypedYMatrix(secondLevelOutput.Yl_Matrixes);

                {
                    List<List<int>> matrixP = secondLevelOutput.P_Matrix;

                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + batchSize]];
                    r.Merge(true);
                    metaDataSheet.Cells[row, col] = "Матрица P";
                    metaDataSheet.Cells[row, col].Font.Bold = true;
                    metaDataSheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    r.Columns.AutoFit();

                    for (int dataType = 0; dataType < matrixP.Count; dataType++)
                        metaDataSheet.Cells[row + dataType + 2, col] = $"Тип {dataType + 1}";
                    for (int batchIndex = 0; batchIndex < matrixP[0].Count; batchIndex++)
                        metaDataSheet.Cells[row + 1, col + batchIndex + 1] = $"ПЗ {batchIndex + 1}";
                    for (int dataType = 0; dataType < matrixP.Count; dataType++)
                        for (int batchIndex = 0; batchIndex < matrixP[0].Count; batchIndex++)
                            metaDataSheet.Cells[row + dataType + 2, col + batchIndex + 1] = $"{matrixP[dataType][batchIndex]}";

                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + config.dataTypesCount + 1, col + matrixP[0].Count]];
                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                    r.Borders.Weight = XlBorderWeight.xlThin;
                    nextRow = Math.Max(nextRow, row + matrixP.Count + 1);
                    row += matrixP.Count + 2;
                }

                {
                    List<List<int>> matrixR = secondLevelOutput.R_Matrix;

                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + batchSize]];
                    r.Merge(true);
                    metaDataSheet.Cells[row, col] = "Матрица R";
                    metaDataSheet.Cells[row, col].Font.Bold = true;
                    metaDataSheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    r.Columns.AutoFit();

                    for (int dataType = 0; dataType < matrixR.Count; dataType++)
                        metaDataSheet.Cells[row + dataType + 2, col] = $"Тип {dataType + 1}";
                    for (int batchIndex = 0; batchIndex < matrixR[0].Count; batchIndex++)
                        metaDataSheet.Cells[row + 1, col + batchIndex + 1] = $"ПЗ {batchIndex + 1}";
                    for (int dataType = 0; dataType < matrixR.Count; dataType++)
                        for (int batchIndex = 0; batchIndex < matrixR[0].Count; batchIndex++)
                            metaDataSheet.Cells[row + dataType + 2, col + batchIndex + 1] = $"{matrixR[dataType][batchIndex]}";

                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + config.dataTypesCount + 1, col + matrixR[0].Count]];
                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                    r.Borders.Weight = XlBorderWeight.xlThin;
                    nextRow = Math.Max(nextRow, row + matrixR.Count + 1);
                    row += matrixR.Count + 2;
                }

                {
                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + batchSize]];
                    if ((bool)r.MergeCells)
                    {
                        r.UnMerge();
                    }
                    r.Merge(true);
                    metaDataSheet.Cells[row, col] = "Матрица Y (тип ПТО)";
                    metaDataSheet.Cells[row, col].Font.Bold = true;
                    metaDataSheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    r.Columns.AutoFit();

                    for (int device = 0; device < matrixY.Count; device++)
                        metaDataSheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    for (int batchIndex = 0; batchIndex < matrixY[0].Count; batchIndex++)
                        metaDataSheet.Cells[row + 1, col + batchIndex + 1] = $"ПЗ {batchIndex + 1}";
                    for (int device = 0; device < matrixY.Count; device++)
                        for (int batchIndex = 0; batchIndex < matrixY[0].Count; batchIndex++)
                            metaDataSheet.Cells[row + device + 2, col + batchIndex + 1] = $"{matrixY[device][batchIndex]}";

                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + matrixY.Count + 1, col + matrixY[0].Count]];
                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                    r.Borders.Weight = XlBorderWeight.xlThin;
                    nextRow = Math.Max(nextRow, row + matrixY.Count + 1);
                    row += matrixY.Count + 2;
                }

                {
                    List<List<int>> matrixTPM = secondLevelOutput.Tpm_Matrix;

                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + batchSize]];
                    r.Merge(true);
                    metaDataSheet.Cells[row, col] = "Матрица T^pm";
                    metaDataSheet.Cells[row, col].Font.Bold = true;
                    metaDataSheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    r.Columns.AutoFit();

                    for (int device = 0; device < matrixTPM.Count; device++)
                        metaDataSheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    for (int batchIndex = 0; batchIndex < matrixTPM[0].Count; batchIndex++)
                        metaDataSheet.Cells[row + 1, col + batchIndex + 1] = $"ПЗ {batchIndex + 1}";
                    for (int device = 0; device < matrixTPM.Count; device++)
                        for (int batchIndex = 0; batchIndex < matrixTPM[0].Count; batchIndex++)
                            metaDataSheet.Cells[row + device + 2, col + batchIndex + 1] = $"{matrixTPM[device][batchIndex]}";

                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + matrixTPM.Count + 1, col + matrixTPM[0].Count]];
                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                    r.Borders.Weight = XlBorderWeight.xlThin;
                    nextRow = Math.Max(nextRow, row + matrixTPM.Count + 1);
                    col += matrixTPM[0].Count + 2;
                    row = startRow;
                }

                {
                    Dictionary<int, List<List<int>>> startProcessing = secondLevelOutput.StartProcessing;

                    for (int device = 0; device < config.deviceCount; device++)
                    {
                        r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + maxJobCount]];
                        r.Merge(true);
                        r.Columns.AutoFit();
                        metaDataSheet.Cells[row, col] = $"Матрица T^0{device + 1}";
                        metaDataSheet.Cells[row, col].Font.Bold = true;
                        metaDataSheet.Cells[row, col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                        for (int batchIndex = 0; batchIndex < startProcessing[device].Count(); batchIndex++)
                            metaDataSheet.Cells[row + batchIndex + 2, col] = $"ПЗ {batchIndex + 1}";
                        metaDataSheet.Cells[row + 1, col] = "Задание:";
                        for (int job = 0; job < maxJobCount; job++)
                            metaDataSheet.Cells[row + 1, col + job + 1] = job + 1;
                        for (int batchIndex = 0; batchIndex < startProcessing[device].Count(); batchIndex++)
                            for (int job = 0; job < startProcessing[device][batchIndex].Count; job++)
                                metaDataSheet.Cells[row + batchIndex + 2, col + job + 1] = startProcessing[device][batchIndex][job];

                        r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + startProcessing[device].Count() + 1, col + maxJobCount]];
                        r.Borders.LineStyle = XlLineStyle.xlContinuous;
                        r.Borders.Weight = XlBorderWeight.xlThin;
                        row += startProcessing[device].Count() + 2;
                    }

                    nextRow = Math.Max(nextRow, row + 1);

                    if (Form1.gantaOn)
                    {
                        int maxPreMDuration = typedPreMConfig.PreMaintenanceDurations.Cast<int>().Max();

                        row = startRow;
                        col = col + maxJobCount + 2;

                        for (int device = 0; device < config.deviceCount; device++)
                            metaDataSheet.Cells[row + device + 1, col] = $"Прибор {device + 1}";

                        for (int time = 0; time <= secondLevel.Makespan + maxPreMDuration; time++)
                        {
                            metaDataSheet.Cells[row, col + time + 1] = time;
                            metaDataSheet.Cells[row, col + time + 1].Columns.AutoFit();
                        }

                        for (int device = 0; device < config.deviceCount; device++)
                        {

                            for (int batchIndex = 0; batchIndex < startProcessing[device].Count; batchIndex++)
                            {

                                int batchType = secondLevelOutput.BatchType(batchIndex);

                                for (int job = 0; job < startProcessing[device][batchIndex].Count; job++)
                                {
                                    metaDataSheet.Cells[row + device + 1, col + startProcessing[device][batchIndex][job] + 1] = $"Тип {batchType + 1}";

                                    r = metaDataSheet.Range[
                                        metaDataSheet.Cells[row + device + 1, col + startProcessing[device][batchIndex][job] + 1],
                                        metaDataSheet.Cells[row + device + 1, col + startProcessing[device][batchIndex][job] + config.proccessingTime[device][batchType]]
                                    ];
                                    r.Merge(true);
                                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                                    r.Borders.Weight = XlBorderWeight.xlThin;
                                    r.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Lime);
                                }

                                int preMType = matrixY[device][batchIndex];
                                if (preMType != 0)
                                {
                                    int duration = typedPreMConfig.PreMaintenanceDurations[device, preMType - 1];

                                    int preMStartCol = col + startProcessing[device][batchIndex].Last() + config.proccessingTime[device][batchType] + 1;

                                    metaDataSheet.Cells[row + device + 1, preMStartCol] = $"ПТО{preMType}";
                                    r = metaDataSheet.Range[
                                        metaDataSheet.Cells[row + device + 1, preMStartCol],
                                        metaDataSheet.Cells[row + device + 1, preMStartCol + duration - 1]
                                    ];
                                    r.Merge(true);
                                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                                    r.Borders.Weight = XlBorderWeight.xlThin;
                                    r.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Yellow);
                                }
                            }
                        }

                        r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + config.deviceCount, col + secondLevel.Makespan + maxPreMDuration + 1]];
                        r.Borders.LineStyle = XlLineStyle.xlContinuous;
                        r.Borders.Weight = XlBorderWeight.xlThin;
                    }
                }
            }

            metaDataSheet.Cells[startRow, 1] = compositionNumber;

            r = metaDataSheet.Range[
                metaDataSheet.Cells[startRow, 1],
                metaDataSheet.Cells[nextRow, 1]
            ];

            r.Merge(Type.Missing);

            r.Columns.AutoFit();

            r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            r.VerticalAlignment = XlVAlign.xlVAlignCenter;

            r.Borders.LineStyle = XlLineStyle.xlContinuous;
            r.Borders.Weight = XlBorderWeight.xlThin;

            row = nextRow + 2;
        }

        private static List<List<int>> BuildTypedYMatrix(List<List<List<int>>> ylMatrixes)
        {
            var result = new List<List<int>>(ylMatrixes.Count);

            for (int device = 0; device < ylMatrixes.Count; device++)
            {
                List<List<int>> deviceMatrix = ylMatrixes[device];

                int batchCount = deviceMatrix.Count > 0 ? deviceMatrix[0].Count : 0;

                var row = new List<int>(batchCount);
                for (int batch = 0; batch < batchCount; batch++)
                {
                    int preMType = 0;
                    for (int type = 0; type < deviceMatrix.Count; type++)
                    {
                        if (deviceMatrix[type][batch] == 1)
                        {
                            preMType = type + 1;
                            break;
                        }
                    }
                    row.Add(preMType);
                }
                result.Add(row);
            }

            return result;
        }
    }
}
