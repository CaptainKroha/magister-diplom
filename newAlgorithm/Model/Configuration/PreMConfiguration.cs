using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace magisterDiplom.Model.Configuration
{
    public class PreMConfiguration : Configuration
    {
        /// <summary>
        /// Нижняя граница для значения интенсивностей
        /// </summary>
        public const double lowerRate = 0.0;

        /// <summary>
        /// Нижняя граница для значения интенсивностей
        /// </summary>
        public const double upperRate = 1.0;

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
        /// Конструктор по умолчанию
        /// </summary>
        public PreMConfiguration() { }

        public PreMConfiguration(PreMConfig config) : base(config.config)
        {
            beta = config.beta;
            preMaintenanceTimes = config.preMaintenanceTimes;
            failureRates = config.failureRates;
            restoringDevice = config.restoringDevice;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="dataTypesCount">Количество типов данных</param>
        /// <param name="deviceCount">Количество приборов</param>
        /// <param name="proccessingTime">Матрица времени выполнения</param>
        /// <param name="changeoverTime">Матрица времене переналадки</param>
        /// <param name="isFixedBatches">Размеры пакетов фиксированные, если True, иначе False</param>
        /// <param name="beta">Нижний порог надёжности</param>
        /// <param name="failureRates">Интенсивность отказов приборов</param>
        /// <param name="restoringDevice">Интенсивность восстановления приборов</param>
        /// <param name="preMaintenanceTimes">Длительности ПТО приборов</param>
        /// <exception cref="ArgumentException">Переданные данные имеют некорректны</exception>
        /// <exception cref="ArgumentNullException">Был передан null</exception>
        /// <exception cref="IndexOutOfRangeException">Размеры переданных данных не совпадают</exception>
        public PreMConfiguration(
            int dataTypesCount,
            int batchCount,
            int deviceCount,
            List<List<int>> proccessingTime,
            Dictionary<int, List<List<int>>> changeoverTime,
            bool isFixedBatches,
            List<int> preMaintenanceTimes,
            List<double> failureRates,
            List<double> restoringDevice,
            double beta
        ) : base(dataTypesCount, batchCount, deviceCount, proccessingTime, changeoverTime, isFixedBatches)
        { 
            // Если нижний порог выходит за границы
            if (beta < lowerRate || beta > upperRate)
                throw new ArgumentException($"The value of beta must be between {lowerRate} and {upperRate}.");

            // Вектор ПТО равен null
            if (preMaintenanceTimes == null)
                throw new ArgumentNullException("The preMaintenanceTimes list is null.");

            // Размер вектора ПТО не совпадает с количеством приборов
            if (preMaintenanceTimes.Count() != this.deviceCount)
                throw new IndexOutOfRangeException("The number of items in the list preMaintenanceTimes does not match the deviceCount.");

            // Для каждого прибора
            for (int device = 0; device < this.deviceCount; device++)

                // Если элемент ветора preMaintenanceTimes ниже 0
                if (preMaintenanceTimes[device] < 0)
                    throw new ArgumentException("The value in vector preMaintenanceTimes cannot be less than 0");

            // Вектор отказов равен null
            if (failureRates == null)
                throw new ArgumentNullException("The failureRates list is null.");

            // Вектор отказов равен null
            if (failureRates.Count() != this.deviceCount)
                throw new IndexOutOfRangeException("The number of items in the list failureRates does not match the deviceCount.");

            // Проверяем, что диапазон данных от 0 до 1
            for (int device = 0; device < this.deviceCount; device++)

                // Если данные выходят за диапазон
                if (failureRates[device] < lowerRate || failureRates[device] > upperRate)
                    throw new ArgumentException($"The value of failure rates must be between {lowerRate} and {upperRate}.");

            // Вектор востановления равен null
            if (restoringDevice == null)
                throw new ArgumentNullException("The restoringDevice vector is null.");

            // Вектор отказов равен null
            if (restoringDevice.Count() != this.deviceCount)
                throw new IndexOutOfRangeException("The number of items in the list restoringDevice does not match the deviceCount.");

            // Проверяем, что диапазон данных от 0 до 1
            for (int device = 0; device < this.deviceCount; device++)

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
        public new string ToString(string prefix = "\t")
        {

            string res = base.ToString(prefix);

            // Объявляем индекс прибора
            int device;

            // Выполняем формирование строки времени ПТО
            res += prefix + "preMaintenanceTimes: [";
            for (device = 0; device < this.deviceCount - 1; device++)
                res += $"{this.preMaintenanceTimes[device]}, ";
            res += $"{this.preMaintenanceTimes[device]}];" + Environment.NewLine;

            // Выполняем формирование строки интенсивности востановления приборов
            res += prefix + "restoringDevice: [";
            for (device = 0; device < this.deviceCount - 1; device++)
                res += $"{this.restoringDevice[device]}, ";
            res += $"{this.restoringDevice[device]}];" + Environment.NewLine;

            // Выполняем формирование строки интенсивности отказов приборов
            res += prefix + "failureRates: [";
            for (device = 0; device < this.deviceCount - 1; device++)
                res += $"{this.failureRates[device]}, ";
            res += $"{this.failureRates[device]}];" + Environment.NewLine;

            // Выполняем формирование строки нижнего порога надёжности
            res += prefix + $"beta: {beta};" + Environment.NewLine;

            // Возвращяем результат
            return res;
        }
    }
}
