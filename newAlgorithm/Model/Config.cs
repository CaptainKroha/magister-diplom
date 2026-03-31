using newAlgorithm.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace magisterDiplom.Model
{

    /// <summary>
    /// Структура описывает конфигурацию для расчёа расписания с ПТО
    /// </summary>
    public readonly struct PreMConfig
    {

        /// <summary>
        /// Нижняя граница для значения интенсивностей
        /// </summary>
        private const double lowerRate = 0.0;

        /// <summary>
        /// Нижняя граница для значения интенсивностей
        /// </summary>
        private const double upperRate = 1.0;

        /// <summary>
        /// Базовая конфигурационная структура
        /// </summary>
        public readonly Config config;

        /// <summary>
        /// Список из времён времени выполнения ПТО для соответствующих приборов: preMaintenanceTimes = [deviceCount]
        /// </summary>
        public readonly List<int> preMaintenanceTimes;

        /// <summary>
        /// Список интенсивностей отказов для соответствующих приборов: [deviceCount]
        /// </summary>
        public readonly List<double> failureRates;

        /// <summary>
        /// Список интенсивностей восстановлений для соответствующих приборов: [deviceCount]
        /// </summary>
        public readonly List<double> restoringDevice;

        /// <summary>
        /// Нижний порог надёжности
        /// </summary>
        public readonly double beta;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="beta">Нижний порог надёжности</param>
        /// <param name="failureRates">Интенсивность отказов приборов</param>
        /// <param name="restoringDevice">Интенсивность восстановления приборов</param>
        /// <param name="preMaintenanceTimes">Длительности ПТО приборов</param>
        /// <exception cref="ArgumentException">Переданные данные имеют некорректны</exception>
        /// <exception cref="ArgumentNullException">Был передан null</exception>
        /// <exception cref="IndexOutOfRangeException">Размеры переданных данных не совпадают</exception>
        public PreMConfig(
            Config config,
            List<int> preMaintenanceTimes,
            List<double> failureRates,
            List<double> restoringDevice,
            double beta
        ) {

            // Переопределяем конфигурационную структуру
            this.config = config;

            // Если нижний порог выходит за границы
            if (beta < lowerRate || beta > upperRate)
                throw new ArgumentException($"The value of beta must be between {lowerRate} and {upperRate}.");

            // Вектор ПТО равен null
            if (preMaintenanceTimes == null)
                throw new ArgumentNullException("The preMaintenanceTimes list is null.");

            // Размер вектора ПТО не совпадает с количеством приборов
            if (preMaintenanceTimes.Count() != config.deviceCount)
                throw new IndexOutOfRangeException("The number of items in the list preMaintenanceTimes does not match the deviceCount.");

            // Для каждого прибора
            for (int device = 0; device < config.deviceCount; device++)

                // Если элемент ветора preMaintenanceTimes ниже 0
                if (preMaintenanceTimes[device] < 0)
                    throw new ArgumentException("The value in vector preMaintenanceTimes cannot be less than 0");

            // Вектор отказов равен null
            if (failureRates == null)
                throw new ArgumentNullException("The failureRates list is null.");
            
            // Вектор отказов равен null
            if (failureRates.Count() != config.deviceCount)
                throw new IndexOutOfRangeException("The number of items in the list failureRates does not match the deviceCount.");

            // Проверяем, что диапазон данных от 0 до 1
            for (int device = 0; device < config.deviceCount; device++)
                
                // Если данные выходят за диапазон
                if (failureRates[device] < lowerRate || failureRates[device] > upperRate)
                    throw new ArgumentException($"The value of failure rates must be between {lowerRate} and {upperRate}.");

            // Вектор востановления равен null
            if (restoringDevice == null)
                throw new ArgumentNullException("The restoringDevice vector is null.");

            // Вектор отказов равен null
            if (restoringDevice.Count() != config.deviceCount)
                throw new IndexOutOfRangeException("The number of items in the list restoringDevice does not match the deviceCount.");

            // Проверяем, что диапазон данных от 0 до 1
            for (int device = 0; device < config.deviceCount; device++)

                // Если данные выходят за диапазон
                if (restoringDevice[device] < lowerRate || restoringDevice[device] > upperRate)
                    throw new ArgumentException($"The value of restore rates must be between {lowerRate} and {upperRate}.");
            
            // Выполняем присваивание
            this.preMaintenanceTimes = preMaintenanceTimes;
            this.restoringDevice = restoringDevice;
            this.failureRates = failureRates;
            this.beta = beta;
        }

        /// <summary>
        /// Данная функция формируем выходную строку об конфигурационной структуре
        /// </summary>
        /// <param name="prefix">Префикс для формированного вывода</param>
        /// <returns>Результирующая строка со всей необходимой информацией</returns>
        public string ToString(string prefix = "\t")
        {

            // Объявляем индекс прибора
            int device;

            // Результирующая информация
            string res = "";

            // Выполняем формирование строки времени ПТО
            res += prefix + "preMaintenanceTimes: [";
            for (device = 0; device < this.config.deviceCount - 1; device++)
                res += $"{this.preMaintenanceTimes[device]}, ";
            res += $"{this.preMaintenanceTimes[device]}];" + Environment.NewLine;

            // Выполняем формирование строки интенсивности востановления приборов
            res += prefix + "restoringDevice: [";
            for (device = 0; device < this.config.deviceCount - 1; device++)
                res += $"{this.restoringDevice[device]}, ";
            res += $"{this.restoringDevice[device]}];" + Environment.NewLine;

            // Выполняем формирование строки интенсивности отказов приборов
            res += prefix + "failureRates: [";
            for (device = 0; device < this.config.deviceCount - 1; device++)
                res += $"{this.failureRates[device]}, ";
            res += $"{this.failureRates[device]}];" + Environment.NewLine;

            // Выполняем формирование строки нижнего порога надёжности
            res += prefix + $"beta: {beta};" + Environment.NewLine;

            // Возвращяем результат
            return res;
        }
    }
    
    /// <summary>
    /// Структура описывает конфигурацию по которой необходимо выполнить построение расписания
    /// </summary>
    public readonly struct Config
    {

        /// <summary>
        /// Данная переменная устанавливаем режим отладки для всей программы
        /// </summary>
        public readonly static bool isDebug = true;

        /// <summary>
        /// Устанавливаем количество заданий для каждого типа данных
        /// </summary>
        public readonly int batchCount;

        /// <summary>
        /// Данная переменная определяет являются ли партии фиксированными
        /// </summary>
        public readonly bool isFixedBatches;

        /// <summary>
        /// Данная переменная определяет количество типов данных в конвейерной системе
        /// </summary>
        public readonly int dataTypesCount;

        /// <summary>
        /// Данная переменная определяет размер буфера перед приборами
        /// </summary>
        public readonly int buffer;

        /// <summary>
        /// Данная переменная определяет длину конвейера, как количество приборов
        /// </summary>
        public readonly int deviceCount;

        /// <summary>
        /// Данная переменная представляет из себя словарь соответствия прибора к матрице переналадки. 
        /// Для каждого прибора есть матрица переналадки приборов с одного типа задания на другой.
        /// Таким образом changeoverTime = [deviceCount] : [dataTypesCount x dataTypesCount]
        /// </summary>
        public readonly Dictionary<int, List<List<int>>> changeoverTime;

        /// <summary>
        /// Данная переменная представляет из себя двухмерную матрицу и используется, как матрица времени выполнения заданий.
        /// Первое измерение определяется, как количество приборов на конвейере. Второе измерения это количество
        /// типов данных. Таким образом proccessingTime = [deviceCount x dataTypesCount]
        /// </summary>
        public readonly List<List<int>> proccessingTime;

        /// <summary>
        /// Конструктор конфигурационного класса
        /// </summary>
        /// <param name="dataTypesCount">Количество типов данных</param>
        /// <param name="deviceCount">Количество приборов</param>
        /// <param name="buffer">Размер буфера</param>
        /// <param name="proccessingTime">Матрица времени выполнения</param>
        /// <param name="changeoverTime">Матрица времене переналадки</param>
        /// <param name="isFixedBatches">Размеры пакетов фиксированные, если True, иначе False</param>
        /// <exception cref="ArgumentNullException">Один из переданных аргументов имеет Null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Размеры переданных данных не совпадают</exception>
        /// <exception cref="ArgumentException">Один из переданных аргументов неверный</exception>
        public Config(
            int dataTypesCount,
            int deviceCount,
            int buffer,
            List<List<int>> proccessingTime,
            Dictionary<int, List<List<int>>> changeoverTime,
            bool isFixedBatches,
            int batchCount = 12
        ) {

            // Если количество типов данных меньше или равно 0
            if (dataTypesCount <= 0)
                throw new ArgumentException("The value in dataTypesCount cannot be less or equal than 0");

            // Если количество заданий для каждого типа данных меньше или равно 2
            if (batchCount <= 2)
                throw new ArgumentException("The value in batchCount cannot be less or equal than 2");

            // Если количество приборов меньше или равно 0
            if (deviceCount <= 0)
                throw new ArgumentException("The value in deviceCount cannot be less or equal than 0");

            // Если количество приборов меньше 0
            if (buffer < 0)
                throw new ArgumentException("The value in buffer cannot be less than 0");

            // Матрица времени выполнения равна null
            if (proccessingTime == null)
                throw new ArgumentNullException("The proccessingTime matrix is null.");

            // Размер матрицы времени выполнения не совпадает с количеством приборов
            if (proccessingTime.Count != deviceCount) 
                throw new ArgumentOutOfRangeException("The number of items in the list proccessingTime does not match the deviceCount.");

            // Выполняем проверку исключений для матрицы времени выполнения
            for (int device = 0; device < proccessingTime.Count; device++) { 

                // Размер матрицы времени выполнения не совпадает с количеством типов
                if (proccessingTime[device].Count != dataTypesCount)
                    throw new ArgumentOutOfRangeException("The number of items in the list proccessingTime does not match the dataTypesCount.");

                // Выполняем проверку исключений для матрицы времени выполнения
                for (int dataType = 0; dataType < proccessingTime[device].Count; dataType++) { 
                
                    // Если элемент матрицы меньше или равен 0
                    if (proccessingTime[device][dataType] <= 0)
                        throw new ArgumentException("The value in matrix proccessingTime cannot be less or equal than 0");
                }
            }

            // Словарь соответствий времени переналадки равен null
            if (changeoverTime == null)
                throw new ArgumentNullException("The changeoverTime matrix is null.");

            // Размер словаря переналадки не совпадает с количеством приборов
            if (changeoverTime.Count != deviceCount)
                throw new ArgumentOutOfRangeException("The number of items in the Dictionary changeoverTime does not match the deviceCount.");

            // Выполняем проверку для каждого прибора
            for (int device = 0; device < changeoverTime.Count; device++)
            {

                // Размер матрицы по словарю переналадки не совпадает с количеством типов
                if (changeoverTime[device].Count != dataTypesCount)
                    throw new ArgumentOutOfRangeException("The number of items in the Dictionary changeoverTime does not match the dataTypesCount.");

                // Размер вектора матрицы по словарю переналадки не совпадает с количеством типов
                for (int fromDataType = 0; fromDataType < changeoverTime[device].Count; fromDataType++) {

                    // Если размеры не совпадают
                    if (changeoverTime[device][fromDataType].Count != dataTypesCount)
                        throw new ArgumentOutOfRangeException("The number of items in the Dictionary changeoverTime does not match the dataTypesCount.");

                    // Выполняем проверку исключений для матрицы времени выполнения
                    for (int toDataType = 0; toDataType < proccessingTime[device].Count; toDataType++)

                        // Если элемент матрицы меньше 0
                        if (changeoverTime[device][fromDataType][toDataType] < 0)
                            throw new ArgumentException("The value in matrix changeoverTime cannot be less than 0");
                }
            }

            // Выполняем инициализацию
            this.dataTypesCount = dataTypesCount;
            this.deviceCount = deviceCount;
            this.buffer = buffer;
            this.proccessingTime = proccessingTime;
            this.changeoverTime = changeoverTime;
            this.isFixedBatches = isFixedBatches;
            this.batchCount = batchCount;
        }

        /// <summary>
        /// Данная функция выполняем преобразование данных из матричной 3-ёх мерной формы в словарь матриц
        /// </summary>
        /// <param name="changeoverTime">3-ёх мерная матрица</param>
        /// <returns>Словарь соответствий приборов и матриц переналадок</returns>
        public static Dictionary<int, List<List<int>>> ChangeoverTimeConverter(List<List<List<int>>> changeoverTime)
        {

            // Создаём словарь матриц переналадки
            Dictionary<int, List<List<int>>> _changeoverTime = new Dictionary<int, List<List<int>>>();

            // Для каждого прибора выполняем переопределение данных в необходимую форму
            for (int device = 0; device < changeoverTime.Count; device++)
                _changeoverTime.Add(device, changeoverTime[device]);

            // Возвращаем словарь матриц переналадки
            return _changeoverTime;
        }

        /// <summary>
        /// Данная функция формируем выходную строку об конфигурационной структуре
        /// </summary>
        /// <param name="prefix">Префикс для формированного вывода</param>
        /// <returns>Результирующая строка со всей необходимой информацией</returns>
        public string ToString(string prefix = "\t")
        {

            // Результирующая информация
            string res = "";

            // Добавляем информацию о фиксированности пакетов
            res += prefix + $"isFixedBatches: {this.isFixedBatches}" + Environment.NewLine;

            // Добавляем информацию о количестве типов данных
            res += prefix + $"dataTypesCount: {this.dataTypesCount}" + Environment.NewLine;

            // Добавляем информацию о количестве приборов
            res += prefix + $"deviceCount:    {this.deviceCount}" + Environment.NewLine;

            // Добавляем информацию о размере буфера
            res += prefix + $"buffer:         {this.buffer}" + Environment.NewLine;
            
            // Выполняем формирование вывода времени выполнения
            res += prefix + "proccessingTime:" + Environment.NewLine;
            for (int device = 0; device < this.deviceCount; device++)
            {
                int dataType;
                res += prefix + prefix + $"Device {device}: " + prefix;
                for (dataType = 0; dataType < this.dataTypesCount - 1; dataType++)
                    res += $"{this.proccessingTime[device][dataType],-2}, ";
                res += $"{this.proccessingTime[device][dataType]};" + Environment.NewLine;
            }
            res += prefix + "changeoverTime:" + Environment.NewLine;

            // Выполняем формирование вывода времени переналадки
            for (int device = 0; device < this.deviceCount; device++)
            {
                res += prefix + prefix + $"Device {device}: " + Environment.NewLine;
                for (int dataTypeRow = 0; dataTypeRow < this.dataTypesCount; dataTypeRow++)
                {
                    int dataTypeCol;
                    res += prefix + prefix + prefix + $"Type {dataTypeRow}: " + prefix;
                    for (dataTypeCol = 0; dataTypeCol < this.dataTypesCount - 1; dataTypeCol++)
                        res += $"{this.changeoverTime[device][dataTypeRow][dataTypeCol],-2}, ";
                    res += $"{this.changeoverTime[device][dataTypeRow][dataTypeCol]};" + Environment.NewLine;
                }
            }

            // Возвращяем результат
            return res;
        }

        /// <summary>
        /// Вернёт объект данной структуры для отладки.
        /// dataTypesCount = 3;
        /// deviceCount = 2;
        /// buffer = 5;
        /// proccessingTime = [[2, 2, 1], [1, 3, 3]];
        /// changeoverTime = 
        ///     1: [[1, 1, 1], [1, 1, 1], [1, 1, 1]]; 
        ///     2: [[2, 2, 2], [2, 2, 2], [2, 2, 2]];
        /// isFixedBatches = false;
        /// </summary>
        /// <returns>Объект структуры</returns>
        public static Config GetDebugConfig_1()
        {
            return new Config(
                3,
                2,
                5,
                new List<List<int>>()
                {
                    new List<int> { 2, 2, 1 },
                    new List<int> { 1, 3, 3 },
                },
                new Dictionary<int, List<List<int>>>
                {
                    {
                        0,
                        new List<List<int>>()
                        {
                            new List<int>{1, 1, 1},
                            new List<int>{1, 1, 1},
                            new List<int>{1, 1, 1},
                        }
                    },
                    {
                        1,
                        new List<List<int>>()
                        {
                            new List<int>{2, 2, 2},
                            new List<int>{2, 2, 2},
                            new List<int>{2, 2, 2},
                        }
                    },
                },
                false
            );
        }

        public static Config GetDebugConfig_2()
        {
            return new Config(
                3,
                3,
                5,
                new List<List<int>>()
                {
                    new List<int> { 2, 2, 1 },
                    new List<int> { 1, 3, 2 },
                    new List<int> { 1, 2, 3 },
                },
                new Dictionary<int, List<List<int>>>
                {
                    {
                        0,
                        new List<List<int>>()
                        {
                            new List<int>{1, 2, 3},
                            new List<int>{1, 2, 3},
                            new List<int>{1, 2, 3},
                        }
                    },
                    {
                        1,
                        new List<List<int>>()
                        {
                            new List<int>{1, 2, 3},
                            new List<int>{1, 2, 3},
                            new List<int>{1, 2, 3},
                        }
                    },
                    {
                        2,
                        new List<List<int>>()
                        {
                            new List<int>{1, 2, 3},
                            new List<int>{1, 2, 3},
                            new List<int>{1, 2, 3},
                        }
                    },
                },
                false
            );
        }
    }
}
