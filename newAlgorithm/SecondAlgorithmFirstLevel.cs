using System.Collections.Generic;
using System.Linq;
using System.IO;
using magisterDiplom.Utils;
using magisterDiplom.Model;
using System;
using newAlgorithm.Model;

namespace newAlgorithm
{
    /// <summary>
    /// Реализация второго алгоритма первого уровня
    /// </summary>
    public class SecondAlgorithmFirstLevel : IFirstLevel
    {
        /// <summary>
        /// Данная структура данных содержит информацию о конфигурации конвейерной системы
        /// </summary>
        private readonly Config config;

        /// <summary>
        /// Данная переменная определяет вектор количества требований для каждого типа данных
        /// </summary>
        private readonly List<int> batchCountList;

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
        /// </summary>
        private List<List<List<int>>> _a2;              

        /// <summary>
        /// Лучший критерий f1
        /// </summary>
        private int f1Optimal;

        /// <summary>
        /// Флаг, указывающий, было ли найдено лучшее решение на текущей итерации
        /// </summary>
        private bool isBestSolution;

        /// <summary>
        /// Матрица составов партий требований на текущем шаге
        /// </summary>
        public List<List<int>> PrimeMatrixA { get; private set; }

        /// <summary>
        /// Конструктор класса SetsFirstLevel.
        /// </summary>
        /// <param name="config">Структура конфигурации, содержащая информацию о конвейерной системе.</param>
        /// <param name="batchCountList">Вектор длиной config.dataTypesCount, каждый элемент которого - это количество элементов в партии.</param>
        public SecondAlgorithmFirstLevel(Config config, List<int> batchCountList)
        {
            this.config = config;
            this.batchCountList = batchCountList;
            _i = new List<int>(config.dataTypesCount);
        }

        /// <summary>
        /// Основной метод для запуска процесса генерации решения на основе множеств.
        /// </summary>
        public void GenerateSolution()
        {
            var sets = new Sets(Form1.compositionSets, Form1.timeSets);
            // Генерируем начальное решение
            GenerateStartSolution();
            // Создаём экземпляр класса расписания с помощью матрицы Aprime
            var shedule = new Shedule(PrimeMatrixA);
            // Выполняем построение расписания
            shedule.ConstructShedule();
            shedule.BuildMatrixRWithTime();
            var matrixRWithTime = shedule.ReturnMatrixRWithTime();
            sets.GetSolution(matrixRWithTime);
            var time = sets.GetNewCriterion(Form1.direct);
            f1Optimal = time;
            isBestSolution = true;

            // До тех пор пока в наличие есть оставшиеся типы и партии не фиксированные выполняем обработку
            while (CheckType() && !config.isFixedBatches)
            {
                // Буферезируем текущее решение для построение нового на его основе
                _ai = ListUtils.MatrixIntDeepCopy(PrimeMatrixA);
                if (isBestSolution)
                {
                    _a1 = new List<List<List<int>>>();
                    // Для каждого типа данных выполняем обработку
                    for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
                        _a1.Add(new List<List<int>> { ListUtils.VectorIntDeepCopy(PrimeMatrixA[dataType]) });
                    isBestSolution = false;
                }

                bestMatrixA = ListUtils.MatrixIntDeepCopy(_ai);
                _a2 = new List<List<List<int>>>();

                // Для каждого типа и каждого решения в типе строим новое решение и проверяем его на критерий
                for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
                {
                    _a2.Add(new List<List<int>>());
                    if (_i[dataType] <= 0) continue;
                    _a2[dataType] = NewData(dataType);
                    for (var j = 0; j < _a2[dataType].Count; j++)
                    {
                        var tempA = SetTempAFromA2(dataType, j);
                        shedule = new Shedule(tempA);
                        shedule.ConstructShedule();
                        shedule.BuildMatrixRWithTime();
                        matrixRWithTime = shedule.ReturnMatrixRWithTime();
                        sets = new Sets(Form1.compositionSets, Form1.timeSets);
                        sets.GetSolution(matrixRWithTime);
                        var curTime = sets.GetNewCriterion(Form1.direct);
                        if (curTime < f1Optimal)
                        {
                            bestMatrixA = ListUtils.MatrixIntDeepCopy(tempA);
                            isBestSolution = true;
                            f1Optimal = curTime;
                        }
                    }
                }
                if (!isBestSolution)
                {
                    var nTemp = Enumerable.Repeat(0, config.dataTypesCount).ToList();
                    GenerateCombination(nTemp);
                }
                if (isBestSolution)
                {
                    PrimeMatrixA = ListUtils.MatrixIntDeepCopy(bestMatrixA);
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

        /// <summary>
        /// Алгоритм формирования начальных решений по составам партий всех типов
        /// </summary>
        private void GenerateStartSolution()
        {
            const int minBatchSize = 2;
            _i.Clear();
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
            return _i.Any(v => v > 0);
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
        /// Формирование перебора для всех возможных решений из А2
        /// </summary>
        private void GenerateCombination(List<int> _n)
        {
            // Для каждого типа данных выполняем обработку с конца
            for (int dataType = config.dataTypesCount - 1; dataType >= 0; dataType--)
            {
                for (int j = 0; j < _a2[dataType].Count; j++)
                {
                    _n[dataType] = j;
                    GetSolution(_n);
                }        
            }
        }

        /// <summary>
        /// Подстановка данных из перебора и вычисление решения
        /// </summary>
        private void GetSolution(List<int> _n)
        {
            var tempA = ListUtils.MatrixIntDeepCopy(PrimeMatrixA);
            for (var dataType = 0; dataType < config.dataTypesCount; dataType++)
                if (_n[dataType] >= 0)
                    tempA[dataType] = ListUtils.VectorIntDeepCopy(SetTempAFromA2(dataType, _n[dataType])[dataType]);

            var shedule = new Shedule(tempA);
            shedule.ConstructSheduleWithBuffer(3, config.dataTypesCount);
            shedule.BuildMatrixRWithTime();
            var matrixRWithTime = shedule.ReturnMatrixRWithTime();
            var sets = new Sets(Form1.compositionSets, Form1.timeSets);
            sets.GetSolution(matrixRWithTime);
            var time = sets.GetNewCriterion(Form1.direct);
            if (time < f1Optimal)
            {
                bestMatrixA = ListUtils.MatrixIntDeepCopy(tempA);
                isBestSolution = true;
                f1Optimal = time;
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
                if (result.Count > 0 && result[result.Count - 1][0] != row[0]) continue;
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
            return SortedMatrix(result);
        }

        /// <summary>
        /// Функция получения неповторяющихся решений
        /// </summary>
        private List<List<int>> SortedMatrix(List<List<int>> inMatrix)
        {
            return inMatrix.GroupBy(l => string.Join(",", l)).Select(g => g.First()).ToList();
        }
    }
}
