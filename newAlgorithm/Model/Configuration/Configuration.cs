using newAlgorithm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace magisterDiplom.Model.Configuration
{
    public class Configuration
    {
        /// <summary>
        /// Данная переменная устанавливаем режим отладки для всей программы
        /// </summary>
        public static bool isDebug = true;

        /// <summary>
        /// Устанавливаем количество заданий для каждого типа данных
        /// </summary>
        public int batchCount;

        /// <summary>
        /// Данная переменная определяет являются ли партии фиксированными
        /// </summary>
        public bool isFixedBatches;

        /// <summary>
        /// Данная переменная определяет количество типов данных в конвейерной системе
        /// </summary>
        public int dataTypesCount;

        /// <summary>
        /// Данная переменная определяет длину конвейера, как количество приборов
        /// </summary>
        public int deviceCount;

        /// <summary>
        /// Данная переменная представляет из себя словарь соответствия прибора к матрице переналадки. 
        /// Для каждого прибора есть матрица переналадки приборов с одного типа задания на другой.
        /// Таким образом changeoverTime = [deviceCount] : [dataTypesCount x dataTypesCount]
        /// </summary>
        public Dictionary<int, int[,]> changeoverTime = new Dictionary<int, int[,]> { };

        /// <summary>
        /// Данная переменная представляет из себя двухмерную матрицу и используется, как матрица времени выполнения заданий.
        /// Первое измерение определяется, как количество приборов на конвейере. Второе измерения это количество
        /// типов данных. Таким образом proccessingTime = [deviceCount x dataTypesCount]
        /// </summary>
        public int[,] proccessingTime;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Configuration() { }

        public Configuration(Config config)
        {
            dataTypesCount = config.dataTypesCount;
            deviceCount = config.deviceCount;
            batchCount = config.batchCount;
            proccessingTime = ListListToArray(config.proccessingTime);
            for (int i = 0; i < deviceCount; ++i)
            {
                changeoverTime.Add(i, ListListToArray(config.changeoverTime[i]));
            }
            isFixedBatches = config.isFixedBatches;
        }

        /// <summary>
        /// Конструктор конфигурационного класса
        /// </summary>
        /// <param name="dataTypesCount">Количество типов данных</param>
        /// <param name="deviceCount">Количество приборов</param>
        /// <param name="proccessingTime">Матрица времени выполнения</param>
        /// <param name="changeoverTime">Матрица времене переналадки</param>
        /// <param name="isFixedBatches">Размеры пакетов фиксированные, если True, иначе False</param>
        /// <exception cref="ArgumentNullException">Один из переданных аргументов имеет Null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Размеры переданных данных не совпадают</exception>
        /// <exception cref="ArgumentException">Один из переданных аргументов неверный</exception>
        public Configuration(
            int dataTypesCount,
            int batchCount,
            int deviceCount,
            List<List<int>> proccessingTime,
            Dictionary<int, List<List<int>>> changeoverTime,
            bool isFixedBatches
        )
        {

            // Если количество типов данных меньше или равно 0
            if (dataTypesCount <= 0)
                throw new ArgumentException("The value in dataTypesCount cannot be less or equal than 0");

            // Если количество заданий для каждого типа данных меньше или равно 2
            if (batchCount <= 2)
                throw new ArgumentException("The value in batchCount cannot be less or equal than 2");

            // Если количество приборов меньше или равно 0
            if (deviceCount <= 0)
                throw new ArgumentException("The value in deviceCount cannot be less or equal than 0");

            // Матрица времени выполнения равна null
            if (proccessingTime == null)
                throw new ArgumentNullException("The proccessingTime matrix is null.");

            // Размер матрицы времени выполнения не совпадает с количеством приборов
            if (proccessingTime.Count != deviceCount)
                throw new ArgumentOutOfRangeException("The number of items in the list proccessingTime does not match the deviceCount.");

            // Выполняем проверку исключений для матрицы времени выполнения
            for (int device = 0; device < proccessingTime.Count; device++)
            {

                // Размер матрицы времени выполнения не совпадает с количеством типов
                if (proccessingTime[device].Count != dataTypesCount)
                    throw new ArgumentOutOfRangeException("The number of items in the list proccessingTime does not match the dataTypesCount.");

                // Выполняем проверку исключений для матрицы времени выполнения
                for (int dataType = 0; dataType < proccessingTime[device].Count; dataType++)
                {

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
                for (int fromDataType = 0; fromDataType < changeoverTime[device].Count; fromDataType++)
                {

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
            this.proccessingTime = ListListToArray(proccessingTime);
            for(int i = 0; i < deviceCount; ++i)
            {
                this.changeoverTime.Add(i, ListListToArray(changeoverTime[i]));
            }
            
            this.isFixedBatches = isFixedBatches;
            this.batchCount = batchCount;
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
            res += prefix + $"isFixedBatches: {isFixedBatches}" + Environment.NewLine;

            // Добавляем информацию о количестве типов данных
            res += prefix + $"dataTypesCount: {dataTypesCount}" + Environment.NewLine;

            // Добавляем информацию о количестве приборов
            res += prefix + $"deviceCount:    {deviceCount}" + Environment.NewLine;

            // Выполняем формирование вывода времени выполнения
            res += prefix + "proccessingTime:" + Environment.NewLine;
            for (int device = 0; device < deviceCount; device++)
            {
                int dataType;
                res += prefix + prefix + $"Device {device}: " + prefix;
                for (dataType = 0; dataType < dataTypesCount - 1; dataType++)
                    res += $"{proccessingTime[device,dataType],-2}, ";
                res += $"{proccessingTime[device, dataType]};" + Environment.NewLine;
            }
            res += prefix + "changeoverTime:" + Environment.NewLine;

            // Выполняем формирование вывода времени переналадки
            for (int device = 0; device < deviceCount; device++)
            {
                res += prefix + prefix + $"Device {device}: " + Environment.NewLine;
                for (int dataTypeRow = 0; dataTypeRow < dataTypesCount; dataTypeRow++)
                {
                    int dataTypeCol;
                    res += prefix + prefix + prefix + $"Type {dataTypeRow}: " + prefix;
                    for (dataTypeCol = 0; dataTypeCol < dataTypesCount - 1; dataTypeCol++)
                        res += $"{changeoverTime[device][dataTypeRow, dataTypeCol],-2}, ";
                    res += $"{changeoverTime[device][dataTypeRow, dataTypeCol]};" + Environment.NewLine;
                }
            }

            return res;
        }

        protected int[,] ListListToArray(List<List<int>> data)
        {
            int rows = data.Count;
            if(rows == 0) return null;
            int columns = data[0].Count;
            var result = new int[rows, columns];
            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < columns; j++)
                {
                    result[i, j] = data[i][j];
                }
            }
            return result;
        }
    }
}
