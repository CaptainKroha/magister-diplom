using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using magisterDiplom.Model;
using magisterDiplom.Model.Configuration; // Для TypedPreMConfiguration
using magisterDiplom.Fabric;              // Для TypedPreMShedule
using magisterDiplom.HierarchicalGameModel; // Для SecondLevel
using newAlgorithm; // Для Form1

namespace magisterDiplom.UI.Visualizer
{
    /// <summary>
    /// Класс для визуализации результатов расчетов с типизированным ПТО в формате Excel.
    /// Наследуется от базового ExcelVisualizer для переиспользования общей логики
    /// (визуализация верхнего уровня, матриц P/R/T^pm/T^0l, диаграммы Ганта).
    ///
    /// Отличие от простого ПТО:
    /// - ПТО имеет тип (1..PreMaintenceTypesCount). Вместо одной матрицы Y (прибор × ПЗ, 0/1)
    ///   во втором уровне формируется набор матриц <see cref="TypedPreMShedule.TypedPreMaintenceSecondLevelOutput.Yl_Matrixes"/>
    ///   (прибор × тип ПТО × ПЗ, 0/1). Здесь они сворачиваются в матрицу прибор × ПЗ,
    ///   где значение ячейки — номер типа ПТО (или 0, если ПТО нет).
    /// - Длительность ПТО берётся из <see cref="TypedPreMConfiguration.PreMaintenanceDurations"/>[прибор, типПТО],
    ///   а не из вектора preMaintenanceTimes[прибор].
    /// </summary>
    internal class TypedPreMVizualizer : ExcelVisualizer
    {
        /// <summary>
        /// Конструктор класса TypedPreMVizualizer.
        /// </summary>
        /// <param name="config">Структура конфигурации, содержащая информацию о конвейерной системе.</param>
        public TypedPreMVizualizer(Config config) : base(config)
        {
        }

        /// <summary>
        /// Инициализация Excel приложения и создание вкладок для вывода данных с учетом TypedPreMConfiguration.
        /// </summary>
        /// <param name="typedPreMConfig">Параметры, характеризующие типизированное ПТО.</param>
        public void Initialize(TypedPreMConfiguration typedPreMConfig)
        {
            // Инициализируем объект для работы с Excel
            excelApplication = new Excel.Application
            {
                Visible = true
            };

            // Создаём вкладку
            Workbook mainWorkbook = excelApplication.Workbooks.Add(Type.Missing);
            while (mainWorkbook.Worksheets.Count < 3)
                mainWorkbook.Worksheets.Add();

            // Получаем вкладку с параметрами и визуализируем конфигурацию типизированного ПТО
            VisualizeConfig((Excel.Worksheet)excelApplication.Worksheets.get_Item(1), typedPreMConfig, 1, 1);

            // Получаем вкладки и переключаемся на неё
            excelSheet = (Excel.Worksheet)excelApplication.Worksheets.get_Item(2);
            excelSheet.Activate();

            // Устанавливаем имя вкладки
            excelSheet.Name = "Результаты";

            excelSheet.Cells[displayRowNumber, displayColumnNumber + 0] = "Номер состава";
            excelSheet.Cells[displayRowNumber, displayColumnNumber + 1] = "Критерий f1";
            excelSheet.Cells[displayRowNumber, displayColumnNumber + 2] = "Критерий f2";

            excelSheet.Rows.AutoFit();
            excelSheet.Columns.AutoFit();

            // Получаем вкладку с параметрами
            metaDataSheet = (Excel.Worksheet)excelApplication.Worksheets.get_Item(3);

            // Устанавливаем имя вкладки
            metaDataSheet.Name = "Промежуточные данные";
        }

        /// <summary>
        /// Функция отображает информацию о параметрах характеризующих систему и задания в Excel форме
        /// для типизированного ПТО. Сначала выводятся общие параметры (через базовый метод),
        /// затем дополнительные параметры типизированного ПТО.
        /// </summary>
        /// <param name="worksheet">Закладка отображения</param>
        /// <param name="typedPreMConfig">Параметры, характеризующие типизированное ПТО</param>
        /// <param name="row">Номер строки начала отрисовки</param>
        /// <param name="col">Номер столбца начала отрисовки</param>
        private void VisualizeConfig(Worksheet worksheet, TypedPreMConfiguration typedPreMConfig, int row = 1, int col = 1)
        {
            Range r;

            // Вызываем базовую визуализацию для общих параметров (типы данных, приборы, времена
            // выполнения, переналадки, надёжность, интенсивности отказов/восстановления и т.д.).
            // TypedPreMConfiguration является PreMConfiguration, поэтому передаём напрямую.
            base.VisualizeConfig(worksheet, typedPreMConfig, row, col);

            // Базовый метод приватный и не возвращает обновлённые row/col, поэтому продолжаем
            // вывод дополнительных параметров с новой свободной строки.
            int lastUsedRow = worksheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell).Row;
            row = lastUsedRow + 2;

            // Визуализируем PreMaintenceTypesCount
            {
                worksheet.Cells[row, col] = "Кол-во типов ПТО:";
                worksheet.Cells[row, col].Font.Bold = true;
                worksheet.Cells[row, col + 1] = $"{typedPreMConfig.PreMaintenceTypesCount}";
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + 1]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;
                row += 2;
            }

            // Визуализируем InactionCosts (затраты на бездействие по приборам)
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

            // Визуализируем PreMaintenanceCosts (затраты на ПТО: прибор × тип ПТО)
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

            // Визуализируем PreMaintenanceDurations (длительность ПТО: прибор × тип ПТО)
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

            // Переименовываем вкладку (после базового метода, который задаёт своё имя)
            worksheet.Name = "Начальные параметры";
        }

        /// <summary>
        /// Визуализация результатов текущей итерации для типизированного ПТО.
        /// Аналог <see cref="ExcelVisualizer.VisualizeSimplePreMResult"/> для типизированного случая.
        /// </summary>
        /// <param name="compositionNumber">Номер состава пакетов заданий.</param>
        /// <param name="secondLevelOutput">Выходные данные второго уровня (типизированное ПТО).</param>
        /// <param name="secondLevel">Класс для получения данных нижнего уровня (Makespan).</param>
        /// <param name="matrixA">Матрица составов пакетов заданий.</param>
        /// <param name="typedPreMConfig">Параметры, характеризующие типизированное ПТО.</param>
        /// <param name="helpRowNumber">Вспомогательный номер строки для позиционирования данных.</param>
        /// <param name="isSuccessfully">Флаг успешности построения расписания.</param>
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

                // Визуализируем промежуточные данные
                VisualizeTypedPreMData(compositionNumber, matrixA, secondLevelOutput, secondLevel, typedPreMConfig, ref helpRowNumber);
            }
            else if (Form1.showND)
            {
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 0] = $"{compositionNumber}";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 1] = "#N/A";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 2] = "#N/A";

                // Визуализируем промежуточные данные (неудачное расписание)
                VisualizeTypedPreMData(compositionNumber, matrixA, secondLevelOutput, secondLevel, typedPreMConfig, ref helpRowNumber, false);
            }
        }

        /// <summary>
        /// Детальная визуализация всех матриц нижнего уровня и диаграммы Ганта для типизированного ПТО.
        /// Аналог <see cref="ExcelVisualizer.VisualizeSimplePreMData"/> для типизированного случая.
        /// </summary>
        /// <param name="compositionNumber">Номер состава пакетов заданий.</param>
        /// <param name="matrixA">Матрица составов пакетов заданий.</param>
        /// <param name="secondLevelOutput">Выходные данные второго уровня (типизированное ПТО).</param>
        /// <param name="secondLevel">Класс для получения данных нижнего уровня (Makespan).</param>
        /// <param name="typedPreMConfig">Параметры, характеризующие типизированное ПТО.</param>
        /// <param name="row">Номер строки начала отрисовки.</param>
        /// <param name="isSuccessfully">Флаг успешности построения расписания.</param>
        private void VisualizeTypedPreMData(
            int compositionNumber,
            List<List<int>> matrixA,
            TypedPreMShedule.TypedPreMaintenceSecondLevelOutput secondLevelOutput,
            SecondLevel secondLevel,
            TypedPreMConfiguration typedPreMConfig,
            ref int row,
            bool isSuccessfully = true
        ) {

            // Устанавливаем номер колонки
            int col = 2;

            // Объявим и инициализируем начальную строку
            int startRow = row;

            // Объявим и инициализируем конечную строку
            int nextRow = row;

            // Объявляем максимальное количество заданий
            int maxJobCount = 0;

            // Объявляем максимальное количество пакетов заданий
            int maxBatchCount = 0;

            // Объявляем общее количество пакетов заданий
            int batchSize = 0;

            // Объявляем диапазон
            Excel.Range r;

            // Компактная матрица Y (прибор × ПЗ) с номерами типов ПТО. Формируется только
            // при успешном построении расписания (см. блок ниже).
            List<List<int>> matrixY = null;

            // Вычисляем максимальное количество заданий, пакетов и общий размер пакетов
            for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
            {
                maxBatchCount = Math.Max(maxBatchCount, matrixA[dataType].Count);
                batchSize += matrixA[dataType].Count;
                for (int batchIndex = 0; batchIndex < matrixA[dataType].Count; batchIndex++)
                    maxJobCount = Math.Max(maxJobCount, matrixA[dataType][batchIndex]);
            }

            // Визуализируем верхний уровень (матрицы A и M)
            VisualizeUpperLevel(matrixA, ref row);

            // Вычисляем номер конечной строки
            nextRow = Math.Max(nextRow, row);

            // Устанавливаем номер колонки со смещением
            col += maxBatchCount + 2;

            // Устанавливаем начальный номер строки
            row = startRow;

            // Если расписание было построено успешно
            if (isSuccessfully) {

                // Сворачиваем набор типизированных матриц Y (прибор × тип ПТО × ПЗ) в компактную
                // матрицу (прибор × ПЗ), где значение — номер типа ПТО после ПЗ (или 0, если ПТО нет).
                matrixY = BuildTypedYMatrix(secondLevelOutput.Yl_Matrixes);

                // Отображаем матрицу P
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

                // Отображаем матрицу R
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

                // Отображаем матрицу Y (тип ПТО после каждого ПЗ на каждом приборе)
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

                // Отображаем матрицу T^pm (моменты времени окончания ПТО)
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

                // Отображаем матрицы T^0l (моменты начала времени выполнения заданий по приборам)
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

                    // Если установлен флаг отрисовки диаграммы Ганта
                    if (Form1.gantaOn)
                    {
                        // Максимальная длительность ПТО среди всех приборов и типов ПТО — нужна
                        // для определения ширины диаграммы Ганта.
                        int maxPreMDuration = typedPreMConfig.PreMaintenanceDurations.Cast<int>().Max();

                        row = startRow;
                        col = col + maxJobCount + 2;

                        // Для каждого прибора отображаем его имя
                        for (int device = 0; device < config.deviceCount; device++)
                            metaDataSheet.Cells[row + device + 1, col] = $"Прибор {device + 1}";

                        // Для каждого момента времени отображаем шкалу
                        for (int time = 0; time <= secondLevel.Makespan + maxPreMDuration; time++)
                        {
                            metaDataSheet.Cells[row, col + time + 1] = time;
                            metaDataSheet.Cells[row, col + time + 1].Columns.AutoFit();
                        }

                        // Для каждого прибора
                        for (int device = 0; device < config.deviceCount; device++)
                        {

                            // Для каждого пакета заданий
                            for (int batchIndex = 0; batchIndex < startProcessing[device].Count; batchIndex++)
                            {

                                int batchType = secondLevelOutput.BatchType(batchIndex);

                                // Для каждого задания пакета отображаем блок выполнения
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

                                // Если после данного пакета на приборе есть ПТО — отображаем его блок.
                                // В типизированном случае значение matrixY содержит номер типа ПТО (1..n),
                                // длительность зависит от прибора и типа ПТО.
                                int preMType = matrixY[device][batchIndex];
                                if (preMType != 0)
                                {
                                    // Длительность ПТО для прибора device и типа ПТО (preMType - 1)
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

                        // Устанавливаем границы диаграммы Ганта
                        r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + config.deviceCount, col + secondLevel.Makespan + maxPreMDuration + 1]];
                        r.Borders.LineStyle = XlLineStyle.xlContinuous;
                        r.Borders.Weight = XlBorderWeight.xlThin;
                    }
                }
            }

            // Устанавливаем номер состава ПЗ
            metaDataSheet.Cells[startRow, 1] = compositionNumber;

            // Получаем диапазон данных высотой в количество занимаемых строк
            r = metaDataSheet.Range[
                metaDataSheet.Cells[startRow, 1],
                metaDataSheet.Cells[nextRow, 1]
            ];

            // Объединяем ячейки
            r.Merge(Type.Missing);

            // Растягиваем ширину диапазона
            r.Columns.AutoFit();

            // Выравниваем текст по центру
            r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            r.VerticalAlignment = XlVAlign.xlVAlignCenter;

            // Устанавливаем границы
            r.Borders.LineStyle = XlLineStyle.xlContinuous;
            r.Borders.Weight = XlBorderWeight.xlThin;

            // Устанавливаем следующую строку
            row = nextRow + 2;
        }

        /// <summary>
        /// Сворачивает набор типизированных матриц Y (прибор × тип ПТО × ПЗ, значения 0/1)
        /// в компактную матрицу (прибор × ПЗ), где значение ячейки — номер типа ПТО (1..n),
        /// назначенного после данного ПЗ на данном приборе, либо 0, если ПТО не назначено.
        /// </summary>
        /// <param name="ylMatrixes">Набор матриц Y_l из выходных данных второго уровня.</param>
        /// <returns>Матрица прибор × ПЗ с номерами типов ПТО.</returns>
        private static List<List<int>> BuildTypedYMatrix(List<List<List<int>>> ylMatrixes)
        {
            var result = new List<List<int>>(ylMatrixes.Count);

            for (int device = 0; device < ylMatrixes.Count; device++)
            {
                // Матрица данного прибора: [тип ПТО][ПЗ]
                List<List<int>> deviceMatrix = ylMatrixes[device];

                // Количество ПЗ определяем по первой строке (количеству столбцов)
                int batchCount = deviceMatrix.Count > 0 ? deviceMatrix[0].Count : 0;

                var row = new List<int>(batchCount);
                for (int batch = 0; batch < batchCount; batch++)
                {
                    // Ищем тип ПТО, назначенный после данного ПЗ (первый со значением 1)
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
