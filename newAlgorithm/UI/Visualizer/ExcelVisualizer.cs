using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using magisterDiplom.Model;
using newAlgorithm.Model;
using System.Drawing;
using magisterDiplom.Fabric;
using magisterDiplom.HierarchicalGameModel;
using magisterDiplom.Model.Configuration;
using newAlgorithm;

namespace magisterDiplom.UI.Visualizer
{
    /// <summary>
    /// Класс для визуализации результатов расчетов в формате Excel
    /// </summary>
    internal class ExcelVisualizer
    {
        /// <summary>
        /// Данная структура данных содержит информацию о конфигурации конвейерной системы
        /// </summary>
        private readonly Config config;

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

        /// <summary>
        /// Номер строки отображения в Excel таблице
        /// </summary>
        private const int displayRowNumber = 1;

        /// <summary>
        /// Номер колонки отображения в Excel таблице
        /// </summary>
        private const int displayColumnNumber = 1;

        public ExcelVisualizer(Config config)
        {
            this.config = config;
        }

        /// <summary>
        /// Инициализация Excel приложения и создание вкладок для вывода данных
        /// </summary>
        /// <param name="preMConfig">Параметры характеризующие ПТО</param>
        public void Initialize(PreMConfiguration preMConfig)
        {
            // Инициализируем объект для работы с Excel
            excelApplication = new Excel.Application
            {
                // Устанавливаем флаг отображение 
                Visible = true
            };

            // Создаём вкладку
            Workbook mainWorkbook = excelApplication.Workbooks.Add(Type.Missing);
            mainWorkbook.Worksheets.Add();
            mainWorkbook.Worksheets.Add();

            // Получаем вкладку с параметрами
            VisualizeConfig((Excel.Worksheet)excelApplication.Worksheets.get_Item(1), preMConfig, 1, 1);

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
        /// Визуализация результатов текущей итерации
        /// </summary>
        public void VisualizeSimplePreMResult(int compositionNumber, SimplePreMSchedule.SimplePreMaintenceSecondLevelOutput secondLevelOutput, SecondLevel secondLevel , List<List<int>> matrixA, PreMConfiguration preMConfig, ref int helpRowNumber, bool isSuccessfully = true)
        {
            if (isSuccessfully)
            {
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 0] = $"{compositionNumber}";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 1] = $"{secondLevel.Makespan}";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 2] = $"{secondLevelOutput.F2_Criteria}";

                // Визуализируем промежуточные данные
                VisualizeSimplePreMData(compositionNumber, matrixA, secondLevelOutput, secondLevel, preMConfig, ref helpRowNumber);
            }
            else if (Form1.showND)
            {
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 0] = $"{compositionNumber}";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 1] = "#N/A";
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 2] = "#N/A";

                // Визуализируем промежуточные данные
                VisualizeSimplePreMData(compositionNumber, matrixA, secondLevelOutput, secondLevel, preMConfig, ref helpRowNumber, false);
            }
        }

        /// <summary>
        /// Покраска ячеек оптимального решения в зеленый цвет
        /// </summary>
        public void MarkAsOptimal(int compositionNumber)
        {
            excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 0].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Lime);
            excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Lime);
            excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Lime);
        }

        /// <summary>
        /// Создание финального линейного графика на листе результатов
        /// </summary>
        public void CreateFinalChart(int compositionNumber)
        {
            // Инициализируем линейный график
            Excel.Range r = excelSheet.Range[
                excelSheet.Cells[displayRowNumber, displayColumnNumber + 1],
                excelSheet.Cells[displayRowNumber + compositionNumber, displayColumnNumber + 1]
            ];
            ChartObjects linerChart = (ChartObjects)excelSheet.ChartObjects(Type.Missing);
            ChartObject charts = linerChart.Add(400, 0, 600, 300);
            Chart chart = (Chart)charts.Chart;
            chart.SetSourceData(r);
            chart.ChartType = XlChartType.xlLine;
        }

        /// <summary>
        /// Функция отображает информацию параметрах характеризующих систему и задания в Excel форме
        /// </summary>
        /// <param name="worksheet">Закладка отображения</param>
        /// <param name="preMConfig">Параметры характеризующие ПТО</param>
        /// <param name="row">Номер строки начала отрисовки</param>
        /// <param name="col">Номер столбца начала отрисовки</param>
        private void VisualizeConfig(Worksheet worksheet, PreMConfiguration preMConfig, int row = 0, int col = 0)
        {
            // Изменяем название закладки
            worksheet.Name = "Начальные параметры";

            // Объявляем диапазон
            Range r;

            // Выводим количество типов данных
            {
                worksheet.Cells[row, col] = "Типов данных:";
                worksheet.Cells[row, col].Font.Bold = true;
                worksheet.Cells[row, col + 1] = $"{config.dataTypesCount}";
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + 1]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;
                row += 2;
            }

            // Выводим количество приборов
            {
                worksheet.Cells[row, col] = "Приборов:";
                worksheet.Cells[row, col].Font.Bold = true;
                worksheet.Cells[row, col + 1] = $"{config.deviceCount}";
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + 1]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;
                row += 2;
            }

            // Выводим нижний порог надёжности
            {
                worksheet.Cells[row, col] = "Надёжность:";
                worksheet.Cells[row, col + 1] = preMConfig.beta;
                worksheet.Cells[row, col].Font.Bold = true;
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + 1]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;
                row += 2;
            }

            // Визуализируем матрицу времени выполнения
            {
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + config.dataTypesCount]];
                r.Merge(true);
                r.Columns.AutoFit();
                worksheet.Cells[row, col] = "Времени выполнения";
                worksheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                worksheet.Cells[row, col].Font.Bold = true;
                for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                    worksheet.Cells[row + 1, col + dataType + 1] = $"Тип {dataType + 1}";
                for (int device = 0; device < config.deviceCount; device++) {
                    worksheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    worksheet.Cells[row + device + 2, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                }
                for (int device = 0; device < config.deviceCount; device++)
                    for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                        worksheet.Cells[row + device + 2, col + dataType + 1] = $"{config.proccessingTime[device][dataType]}";

                // Получаем диапазон ячеек и устанавливаем границы
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row + config.deviceCount + 1, col + config.dataTypesCount]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;

                // Изменяем позицию для следующих данных
                col += config.dataTypesCount + 2;
                row = 1;
            }

            // Визуализируем матрицу переналадки
            {
                for (int device = 0; device < config.deviceCount; device++)
                {

                    // Визуализируем матрицу переналадки для прибора device
                    r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + config.dataTypesCount]];
                    r.Merge(true);
                    worksheet.Cells[row, col] = $"Переналадка для прибора {device + 1}";
                    worksheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    worksheet.Cells[row, col].Font.Bold = true;
                    r.Columns.AutoFit();
                    for (int fromDataType = 0; fromDataType < config.dataTypesCount; fromDataType++) { 
                        worksheet.Cells[row + fromDataType + 2, col] = $"Тип {fromDataType + 1}";
                        worksheet.Cells[row + fromDataType + 2, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    }
                    for (int toDataType = 0; toDataType < config.dataTypesCount; toDataType++)
                        worksheet.Cells[row + 1, col + toDataType + 1] = $"Тип {toDataType + 1}";
                    for (int fromDataType = 0; fromDataType < config.dataTypesCount; fromDataType++)
                        for (int toDataType = 0; toDataType < config.dataTypesCount; toDataType++)
                            worksheet.Cells[row + fromDataType + 2, col + toDataType + 1] = $"{config.changeoverTime[device][fromDataType][toDataType]}";

                    // Получаем диапазон ячеек и устанавливаем границы
                    r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row + config.dataTypesCount + 1, col + config.dataTypesCount]];
                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                    r.Borders.Weight = XlBorderWeight.xlThin;

                    // Изменяем позицию для следующих данных
                    row += config.dataTypesCount + 3;
                }

                row = 1;
                col += config.dataTypesCount + 2;
            }

            // Визуализируем вектор длительностей ПТО
            {
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + 1]];
                r.Merge(true);
                r.Columns.AutoFit();
                worksheet.Cells[row, col] = $"Длительность ПТО";
                worksheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                worksheet.Cells[row, col].Font.Bold = true;
                worksheet.Cells[row + 1, col + 1] = "Время";
                for (int device = 0; device < config.deviceCount; device++)
                {
                    worksheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    worksheet.Cells[row + device + 2, col + 1] = $"{preMConfig.preMaintenanceTimes[device]}";
                }
                // Получаем диапазон ячеек и устанавливаем границы
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row + config.deviceCount + 1, col + 1]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;

                // Изменяем позицию для следующих данных
                row += config.deviceCount + 3;
            }

            // Визуализируем вектор интенсивности отказов приборов
            {
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + 1]];
                r.Merge(true);
                worksheet.Cells[row, col] = $"Интенсивность отказов";
                worksheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                worksheet.Cells[row, col].Font.Bold = true;
                r.Columns.AutoFit();
                worksheet.Cells[row + 1, col + 1] = "Вероятность";
                worksheet.Cells[row + 1, col + 1].Columns.AutoFit();
                for (int device = 0; device < config.deviceCount; device++)
                {
                    worksheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    worksheet.Cells[row + device + 2, col + 1] =preMConfig.failureRates[device];
                }
                // Получаем диапазон ячеек и устанавливаем границы
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row + config.deviceCount + 1, col + 1]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;

                // Изменяем позицию для следующих данных
                row += config.deviceCount + 3;
            }

            // Визуализируем вектор интенсивности восстановления приборов
            {
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row, col + 1]];
                r.Merge(true);
                worksheet.Cells[row, col] = $"Интенсивность восстановления";
                worksheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                worksheet.Cells[row, col].Font.Bold = true;
                r.Columns.AutoFit();
                worksheet.Cells[row + 1, col + 1] = "Вероятность";
                worksheet.Cells[row + 1, col + 1].Columns.AutoFit();
                for (int device = 0; device < config.deviceCount; device++)
                {
                    worksheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    worksheet.Cells[row + device + 2, col + 1] = preMConfig.restoringDevice[device];
                }
                // Получаем диапазон ячеек и устанавливаем границы
                r = worksheet.Range[worksheet.Cells[row, col], worksheet.Cells[row + config.deviceCount + 1, col + 1]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;
            }
        }

        /// <summary>
        /// Функция отобразит данные верхнего уровня в таблице
        /// </summary>
        /// <param name="matrixA">Матрица составов пакетов заданий</param>
        /// <param name="row">Номер строки для отображения</param>
        private void VisualizeUpperLevel(List<List<int>> matrixA, ref int row)
        {
            // Объявляем диапазон
            Range r;

            // Объявляем номер колонки
            int col = 2;

            // Объявляем максимальное количество пакетов заданий
            int maxBatchCount = 0;

            // Отображаем вектор А
            {

                // Объединяем несколько ячеек для заголовка
                r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + 1]];
                r.Merge(true);

                // Устанавливаем автовыравнивание
                r.Columns.AutoFit();

                // Выводим заголовки таблицы
                metaDataSheet.Cells[row, col] = "Вектор A";
                metaDataSheet.Cells[row, col].Font.Bold = true;
                metaDataSheet.Cells[row, col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                metaDataSheet.Cells[row + 1, col + 1] = "Количество ПЗ";
                metaDataSheet.Cells[row + 1, col + 1].Columns.AutoFit();

                // Выводим таблицу
                for (int dataType = 0; dataType < config.dataTypesCount; dataType++) {

                    // Выводим даные
                    metaDataSheet.Cells[row + dataType + 2, col] = $"Тип {dataType + 1}";
                    metaDataSheet.Cells[row + dataType + 2, col + 1] = $"{matrixA[dataType].Count}";

                    // Определяем максимальное количество пакетов заданий
                    maxBatchCount = Math.Max(maxBatchCount, matrixA[dataType].Count);
                }

                // Получаем диапазон данных
                r = metaDataSheet.Range[
                    metaDataSheet.Cells[row, col],
                    metaDataSheet.Cells[row + config.dataTypesCount + 1, col + 1]
                ];

                // Обводим границы
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;

                // Устанавливаем следующий стобец
                row += config.dataTypesCount + 2;
            }

            // Отображаем матрицу M
            {
                // Объединяем несколько ячеек для заголовка
                r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + maxBatchCount]];
                r.Merge(true);

                // Устанавливаем автовыравнивание
                r.Columns.AutoFit();

                // Выводим заголовок таблицы
                metaDataSheet.Cells[row, col] = "Матрица M";
                metaDataSheet.Cells[row, col].Font.Bold = true;
                metaDataSheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                for (int batchCount = 0; batchCount < maxBatchCount; batchCount++)
                    metaDataSheet.Cells[row + 1, col + batchCount + 1] = $"ПЗ {batchCount + 1}";
                for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                    metaDataSheet.Cells[row + dataType + 2, col] = $"Тип {dataType + 1}";

                // Выводим данны в таблице
                for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                    for (int batchCount = 0; batchCount < matrixA[dataType].Count; batchCount++)
                        metaDataSheet.Cells[row + dataType + 2, col + batchCount + 1] = $"{matrixA[dataType][batchCount]}";

                // Обводим границы
                r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + config.dataTypesCount + 1, col + maxBatchCount]];
                r.Borders.LineStyle = XlLineStyle.xlContinuous;
                r.Borders.Weight = XlBorderWeight.xlThin;

                // Изменяем значение следующей строки
                row += config.dataTypesCount + 2;
            }
        }

        /// <summary>
        /// Функция отображает информацию о обрабатываемых данных в Excel форме
        /// </summary>
        /// <param name="compositionNumber">Номер состава пакетов</param>
        /// <param name="matrixA">Матрица составов пакетов заданий</param>
        /// <param name="secondLevel">Класс для получения данных из нижнего уровня</param>
        /// <param name="preMConfig">Конфигурационная структура для отображения общих данных</param>
        /// <param name="row">Номер строки начала отрисовки</param>
        /// <param name="isSuccessfully">Флаг построения расписания</param>
        private void VisualizeSimplePreMData(
            int compositionNumber,
            List<List<int>> matrixA,
            SimplePreMSchedule.SimplePreMaintenceSecondLevelOutput secondLevelOutput,
            SecondLevel secondLevel,
            PreMConfiguration preMConfig,
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

            int batchSize = 0;

            // Объявляем диапазон
            Excel.Range r;

            // Получаем матрицу Y
            List<List<int>> matrixY = secondLevelOutput.Y_Matrix;

            // Функция подсчёта максимального количества заданий
            void calcMaxJobCount() {
                for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                    for (int batchIndex = 0; batchIndex < matrixA[dataType].Count; batchIndex++)
                        maxJobCount = Math.Max(maxJobCount, matrixA[dataType][batchIndex]);
            }

            // Функция расчёта максимального количества пакетов заданий
            void calcMaxBatchCount()
            {
                for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                    maxBatchCount = Math.Max(maxBatchCount, matrixA[dataType].Count);
            }

            // Функция расчёта общего количества пакетов заданий
            void calcBatchSize()
            {
                for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                    batchSize += matrixA[dataType].Count;
            }

            // Вычисляем максимальное количество заданий
            calcMaxJobCount();

            // Вычисляем максимальное количество пакетов заданий
            calcMaxBatchCount();

            // Вычисляем общее количество пакетов заданий
            calcBatchSize();

            // Визуализируем верхний уровень
            VisualizeUpperLevel(matrixA, ref row);

            // Вычисляем номер конечной строки
            nextRow = Math.Max(nextRow, row);

            // Устанавливаем номер колонки со смещением
            col += maxBatchCount + 2;

            // Устанавливаем начальный номер строки
            row = startRow;

            // Если расписание было построено успешно
            if (isSuccessfully) {

                // Отображаем матрицу P
                {
                    // Получаем матрицу P
                    List<List<int>> matrixP = secondLevelOutput.P_Matrix;

                    // Объединяем несколько ячеек для заголовка
                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + batchSize]];
                    r.Merge(true);

                    // Выводим заголовок таблицы
                    metaDataSheet.Cells[row, col] = "Матрица P";
                    metaDataSheet.Cells[row, col].Font.Bold = true;
                    metaDataSheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                    // Устанавливаем автовыравнивание
                    r.Columns.AutoFit();

                    for (int dataType = 0; dataType < matrixP.Count; dataType++)
                        metaDataSheet.Cells[row + dataType + 2, col] = $"Тип {dataType + 1}";
                    for (int batchIndex = 0; batchIndex < matrixP[0].Count; batchIndex++)
                        metaDataSheet.Cells[row + 1, col + batchIndex + 1] = $"ПЗ {batchIndex + 1}";
                    for (int dataType = 0; dataType < matrixP.Count; dataType++)
                        for (int batchIndex = 0; batchIndex < matrixP[0].Count; batchIndex++)
                            metaDataSheet.Cells[row + dataType + 2, col + batchIndex + 1] = $"{matrixP[dataType][batchIndex]}";

                    // Обводим границы
                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + config.dataTypesCount + 1, col + matrixP[0].Count]];
                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                    r.Borders.Weight = XlBorderWeight.xlThin;

                    nextRow = Math.Max(nextRow, row + matrixP.Count + 1);

                    // Устанавливаем следующий стобец
                    row += matrixP.Count + 2;
                }

                // Отображаем матрицу R
                {
                    // Получаем матрицу R
                    List<List<int>> matrixR = secondLevelOutput.R_Matrix;

                    // Объединяем несколько ячеек для заголовка
                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + batchSize]];
                    r.Merge(true);

                    // Выводим заголовок таблицы
                    metaDataSheet.Cells[row, col] = "Матрица R";
                    metaDataSheet.Cells[row, col].Font.Bold = true;
                    metaDataSheet.Cells[row, col].HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    // Устанавливаем автовыравнивание
                    r.Columns.AutoFit();

                    for (int dataType = 0; dataType < matrixR.Count; dataType++)
                        metaDataSheet.Cells[row + dataType + 2, col] = $"Тип {dataType + 1}";
                    for (int batchIndex = 0; batchIndex < matrixR[0].Count; batchIndex++)
                        metaDataSheet.Cells[row + 1, col + batchIndex + 1] = $"ПЗ {batchIndex + 1}";
                    for (int dataType = 0; dataType < matrixR.Count; dataType++)
                        for (int batchIndex = 0; batchIndex < matrixR[0].Count; batchIndex++)
                            metaDataSheet.Cells[row + dataType + 2, col + batchIndex + 1] = $"{matrixR[dataType][batchIndex]}";

                    // Обводим границы
                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + config.dataTypesCount + 1, col + matrixR[0].Count]];
                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                    r.Borders.Weight = XlBorderWeight.xlThin;

                    nextRow = Math.Max(nextRow, row + matrixR.Count + 1);

                    // Устанавливаем следующий стобец
                    row += matrixR.Count + 2;
                }

                // Отображаем матрицу Y
                {

                    // Объединяем несколько ячеек для заголовка
                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + batchSize]];
                    if ((bool)r.MergeCells)
                    {
                        r.UnMerge();
                    }
                    r.Merge(true);

                    // Выводим заголовок таблицы
                    metaDataSheet.Cells[row, col] = "Матрица Y";
                    metaDataSheet.Cells[row, col].Font.Bold = true;
                    metaDataSheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                    // Устанавливаем автовыравнивание
                    r.Columns.AutoFit();

                    for (int device = 0; device < matrixY.Count; device++)
                        metaDataSheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    for (int batchIndex = 0; batchIndex < matrixY[0].Count; batchIndex++)
                        metaDataSheet.Cells[row + 1, col + batchIndex + 1] = $"ПЗ {batchIndex + 1}";
                    for (int device = 0; device < matrixY.Count; device++)
                        for (int batchIndex = 0; batchIndex < matrixY[0].Count; batchIndex++)
                            metaDataSheet.Cells[row + device + 2, col + batchIndex + 1] = $"{matrixY[device][batchIndex]}";

                    // Обводим границы
                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + matrixY.Count + 1, col + matrixY[0].Count]];
                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                    r.Borders.Weight = XlBorderWeight.xlThin;

                    nextRow = Math.Max(nextRow, row + matrixY.Count + 1);

                    // Устанавливаем следующий стобец
                    row += matrixY.Count + 2;
                }

                // Отображаем матрицу T^pm
                {

                    // Получаем матрицу TPM
                    List<List<int>> matrixTPM = secondLevelOutput.Tpm_Matrix;

                    // Объединяем несколько ячеек для заголовка
                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + batchSize]];
                    r.Merge(true);

                    // Выводим заголовок таблицы
                    metaDataSheet.Cells[row, col] = "Матрица T^pm";
                    metaDataSheet.Cells[row, col].Font.Bold = true;
                    metaDataSheet.Cells[row, col].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                    // Устанавливаем автовыравнивание
                    r.Columns.AutoFit();

                    for (int device = 0; device < matrixTPM.Count; device++)
                        metaDataSheet.Cells[row + device + 2, col] = $"Прибор {device + 1}";
                    for (int batchIndex = 0; batchIndex < matrixTPM[0].Count; batchIndex++)
                        metaDataSheet.Cells[row + 1, col + batchIndex + 1] = $"ПЗ {batchIndex + 1}";
                    for (int device = 0; device < matrixTPM.Count; device++)
                        for (int batchIndex = 0; batchIndex < matrixTPM[0].Count; batchIndex++)
                            metaDataSheet.Cells[row + device + 2, col + batchIndex + 1] = $"{matrixTPM[device][batchIndex]}";

                    // Обводим границы
                    r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + matrixTPM.Count + 1, col + matrixTPM[0].Count]];
                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                    r.Borders.Weight = XlBorderWeight.xlThin;

                    nextRow = Math.Max(nextRow, row + matrixTPM.Count + 1);

                    // Устанавливаем следующий стобец
                    col += matrixTPM[0].Count + 2;
                    row = startRow;
                }

                // Отображаем матрицу T^0l
                {

                    // Получаем матрицу T^0l
                    Dictionary<int, List<List<int>>> startProcessing = secondLevelOutput.StartProcessing;

                    // Отображаем таблицу
                    for (int device = 0; device < config.deviceCount; device++)
                    {

                        // Объединяем несколько ячеек для заголовка
                        r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row, col + maxJobCount]];
                        r.Merge(true);

                        // Устанавливаем автовыравнивание
                        r.Columns.AutoFit();

                        // Заголовок
                        metaDataSheet.Cells[row, col] = $"Матрица T^0{device + 1}";
                        metaDataSheet.Cells[row, col].Font.Bold = true;
                        metaDataSheet.Cells[row, col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                        for (int batchIndex = 0; batchIndex < startProcessing[device].Count(); batchIndex++)
                            metaDataSheet.Cells[row + batchIndex + 2, col] = $"ПЗ {batchIndex+ 1}";
                        metaDataSheet.Cells[row + 1, col] = "Задание:";
                        for (int job = 0; job < maxJobCount; job++)
                            metaDataSheet.Cells[row + 1, col + job + 1] = job + 1;
                        for (int batchIndex = 0; batchIndex < startProcessing[device].Count(); batchIndex++)
                            for (int job = 0; job < startProcessing[device][batchIndex].Count; job++)
                                metaDataSheet.Cells[row + batchIndex + 2, col + job + 1] = startProcessing[device][batchIndex][job];

                        // Обводим границы
                        r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + startProcessing[device].Count() + 1, col + maxJobCount]];
                        r.Borders.LineStyle = XlLineStyle.xlContinuous;
                        r.Borders.Weight = XlBorderWeight.xlThin;

                        row += startProcessing[device].Count() + 2;
                    }

                    nextRow = Math.Max(nextRow, row + 1);

                    // Если установлен флаг отрисовки диагрммы Ганта
                    if (Form1.gantaOn) {

                        row = startRow;
                        col = col + maxJobCount + 2;

                        // Для каждого прибора
                        for (int device = 0; device < config.deviceCount; device++)

                            // Отображаем прибора
                            metaDataSheet.Cells[row + device + 1, col] = $"Прибор {device + 1}";

                        // Для каждого момента времени
                        for (int time = 0; time <= secondLevel.Makespan + preMConfig.preMaintenanceTimes.Max(); time++) {

                            // Отображаем время
                            metaDataSheet.Cells[row, col + time + 1] = time;
                            metaDataSheet.Cells[row, col + time + 1].Columns.AutoFit();
                        }

                        // Для каждого прибора
                        for (int device = 0; device < config.deviceCount; device++)
                        {

                            // Для каджого типа данных
                            for (int batchIndex = 0; batchIndex < startProcessing[device].Count; batchIndex++)
                            {

                                // Для каждого момента времени
                                for (int job = 0; job < startProcessing[device][batchIndex].Count; job++)
                                {

                                    // Выводим информацию
                                    metaDataSheet.Cells[row + device + 1, col + startProcessing[device][batchIndex][job] + 1] = $"Тип {secondLevelOutput.BatchType(batchIndex) + 1}";

                                    // Получаем диапазон задания
                                    r = metaDataSheet.Range[metaDataSheet.Cells[row + device + 1, col + startProcessing[device][batchIndex][job] + 1], metaDataSheet.Cells[row + device + 1, col + startProcessing[device][batchIndex][job] + config.proccessingTime[device][secondLevelOutput.BatchType(batchIndex)]]];
                                    r.Merge(true);
                                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                                    r.Borders.Weight = XlBorderWeight.xlThin;
                                    r.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Lime);
                                }

                                // Если в текущей позиции есть ПТО
                                if (matrixY[device][batchIndex] != 0)
                                {
                                    metaDataSheet.Cells[row + device + 1, col + startProcessing[device][batchIndex].Last() + config.proccessingTime[device][secondLevelOutput.BatchType(batchIndex)] + 1] = $"ПТО";
                                    r = metaDataSheet.Range[
                                        metaDataSheet.Cells[row + device + 1, col + startProcessing[device][batchIndex].Last() + config.proccessingTime[device][secondLevelOutput.BatchType(batchIndex)] + 1],
                                        metaDataSheet.Cells[row + device + 1, col + startProcessing[device][batchIndex].Last() + config.proccessingTime[device][secondLevelOutput.BatchType(batchIndex)] + preMConfig.preMaintenanceTimes[device]]
                                    ];
                                    r.Merge(true);
                                    r.Borders.LineStyle = XlLineStyle.xlContinuous;
                                    r.Borders.Weight = XlBorderWeight.xlThin;
                                    r.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Yellow);
                                }
                            }
                        }

                        // Устанавливаем границы
                        r = metaDataSheet.Range[metaDataSheet.Cells[row, col], metaDataSheet.Cells[row + config.deviceCount, col + secondLevel.Makespan + preMConfig.preMaintenanceTimes.Max() + 1]];
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
                metaDataSheet.Cells[nextRow,  1]
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
    }
}
