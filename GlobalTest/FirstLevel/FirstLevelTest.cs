using magisterDiplom;
using magisterDiplom.Model;
using newAlgorithm;
using newAlgorithm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTest.FirstLevel
{
    [TestClass]
    public class FirstLevelTest
    {

        [TestMethod]
        public void GenerateStartSolutionTest()
        {

            // Формируем входные данные
            #region Init input

            /*
            // dataTypesCount:
            // 3
            // 
            // deviceCount:
            // 3
            // 
            // buffer:
            // 3
            // 
            // proccessingTime:
            // +---+---+---+
            // | 1 | 2 | 5 |
            // +---+---+---+
            // | 3 | 4 | 5 |
            // +---+---+---+
            // | 3 | 4 | 5 |
            // +---+---+---+
            //
            // changeoverTime:
            // +---+---+---+---+
            // |   | 1 | 2 | 5 |
            // +   +---+---+---+
            // | 1 | 3 | 4 | 5 |
            // +   +---+---+---+
            // |   | 3 | 4 | 5 |
            // +---+---+---+---+
            // |   | 4 | 2 | 5 |
            // +   +---+---+---+
            // | 2 | 3 | 1 | 5 |
            // +   +---+---+---+
            // |   | 3 | 4 | 5 |
            // +---+---+---+---+
            // |   | 4 | 2 | 5 |
            // +   +---+---+---+
            // | 3 | 3 | 1 | 5 |
            // +   +---+---+---+
            // |   | 3 | 4 | 5 |
            // +---+---+---+---+
            //
            // preMaintenanceTimes
            // +---+---+---+
            // | 3 | 1 | 5 |
            // +---+---+---+
            //
            // isFixedBatches:
            // false
            */

            // Объявляем матрицу переналадки
            Dictionary<int, List<List<int>>> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            List<List<int>> changeoverTime_1 = new()
            {
                    new() { 1, 2, 5 },
                    new() { 3, 4, 5 },
                    new() { 3, 4, 5 },
                };

            // Создаём матрицу переналадки для 2 прибора
            List<List<int>> changeoverTime_2 = new()
            {
                    new() { 4, 2, 5 },
                    new() { 3, 1, 5 },
                    new() { 3, 4, 5 },
                };

            // Создаём матрицу переналадки для 3 прибора
            List<List<int>> changeoverTime_3 = new()
            {
                    new() { 4, 2, 5 },
                    new() { 3, 1, 5 },
                    new() { 3, 4, 5 },
                };

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);
            changeoverTime.Add(2, changeoverTime_3);

            // Создаём матрицу времени выполнения
            List<List<int>> proccessingTime = new()
            {
                    new() { 1, 2, 5 },
                    new() { 3, 4, 5 },
                    new() { 3, 4, 5 },
                };

            // Формируем конфигурационный файл
            Config config = new(
                3, // int dataTypesCount,
                3, // int deviceCount,
                3, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                false// bool isFixedBatches
            );

            #endregion

            var firstLevel = new newAlgorithm.FirstLevel(config, new List<int> { 12, 12, 12 });
            var output = new List<List<int>> {
                new() { 10, 2 },
                new() { 10, 2 },
                new() { 10, 2 } };
            firstLevel.GenerateStartSolution();

            for (int dataType = 0; dataType < output.Count; dataType++)
                for (int batch = 0; batch < output[dataType].Count; batch++)
                    Assert.AreEqual(output[dataType][batch], firstLevel.PrimeMatrixA[dataType][batch]);
        }

        [TestMethod]
        public void GenerateFixedBatchesSolutionTest()
        {

            // Формируем входные данные
            #region Init input

            /*
            // dataTypesCount:
            // 3
            // 
            // deviceCount:
            // 3
            // 
            // buffer:
            // 3
            // 
            // proccessingTime:
            // +---+---+---+
            // | 1 | 2 | 5 |
            // +---+---+---+
            // | 3 | 4 | 5 |
            // +---+---+---+
            // | 3 | 4 | 5 |
            // +---+---+---+
            //
            // changeoverTime:
            // +---+---+---+---+
            // |   | 1 | 2 | 5 |
            // +   +---+---+---+
            // | 1 | 3 | 4 | 5 |
            // +   +---+---+---+
            // |   | 3 | 4 | 5 |
            // +---+---+---+---+
            // |   | 4 | 2 | 5 |
            // +   +---+---+---+
            // | 2 | 3 | 1 | 5 |
            // +   +---+---+---+
            // |   | 3 | 4 | 5 |
            // +---+---+---+---+
            // |   | 4 | 2 | 5 |
            // +   +---+---+---+
            // | 3 | 3 | 1 | 5 |
            // +   +---+---+---+
            // |   | 3 | 4 | 5 |
            // +---+---+---+---+
            //
            // preMaintenanceTimes
            // +---+---+---+
            // | 3 | 1 | 5 |
            // +---+---+---+
            //
            // isFixedBatches:
            // false
            */

            // Объявляем матрицу переналадки
            Dictionary<int, List<List<int>>> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            List<List<int>> changeoverTime_1 = new()
            {
                    new() { 1, 2, 5 },
                    new() { 3, 4, 5 },
                    new() { 3, 4, 5 },
                };

            // Создаём матрицу переналадки для 2 прибора
            List<List<int>> changeoverTime_2 = new()
            {
                    new() { 4, 2, 5 },
                    new() { 3, 1, 5 },
                    new() { 3, 4, 5 },
                };

            // Создаём матрицу переналадки для 3 прибора
            List<List<int>> changeoverTime_3 = new()
            {
                    new() { 4, 2, 5 },
                    new() { 3, 1, 5 },
                    new() { 3, 4, 5 },
                };

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);
            changeoverTime.Add(2, changeoverTime_3);

            // Создаём матрицу времени выполнения
            List<List<int>> proccessingTime = new()
            {
                    new() { 1, 2, 5 },
                    new() { 3, 4, 5 },
                    new() { 3, 4, 5 },
                };

            // Формируем конфигурационный файл
            Config config = new(
                3, // int dataTypesCount,
                3, // int deviceCount,
                3, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                false// bool isFixedBatches
            );

            #endregion

            var firstLevel = new newAlgorithm.FirstLevel(config, new List<int> { 12, 12, 12 });
            var output = new List<List<int>> {
                new() { 12 },
                new() { 12 },
                new() { 12 }
            };
            firstLevel.GenerateFixedBatchesSolution();

            for (int dataType = 0; dataType < output.Count; dataType++)
                for (int batch = 0; batch < output[dataType].Count; batch++)
                    Assert.AreEqual(output[dataType][batch], firstLevel.PrimeMatrixA[dataType][batch]);
        }
    }
}
