using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using magisterDiplom.Utils;
using magisterDiplom.Model;
using System;
using newAlgorithm.Model;

namespace newAlgorithm
{
    /// <summary>
    /// Базовая реализация алгоритма первого уровня формирования составов партий требований
    /// </summary>
    public class AllSolutionsFirstLevel : IFirstLevel
    {
        /// <summary>
        /// Данная структура данных содержит информацию о конфигурации конвейерной системы
        /// </summary>
        private readonly Config config;

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
        /// Аналог матрицы A - A'
        /// Матрица составов партий требований на k шаге.
        /// matrixA_Prime[i][h], где i - это тип данных. h - это индекс партии, а значение по индексам это количество партий
        /// </summary>
        public List<List<int>> PrimeMatrixA { get; private set; }
        
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
        private bool isBestSolution;
        private readonly string outputFileName;

        /// <summary>
        /// Конструктор с параметрами принимающий структуру конфигурации
        /// </summary>
        /// <param name="config">Структура конифгурации содержащая в себе информацию о конвейерной системе</param>
        /// <param name="batchCountList">Вектор длиной config.dataTypesCount, каждый элемент которого - это количество элементов в партии</param>
        /// <param name="outputFileName">Путь к файлу для вывода результатов</param>
        public AllSolutionsFirstLevel(Config config, List<int> batchCountList, string outputFileName)
        {
            this.config = config;
            this.batchCountList = batchCountList;
            this.outputFileName = outputFileName;
            _i = new List<int>(config.dataTypesCount);
        }

        /// <summary>
        /// Алгоритм формирования решения по составам паритй всех типов данных
        /// </summary>
        public void GenerateSolution()
        {
            using (var file = new StreamWriter(outputFileName))
            {
                GenerateFixedBatchesSolution();
                var shedule = new Shedule(PrimeMatrixA);
                //shedule.ConstructShedule();
                shedule.ConstructSheduleWithBuffer(config.buffer, config.dataTypesCount);
                f1Current = shedule.GetTime();

                MessageBox.Show(ListUtils.MatrixIntToString(PrimeMatrixA, ", ", "", ";") + "Время обработки " + f1Current);
                f1Optimal = f1Current;
                file.WriteLine(f1Optimal);
                isBestSolution = true;

                // Генерируем начальное решение
                GenerateStartSolution();

                shedule = new Shedule(PrimeMatrixA);
                //shedule.ConstructShedule();
                shedule.ConstructSheduleWithBuffer(config.buffer, config.dataTypesCount);
                f1Current = shedule.GetTime();
                MessageBox.Show(ListUtils.MatrixIntToString(PrimeMatrixA, ", ", "", ";") + " Время обработки " + f1Current);
                if (f1Current < f1Optimal)
                {
                    bestMatrixA = ListUtils.MatrixIntDeepCopy(PrimeMatrixA);
                    isBestSolution = true;
                    f1Optimal = f1Current;
                    file.Write(" +");
                }
                if (!config.isFixedBatches)
                {

                    // До тех пор, поа не расмотрели все типы выполняем обработку
                    while (CheckType())
                    {
                        // Буферезируем текущее решение для построение нового на его основе
                        _ai = ListUtils.MatrixIntDeepCopy(PrimeMatrixA);
                        if (isBestSolution)
                        {
                            _a1 = new List<List<List<int>>>();

                            // Для каждого типа данных выполняем обработку
                            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
                            {
                                _a1.Add(new List<List<int>>());
                                _a1[dataType].Add(new List<int>());
                                _a1[dataType][0] = ListUtils.VectorIntDeepCopy(PrimeMatrixA[dataType]);
                            }
                            isBestSolution = false;
                        }

                        bestMatrixA = ListUtils.MatrixIntDeepCopy(_ai);
                        f1Optimal = f1Current;

                        // Для каждого типа и каждого решения в типе строим новое решение и проверяем его на критерий
                        // Строим A2 и параллельно проверяем критерий
                        _a2 = new List<List<List<int>>>(config.dataTypesCount);

                        // Выполяем инициализацию
                        _a2.AddRange(Enumerable.Repeat(new List<List<int>>(), config.dataTypesCount));
                        
                        file.WriteLine("окрестность 1 вида");

                        // Для каждого типа данных в рассмотрении (_i[dataType] != 0) выполняем обработку
                        for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
                        {

                            // Если данный тип данных не находится в рассмотрении
                            if (_i[dataType] <= 0)

                                // Пропускаем итерацию
                                continue;

                            // Формируем новый состав партий для типа dataType
                            _a2[dataType] = NewData(dataType);

                            // Для каждого пакета в новом составе партий выполняем обработку
                            for (var batchIndex = 0; batchIndex < _a2[dataType].Count; batchIndex++)
                            {
                                var tempA = SetTempAFromA2(dataType, batchIndex);
                                shedule = new Shedule(tempA);
                                //shedule.ConstructShedule();
                                shedule.ConstructSheduleWithBuffer(config.buffer, config.dataTypesCount);
                                var fBuf = shedule.GetTime();
                                string s = ListUtils.MatrixIntToString(tempA, ", ", "", ";");
                                file.Write(s + " " + fBuf);
                                MessageBox.Show(s + " Время обработки " + fBuf);                                    
                                if (fBuf < f1Optimal)
                                {
                                    bestMatrixA = ListUtils.MatrixIntDeepCopy(tempA);
                                    isBestSolution = true;
                                    f1Optimal = fBuf;
                                    file.Write(" +");
                                }
                                file.WriteLine();
                            }
                        }
                        if (!isBestSolution)
                        {
                            file.WriteLine("комбинации типов");
                            CombinationType(file, _a2, 0, null, ref isBestSolution);
                        }

                        if (isBestSolution)
                        {
                            MessageBox.Show("Лучшее решение " + ListUtils.MatrixIntToString(bestMatrixA, ", ", "", ";") + " Время обработки " + f1Optimal);
                            PrimeMatrixA = ListUtils.MatrixIntDeepCopy(bestMatrixA);
                            f1Current = f1Optimal;

                            continue;
                        }

                        // Для каждого типа данных выполняем обработку
                        for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                        {
                            _a1[dataType] = ListUtils.MatrixIntDeepCopy(_a2[dataType]);
                            if (!_a1[dataType].Any() || !_a1[dataType][0].Any())
                                _i[dataType] = 0;
                        }
                    }
                }
                file.WriteLine(f1Current);
                file.Close();
                MessageBox.Show("Решения найдены");
            }
        }

        /// <summary>
        /// Алгоритм формирования фиксированных партий
        /// </summary>
        private void GenerateFixedBatchesSolution()
        {
            // Инициализируем строки матрицы A
            PrimeMatrixA = new List<List<int>>();

            // Для каждого типа данных выполняем обработку
            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
            {

                // Добавляем в вектор _i элемент 1 в конец списка
                _i.Add(1);

                // Инициализируем столбцы матрицы A
                PrimeMatrixA.Add(new List<int>());

                // Для каждой строки матрицы A добавляем вектор количеств элементов в партии
                PrimeMatrixA[dataType].Add(batchCountList[dataType]);
            }
        }

        /// <summary>
        /// Алгоритм формирования начальных решений по составам партий всех типов
        /// </summary>
        private void GenerateStartSolution()
        {
            // Минимальное количество данных в партии
            const int minBatchSize = 2;

            // Выполяем отчистку вектора _i и матрицы _a
            _i.Clear();
            PrimeMatrixA?.Clear();

            // Инициализируем матрицу A
            PrimeMatrixA = new List<List<int>>();
            
            // Для каждого типа данных выполняем обработку
            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
            {

                // Для каждого типа указываем, что они находятся на расмотрении
                _i.Add(1);

                // Для каждого типа создаём вектор с составом партий и формируем его, как [n_p - 2, 2]
                PrimeMatrixA.Add(new List<int>());
                PrimeMatrixA[dataType].Add(batchCountList[dataType] - minBatchSize);
                PrimeMatrixA[dataType].Add(minBatchSize);
            }

            // Для каждого типа данных выполняем проверку
            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
            
                // Выполяем проверку на отсутсвие единичных партий
                if (PrimeMatrixA[dataType][0] < 2 || PrimeMatrixA[dataType][0] < PrimeMatrixA[dataType][1])
                {
                    PrimeMatrixA[dataType].Clear();
                    PrimeMatrixA[dataType].Add(batchCountList[dataType]);
                    _i[dataType] = 0;
                }
        }

        /// <summary>
        /// Функция проверки наличия оставшихся в расмотрении типов
        /// </summary>
        private bool CheckType()
        {
            // Для каждого типа данных выполняем проверку на не нулевое количество типов
            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)

                // Проверяем количество типов
                if (_i[dataType] > 0)

                    // Если в списке количество больше 0, значит типы ещё есть в расмотрении
                    return true;

            // Все типы были расмотрены
            return false;
        }

        /// <summary>
        /// Построчное формирование матрицы промежуточного решени
        /// </summary>
        private List<List<int>> SetTempAFromA2(int dataType, int batchIndex)
        {
            var result = ListUtils.MatrixIntDeepCopy(PrimeMatrixA);
            if (batchIndex < _a2[dataType].Count)
                result[dataType] = ListUtils.VectorIntDeepCopy(_a2[dataType][batchIndex]);
            return result;
        }

        /// <summary>
        /// Рекурсивная комбинация всех типов _a2 с фиксированным решением _a
        /// </summary>
        private void CombinationType(StreamWriter file, List<List<List<int>>> tempMatrix, int type, List<List<int>> tempM, ref bool solutionFlag)
        {
            if (type < config.dataTypesCount)
            {
                for (var variantOfSplitIndex = 0; variantOfSplitIndex < _a2[type].Count; variantOfSplitIndex++)
                {
                    List<List<int>> tempB = (tempM != null) ? ListUtils.MatrixIntDeepCopy(tempM) : new List<List<int>>();

                    tempB.Add(tempMatrix[type][variantOfSplitIndex]);
                    CombinationType(file, tempMatrix, type + 1, tempB, ref solutionFlag);
                }
            } else
            {
                var shedule = new Shedule(tempM);
                shedule.ConstructSheduleWithBuffer(config.buffer, config.dataTypesCount);
                var fBuf = shedule.GetTime();
                string s = ListUtils.MatrixIntToString(tempM, ", ", "", ";");
                file.Write(s + " " + fBuf);
                if (fBuf < f1Optimal)
                {
                    bestMatrixA = ListUtils.MatrixIntDeepCopy(tempM);
                    solutionFlag = true;
                    f1Optimal = fBuf;
                    file.Write(" +");
                    return;
                }
                file.WriteLine();
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
                if (result.Any() && result[result.Count - 1][0] != row[0]) continue;
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
            foreach (var row2 in _a1[dataType])
            {
                foreach (var rowMatrix in inMatrix.ToList())
                {
                    if (rowMatrix.Zip(row2, (a, b) => new { a, b }).All(pair => pair.a == pair.b))
                    {
                        inMatrix.Remove(rowMatrix);
                    }
                }
            }
            return inMatrix;
        }
    }
}
