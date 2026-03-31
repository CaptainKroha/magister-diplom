using magisterDiplom.Model;
using newAlgorithm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTest.Model
{

    /*[TestClass]
    public class ConfigTest
    {

        /// <summary>
        /// Данная функция обкладывает функцию ToString() структуры Config тестом
        /// </summary>
        [TestMethod]
        public void ConfigStringTest()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Формируем конфигурационный файл
            Config config = new(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 3, 1 }),
                new Vector(new List<int> { 3, 1 }),
                new Vector(new List<int> { 3, 1 }),
                false// bool isFixedBatches
            );

            #endregion

            // Формируем выходные данные
            string output = ""+
                "\tisFixedBatches: False" +         Environment.NewLine +
                "\tdataTypesCount: 2"+              Environment.NewLine +
                "\tdeviceCount:    2"+              Environment.NewLine +
                "\tbuffer:         999" +           Environment.NewLine +
                "\tpreMaintenanceTimes: 3 , 1;" +   Environment.NewLine +
                "\trestoringDevice: 3 , 1;" +        Environment.NewLine +
                "\tfailureRates: 3 , 1;" +           Environment.NewLine +
                "\tproccessingTime:" +              Environment.NewLine +
                "\t\tDevice 0: \t1 , 2;" +          Environment.NewLine +
                "\t\tDevice 1: \t3 , 4;" +          Environment.NewLine +
                "\tchangeoverTime:" +               Environment.NewLine +
                "\t\tDevice 0: " +                  Environment.NewLine +
                "\t\t\tType 0: \t1 , 2;" +          Environment.NewLine +
                "\t\t\tType 1: \t3 , 4;" +          Environment.NewLine +
                "\t\tDevice 1: " +                  Environment.NewLine +
                "\t\t\tType 0: \t4 , 2;" +          Environment.NewLine +
                "\t\t\tType 1: \t3 , 1;" +          Environment.NewLine +
                "";

            // Выполняем сравнение
            Assert.AreEqual( output, config.ToString("\t") );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitConfig_PreMaintenanceTimes_ArgumentNullException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                null,
                new Vector(new List<int> { 3, 1 }),
                new Vector(new List<int> { 3, 1 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitConfig_PreMaintenanceTimes_ArgumentException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                3, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2, 3 }),
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitConfig_RestoringDevice_ArgumentNullException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 3, 1 }),
                new Vector(new List<int> { 3, 1 }),
                null,
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitConfig_RestoringDevice_ArgumentException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            #endregion


            // Формируем конфигурационный файл
            new Config(
                3, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2, 3 }),
                false// bool isFixedBatches
            );
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitConfig_FailureRates_ArgumentNullException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 3, 1 }),
                null,
                new Vector(new List<int> { 3, 1 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitConfig_FailureRates_ArgumentException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                3, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2, 3 }),
                new Vector(new List<int> { 1, 2 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitConfig_ProccessingTime_ArgumentNullException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);
            #endregion

            // Формируем конфигурационный файл
            new Config(
                3, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                null, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 3, 1 }),
                new Vector(new List<int> { 3, 1 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitConfig_ProccessingTime_ArgumentException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                3, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2, 4 }),
                new Vector(new List<int> { 1, 2, 4 }),
                new Vector(new List<int> { 1, 2, 4 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitConfig_ProccessingTime_ArgumentException_2()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 1, 2, 3 },
                    new() { 3, 4 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitConfig_ProccessingTime_ArgumentException_3()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 3, 4 },
                    new() { 1, 2, 3 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitConfig_ChangeoverTime_ArgumentNullException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 3, 4 },
                    new() { 1, 2 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                null,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitConfig_ChangeoverTime_ArgumentException()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);
            #pragma warning disable CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.
            changeoverTime.Add(2, null);
            #pragma warning restore CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 3, 4 },
                    new() { 1, 2 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitConfig_ChangeoverTime_ArgumentException_2()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4 },
                    new() { 3, 4 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 3, 4 },
                    new() { 1, 2 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                false// bool isFixedBatches
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitConfig_ChangeoverTime_ArgumentException_3()
        {

            // Формируем входные данные
            #region Input

            // Объявляем матрицу переналадки
            Dictionary<int, Matrix> changeoverTime = new();

            // Создаём матрицу переналадки для 1 прибора
            Matrix changeoverTime_1 = new(new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 3, 4, 5 },
                });

            // Создаём матрицу переналадки для 2 прибора
            Matrix changeoverTime_2 = new(new List<List<int>>
                {
                    new() { 4, 2 },
                    new() { 3, 1 },
                });

            // Добавляем матрицы переналадки в changeoverTime
            changeoverTime.Add(0, changeoverTime_1);
            changeoverTime.Add(1, changeoverTime_2);

            // Создаём матрицу времени выполнения
            Matrix proccessingTime = new(new List<List<int>>
                {
                    new() { 3, 4 },
                    new() { 1, 2 },
                });

            #endregion

            // Формируем конфигурационный файл
            new Config(
                2, // int dataTypesCount,
                2, // int deviceCount,
                999, // int buffer,
                proccessingTime, // Matrix proccessingTime,
                changeoverTime,// Dictionary<int, Matrix> changeoverTime,
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                new Vector(new List<int> { 1, 2 }),
                false// bool isFixedBatches
            );
        }
    }*/
}
