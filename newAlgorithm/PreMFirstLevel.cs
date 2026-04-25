using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using magisterDiplom.Utils;
using magisterDiplom.Model;
using System;
using newAlgorithm.Model;
using magisterDiplom.UI.Visualizer;

namespace newAlgorithm
{
    /// <summary>
    /// Реализация алгоритма первого уровня с учетом технического обслуживания (ПТО)
    /// </summary>
    public class PreMFirstLevel : IFirstLevel
    {
        /// <summary>
        /// Данная структура данных содержит информацию о конфигурации конвейерной системы
        /// </summary>
        private readonly Config config;

        /// <summary>
        /// Параметры, характеризующие планово-предупредительное обслуживание (ПТО)
        /// </summary>
        private readonly PreMConfig preMConfig;

        /// <summary>
        /// Имя файла для вывода результатов
        /// </summary>
        private readonly string outputFileName;

        /// <summary>
        /// Объект для визуализации результатов в Excel
        /// </summary>
        private readonly ExcelVisualizer _visualizer;
        
        /// <summary>
        /// Данная переменная определяет вектор данных для интерпритации типов данных
        /// </summary>
        private readonly List<int> _i;

        /// <summary>
        /// Буферизированная матрица составов партий требований на k+1 шаге
        /// </summary>
        private List<List<int>> _ai;                     
        
        /// <summary>
        /// Лучшая матрица составов партий
        /// </summary>
        private List<List<int>> bestMatrixA;

        /// <summary>
        /// Матрица составов партий требований на k+1 шаге
        /// </summary>
        private List<List<List<int>>> _a1;

        /// <summary>
        /// Матрица составов партий требований фиксированного типа
        /// [dataTypesCount x ??? x ???] 
        /// </summary>
        private List<List<List<int>>> _a2;              

        /// <summary>
        /// Данная переменная определяет вектор количества требований для каждого типа данных
        /// </summary>
        private readonly List<int> batchCountList;        // Начальное количество требований для каждого типа данных
        
        /// <summary>
        /// Текущий критерий f1
        /// </summary>
        private int f1Current;
        
        /// <summary>
        /// Лучший критерий f1
        /// </summary>
        private int f1Optimal;

        /// <summary>
        /// Флаг, указывающий, было ли найдено лучшее решение на текущей итерации
        /// </summary>
        private bool isBestSolution;

        /// <summary>
        /// Номер состава пакетов
        /// </summary>
        private int compositionNumber;

        /// <summary>
        /// Строка с именем файла для логирования
        /// </summary>
        private string logFileNamePrefix;

        /// <summary>
        /// Аналог матрицы A - A'
        /// Матрица составов партий требований на k шаге.
        /// matrixA_Prime[i][h], где i - это тип данных. h - это индекс партии, а значение по индексам это количество партий
        /// </summary>
        public List<List<int>> PrimeMatrixA { get; private set; }

        /// <summary>
        /// Конструктор класса PreMFirstLevel.
        /// </summary>
        /// <param name="config">Структура конфигурации, содержащая информацию о конвейерной системе.</param>
        /// <param name="preMConfig">Параметры, характеризующие планово-предупредительное обслуживание (ПТО).</param>
        /// <param name="batchCountList">Вектор длиной config.dataTypesCount, каждый элемент которого - это количество элементов в партии.</param>
        /// <param name="outputFileName">Путь к файлу для вывода результатов.</param>
        public PreMFirstLevel(Config config, PreMConfig preMConfig, List<int> batchCountList, string outputFileName)
        {
            this.config = config;
            this.preMConfig = preMConfig;
            this.batchCountList = batchCountList;
            this.outputFileName = outputFileName;
            _i = new List<int>(config.dataTypesCount);
            if (Form1.vizualizationOn)
            {
                _visualizer = new ExcelVisualizer(config);
            }
        }

        /// <summary>
        /// Основной метод для запуска процесса генерации решения с учетом ПТО.
        /// </summary>
        public void GenerateSolution()
        {
            // Устанавливаем номер строки для вспомогательных данных
            int helpRowNumber = 1;

            // Формируем имя файла для логирования
            logFileNamePrefix = $"{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}_{DateTime.Now.Hour}_{DateTime.Now.Minute}";

            // Инициализируем номер состава пакетов
            compositionNumber = 1;

            // Если визуализация включена
            if (Form1.vizualizationOn) {
                // Инициализируем объект для работы с Excel через Visualizer
                _visualizer.Initialize(preMConfig);
            }

            // Переопределяем значение оптимального критерия f1
            f1Optimal = int.MaxValue;
            f1Current = int.MaxValue;

            using (var file = new StreamWriter(outputFileName))
            {
                // Создаём экземпляр класса для работы с нижним уровнем
                SimplePreMSchedule schedule = new SimplePreMSchedule(config, preMConfig);
                
                // Обработка фиксированных пакетов
                {
                    GenerateFixedBatchesSolution();
                    // Если флаг логгирования установлен
                    if (Form1.loggingOn) schedule.SetLogFile($"{logFileNamePrefix}_{compositionNumber}.log");

                    // Если построение расписание выполнено удачно
                    if (schedule.Build(PrimeMatrixA))
                    {
                        // Если установлен флаг визуализации
                        if (Form1.vizualizationOn) _visualizer.VisualizeResult(compositionNumber, schedule, PrimeMatrixA, preMConfig, ref helpRowNumber);
                        // Получаем f1 критерий
                        f1Current = schedule.GetMakespan();
                        file.WriteLine(f1Current);
                        isBestSolution = true;
                        compositionNumber++;
                    } 
                    else if (Form1.vizualizationOn && Form1.showND)
                    {
                        // Визуализируем промежуточные данные (неудачно)
                        _visualizer.VisualizeResult(compositionNumber, schedule, PrimeMatrixA, preMConfig, ref helpRowNumber, false);
                        compositionNumber++;
                    }
                }

                // Сбрасываем значения критериев для основного алгоритма
                f1Optimal = int.MaxValue;
                f1Current = int.MaxValue;
                // Генерируем начальное решение
                GenerateStartSolution();

                // Если флаг логгирования установлен
                if (Form1.loggingOn) schedule.SetLogFile($"{logFileNamePrefix}_{compositionNumber}.log");

                // Вызываем расчёты
                if (schedule.Build(PrimeMatrixA)) {
                    // Получаем f1
                    f1Current = schedule.GetMakespan();
                    if (Form1.vizualizationOn) _visualizer.VisualizeResult(compositionNumber, schedule, PrimeMatrixA, preMConfig, ref helpRowNumber);
                    
                    compositionNumber++;
                    // Если текущей критерий лучше оптимального
                    if (f1Current < f1Optimal)
                    {
                        // Если установлен флаг визуализации
                        if (Form1.vizualizationOn) _visualizer.MarkAsOptimal(compositionNumber - 1);
                        // Копируем матрицу с лучшим решением
                        bestMatrixA = ListUtils.MatrixIntDeepCopy(PrimeMatrixA);
                        // Устанавливаем флаг лучшего решения
                        isBestSolution = true;
                        // Переопределяем критерий f1 лучшего решения
                        f1Optimal = f1Current;
                        // Логируем нахождение лучшего решения
                        file.Write(" +");
                    }
                } 
                else if (Form1.vizualizationOn && Form1.showND)
                {
                    // Визуализируем промежуточные данные
                    _visualizer.VisualizeResult(compositionNumber, schedule, PrimeMatrixA, preMConfig, ref helpRowNumber, false);
                    compositionNumber++;
                }

                // Инициализируем матрицу _a1
                _a1 = new List<List<List<int>>>();
                for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
                {
                    _a1.Add(new List<List<int>> { ListUtils.VectorIntDeepCopy(PrimeMatrixA[dataType]) });
                }

                // Если пакеты не фиксированные
                if (!config.isFixedBatches)
                {
                    // До тех пор, пока не рассмотрели все типы, выполняем обработку
                    while (CheckType())
                    {
                        // Буферизируем текущее решение для построения нового на его основе
                        _ai = ListUtils.MatrixIntDeepCopy(PrimeMatrixA);
                        // Если текущее решение лучше
                        if (isBestSolution)
                        {
                            // Копируем текущее решение во временный массив _a1
                            _a1 = new List<List<List<int>>>();
                            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
                                _a1.Add(new List<List<int>> { ListUtils.VectorIntDeepCopy(PrimeMatrixA[dataType]) });
                            // Сбрасываем флаг лучшего решения
                            isBestSolution = false;
                        }

                        bestMatrixA = ListUtils.MatrixIntDeepCopy(_ai);
                        f1Optimal = f1Current;
                        _a2 = new List<List<List<int>>>(config.dataTypesCount);
                        _a2.AddRange(Enumerable.Repeat(new List<List<int>>(), config.dataTypesCount));
                        file.WriteLine("окрестность 1 вида");

                        // Для каждого типа данных в рассмотрении (_i[dataType] != 0) выполняем обработку
                        for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
                        {
                            // Если данный тип данных не находится в рассмотрении
                            if (_i[dataType] <= 0) continue;
                            // Формируем новый состав партий для типа dataType
                            _a2[dataType] = NewData(dataType);
                            // Для каждого пакета в новом составе партий выполняем обработку
                            for (var batchIndex = 0; batchIndex < _a2[dataType].Count; batchIndex++)
                            {
                                var tempA = SetTempAFromA2(dataType, batchIndex);
                                // Если флаг логгирования установлен
                                if (Form1.loggingOn) schedule.SetLogFile($"{logFileNamePrefix}_{compositionNumber}.log");

                                // Если расписание построилось не успешно
                                if (!schedule.Build(tempA))
                                {
                                    if (Form1.vizualizationOn && Form1.showND)
                                    {
                                        _visualizer.VisualizeResult(compositionNumber, schedule, tempA, preMConfig, ref helpRowNumber, false);
                                        compositionNumber++;
                                    }
                                    // Пропускаем обработку
                                    continue;
                                }

                                // Получаем критерий f1
                                var fBuf = schedule.GetMakespan();
                                if (Form1.vizualizationOn) _visualizer.VisualizeResult(compositionNumber, schedule, tempA, preMConfig, ref helpRowNumber);
                                
                                compositionNumber++;
                                string s = ListUtils.MatrixIntToString(tempA, ", ", "", ";");
                                file.Write(s + " " + fBuf);

                                // Если текущее решение лучше
                                if (fBuf < f1Optimal)
                                {
                                    if (Form1.vizualizationOn) _visualizer.MarkAsOptimal(compositionNumber - 1);
                                    // Копируем матрицу с лучшим решением
                                    bestMatrixA = ListUtils.MatrixIntDeepCopy(tempA);
                                    // Устанавливаем флаг лучшего решения
                                    isBestSolution = true;
                                    // Переопределяем критерий f1 лучшего решения
                                    f1Optimal = fBuf;
                                    // Логируем нахождение лучшего решения
                                    file.Write(" +");
                                }
                                // Логируем
                                file.WriteLine();
                            }
                        }
                        
                        // Если лучшее решения не было найдено
                        if (!isBestSolution)
                        {
                            file.WriteLine("комбинации типов");
                            // Формируем следующий состав пакетов заданий
                            CombinationTypeWithPremaintences(file, _a2, 0, null, ref isBestSolution, ref schedule, ref preMConfig, ref helpRowNumber);
                        }

                        // Если лучшее решения было найдено
                        if (isBestSolution)
                        {
                            // Запоминаем лучшее решение
                            PrimeMatrixA = ListUtils.MatrixIntDeepCopy(bestMatrixA);
                            f1Current = f1Optimal;
                            continue;
                        }

                        // Для каждого типа данных выполняем обработку
                        for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                        {
                            _a1[dataType] = ListUtils.MatrixIntDeepCopy(_a2[dataType]);
                            if (!_a1[dataType].Any() || !_a1[dataType][0].Any()) _i[dataType] = 0;
                        }
                    }
                }

                // Проверяем успешность работы программы
                if (f1Optimal == int.MaxValue)
                {
                    MessageBox.Show("Решения не было найдено");
                    return;
                }

                // Если флаг визуализации установлен
                if (Form1.vizualizationOn) _visualizer.CreateFinalChart(compositionNumber);

                // Логируем лучший критерий f1
                file.WriteLine(f1Optimal);
                file.Close();
                MessageBox.Show("Решения найдены f1 = " + f1Optimal);
            }
        }

        /// <summary>
        /// Алгоритм формирования фиксированных партий
        /// </summary>
        private void GenerateFixedBatchesSolution()
        {
            _i.Clear();
            PrimeMatrixA = new List<List<int>>();
            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
            {
                _i.Add(1);
                PrimeMatrixA.Add(new List<int> { batchCountList[dataType] });
            }
        }

        /// <summary>
        /// Алгоритм формирования начальных решений по составам партий всех типов
        /// </summary>
        private void GenerateStartSolution()
        {
            const int minBatchSize = 2;
            _i.Clear();
            PrimeMatrixA?.Clear();
            PrimeMatrixA = new List<List<int>>();
            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
            {
                _i.Add(1);
                PrimeMatrixA.Add(new List<int> { batchCountList[dataType] - minBatchSize, minBatchSize });
            }
            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
                if (PrimeMatrixA[dataType][0] < 2 || PrimeMatrixA[dataType][0] < PrimeMatrixA[dataType][1])
                {
                    PrimeMatrixA[dataType] = new List<int> { batchCountList[dataType] };
                    _i[dataType] = 0;
                }
        }

        /// <summary>
        /// Функция проверки наличия оставшихся в рассмотрении типов
        /// </summary>
        private bool CheckType()
        {
            return _i.Any(val => val > 0);
        }

        /// <summary>
        /// Построчное формирование матрицы промежуточного решения
        /// </summary>
        private List<List<int>> SetTempAFromA2(int dataType, int batchIndex)
        {
            var result = ListUtils.MatrixIntDeepCopy(PrimeMatrixA);
            if (batchIndex < _a2[dataType].Count)
                result[dataType] = ListUtils.VectorIntDeepCopy(_a2[dataType][batchIndex]);
            return result;
        }

        /// <summary>
        /// Рекурсивная комбинация всех типов _a2 с фиксированным решением _a (с учетом ПТО)
        /// </summary>
        private void CombinationTypeWithPremaintences(StreamWriter file, List<List<List<int>>> tempMatrix, int type, List<List<int>> tempM, ref bool solutionFlag, ref SimplePreMSchedule schedule, ref PreMConfig preMConfig, ref int helpRowNumber)
        {
            if (type < config.dataTypesCount)
            {
                for (var variantOfSplitIndex = 0; variantOfSplitIndex < _a2[type].Count; variantOfSplitIndex++)
                {
                    List<List<int>> tempB = (tempM == null) ? new List<List<int>>() : ListUtils.MatrixIntDeepCopy(tempM);
                    tempB.Add(tempMatrix[type][variantOfSplitIndex]);
                    CombinationTypeWithPremaintences(file, tempMatrix, type + 1, tempB, ref solutionFlag, ref schedule, ref preMConfig, ref helpRowNumber);
                }
            }
            else
            {
                if (Form1.loggingOn) schedule.SetLogFile($"{logFileNamePrefix}_{compositionNumber}.log");

                if (schedule.Build(tempM))
                {
                    if (Form1.vizualizationOn) _visualizer.VisualizeResult(compositionNumber, schedule, tempM, preMConfig, ref helpRowNumber);
                    var fBuf = schedule.GetMakespan();
                    string s = ListUtils.MatrixIntToString(tempM, ", ", "", ";");
                    file.Write(s + " " + fBuf);
                    if (fBuf < f1Optimal)
                    {
                        if (Form1.vizualizationOn) _visualizer.MarkAsOptimal(compositionNumber);
                        bestMatrixA = ListUtils.MatrixIntDeepCopy(tempM);
                        solutionFlag = true;
                        f1Optimal = fBuf;
                        file.Write(" +");
                        return;
                    }
                    compositionNumber++;
                    file.WriteLine();
                }
                else if (Form1.vizualizationOn && Form1.showND)
                {
                    _visualizer.VisualizeResult(compositionNumber, schedule, tempM, preMConfig, ref helpRowNumber, false);
                    compositionNumber++;
                }
            }
        }

        /// <summary>
        /// Формирование новых решений по составам партий текущего типа данных
        /// </summary>
        private List<List<int>> NewData(int dataType)
        {
            var result = new List<List<int>>();
            foreach(var row in _a1[dataType])
            {
                for (var j = 1; j < row.Count; j++)
                {
                    result.Add(ListUtils.VectorIntDeepCopy(row));
                    if (row[0] <= row[j] + 1) continue;
                    result[result.Count - 1][0]--;
                    result[result.Count - 1][j]++;
                }
                if (result[result.Count - 1][0] != row[0]) continue;
                {
                    var summ = row.Sum();
                    var newRow = Enumerable.Repeat(2, row.Count).ToList();
                    newRow.Add(2);
                    newRow[0] = summ - 2 * (newRow.Count - 1);
                    result.Add(newRow);
                }
            }
            result.RemoveAll(r => {
                for (int i = 1; i < r.Count; i++) if (r[i] > r[i-1]) return true;
                return false;
            });
            result = SortedMatrix(result);
            result = CheckMatrix(result, dataType);
            return result;
        }

        /// <summary>
        /// Функция получения неповторяющихся решений в матрице А2 на шаге 9
        /// </summary>
        private List<List<int>> SortedMatrix(List<List<int>> inMatrix)
        {
            return inMatrix.GroupBy(l => string.Join(",", l)).Select(g => g.First()).ToList();
        }

        /// <summary>
        /// Удаление повторений новых решений совпадающих с A1
        /// </summary>
        private List<List<int>> CheckMatrix(List<List<int>> inMatrix, int dataType)
        {
            return inMatrix.Where(rowMatrix => !_a1[dataType].Any(row2 => rowMatrix.SequenceEqual(row2))).ToList();
        }
    }
}
