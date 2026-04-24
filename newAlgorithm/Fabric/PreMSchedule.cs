using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using magisterDiplom.Utils;
using newAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using Batch = magisterDiplom.Model.Batch;

namespace magisterDiplom.Fabric
{
    /// <summary>
    /// Абстрактный класс содержащий общие функции для расписания с ПТО, наследуемый от класса Schedule
    /// </summary>
    public abstract class PreMSchedule : Schedule
    {

        /// <summary>
        /// Конфигурационная структура содержащая информацию о ПТО
        /// </summary>
        private protected readonly new PreMConfiguration config;

        protected PreMSchedule(PreMConfiguration configuration) : base(configuration)
        {
            config = configuration;
        }

        /// <summary>
        /// Матрица моментов времени окончания ПТО приборов
        /// </summary>
        protected List<List<PreMSet>> matrixTPM = new List<List<PreMSet>>();

        /// <summary>
        /// Возвращает матрицу моментов времени окончания ПТО приборов
        /// </summary>
        /// <returns>Матрица моментов времени окончания ПТО приборов</returns>
        public List<List<int>> GetMatrixTPM()
        {
            CalcMatrixTPM();

            // Объявляем матрицу
            List<List<int>> res = new List<List<int>>(matrixTPM.Count);

            // Инициализируем матрицу
            for (int device = 0; device < config.deviceCount; device++)
            {

                // Инициализируем строки матрицы
                res.Add(new List<int>(matrixTPM[device].Count));

                // Для каждого элемента матрицы matrixTPM
                for (int batchIndex = 0; batchIndex < schedule.Count; batchIndex++)

                    // Устанавливаем в 0
                    res[device].Add(0);

                // Для каждого элемента матрицы matrixTPM
                for (int batchIndex = 0; batchIndex < matrixTPM[device].Count; batchIndex++)

                    // Инициализируем столбцы матрицы
                    res[device][matrixTPM[device][batchIndex].BatchIndex] = matrixTPM[device][batchIndex].TimePreM;
            }

            // Возвращаем результат
            return res;
        }

        public override void Add(int dataType, int size)
        {
            schedule.Add(new Batch(dataType, size));
            AddColumnY();

            SearchByPosition(5);

            AddNewPreMaintence();
        }

        public override void Update(int batchesCount)
        {
            schedule = new List<Batch>(batchesCount);
        }

        protected abstract void AddColumnY();

        protected abstract void AddNewPreMaintence();

        protected abstract bool HasPreMaintenceAfter(int device, int packet);
        protected abstract int PreMaintenceStatusAfter(int device, int packet);

        protected override void CalcStartProcessing()
        {

            startProcessing.Clear();

            // Инициалиизруем матрицу заданий в пакете
            List<List<int>> times = new List<List<int>>();
            for (int batch = 0; batch < schedule.Count(); batch++)
                times.Add(ListUtils.InitVectorInt(schedule[batch].Size));

            for (int device = 0; device < config.deviceCount; device++)
                startProcessing.Add(device, ListUtils.MatrixIntDeepCopy(times));

            CalcStartProcessingFirstDevice();

            for (int device = 1; device < config.deviceCount; device++)
            {
                CalcStartProcessingNDevice(device);
            }
        }

        private void CalcStartProcessingFirstDevice()
        {
            int device = 0, batch = 0, job = 0;

            // Устанавливаем момент начала времени выполнения 1 задания в 1 пакете на 1 приборе, как наладку
            startProcessing[device][batch][job] = config.changeoverTime[device][schedule[batch].Type, schedule[batch].Type];

            // Пробегаемся по всем заданиям пакета в первой позиции
            for (job = 1; job < schedule[batch].Size; job++)

                // Устанавливаем момент начала времени выполнения задания job
                startProcessing[device][batch][job] =

                    // Момент начала времени выполнения предыдущего задания
                    startProcessing[device][batch][job - 1] +

                    // Время выполнения предыдущего задания
                    config.proccessingTime[device, schedule[batch].Type];

            // Пробегаемся по всем возможным позициям cо второго пакета
            for (batch = 1; batch < schedule.Count(); batch++)
            {

                job = 0;

                // Момент начала времени выполнения 1 задания в пакете на позиции batch
                startProcessing[device][batch][job] =

                    // Момент начала времени выполнения последнего задания в предыдущем пакете
                    startProcessing[device][batch - 1].Last() +

                    // Время выполнения задания в предыдущем пакете
                    config.proccessingTime[device, schedule[batch - 1].Type] +

                    // Время переналадки с предыдущего типа на текущий
                    config.changeoverTime[device][schedule[batch - 1].Type, schedule[batch].Type] +

                    // Время выполнения ПТО после предыдущего ПЗ
                    PreMaintenceStatusAfter(device, batch - 1) * config.preMaintenanceTimes[0];

                for (job = 1; job < schedule[batch].Size; job++)

                    // Вычисляем момент начала времени выполнения задания job в позиции batch на 1 приборе
                    startProcessing[device][batch][job] =

                        // Момент начала времени выполнения предыдущего задания
                        startProcessing[device][batch][job - 1] +

                        // Время выполнения предыдущего задания
                        config.proccessingTime[device, schedule[batch].Type];
            }
        }

        private void CalcStartProcessingNDevice(int device)
        {
            int batch = 0, job = 0;

            // Устанавливаем момент начала времени выполнения 1 задания в 1 пакете на приборе device, как
            // Максимум, между временем наладки прибора на выполнение 1 задания в 1 пакете
            // и временем окончания выполнения 1 задания в 1 пакете на предыдущем приборе
            startProcessing[device][batch][job] = Math.Max(

                // Время наладки прибора на выполнение 1 задания в 1 пакете
                config.changeoverTime[device][schedule[batch].Type, schedule[batch].Type],

                // Время окончания выполнения 1 задания в 1 пакете на предыдущем приборе
                startProcessing[device - 1][batch][job] + config.proccessingTime[device - 1, schedule[batch].Type]
            );

            // Пробегаемся по всем возможным заданиям пакета в позиции batchIndex
            for (job = 1; job < schedule[batch].Size; job++)

                // Устанавливаем момент начала времени выполнения текущего задания job, как
                // Максимум, между временем окончания предыдущего задания на текущем приборе и
                // временем окончания текущего задания на предыдущем приборе
                startProcessing[device][batch][job] = Math.Max(

                    // Момент начала времени выполнения предыдущего задания
                    startProcessing[device][batch][job - 1] +

                    // Время выполнения предыдущего задания
                    config.proccessingTime[device, schedule[batch].Type],

                    // Момент начала времени выполнения текущего задания на предыдущем приборе
                    startProcessing[device - 1][batch][job] +

                    // Время выполнения текущего задания на предыдущем приборе
                    config.proccessingTime[device - 1, schedule[batch].Type]
                );

            // Пробегаемся по всем возможным позициям пакетов
            for (batch = 1; batch < schedule.Count(); batch++)
            {

                // Инициализируем индекс задания
                job = 0;

                // Устанавливаем момент начала времени выполнения 1 задания в пакете batchIndex на приборе device,
                // как Максимум, между временем окончания выполнения последнего задания в предыдущем пакете вместе с переналадкой и ПТО
                // и временем окончания выполнения 1 задания в пакете на в batchIndex на предыдущем приборе
                startProcessing[device][batch][job] = Math.Max(

                    // Момент начала времени выполнения последнего задания в предыдущем ПЗ
                    startProcessing[device][batch - 1].Last() +

                    // Время выполнения последнего задания в предыдущем ПЗ
                    config.proccessingTime[device, schedule[batch - 1].Type] +

                    // Время переналадки с предыдущего типа на текущий
                    config.changeoverTime[device][schedule[batch - 1].Type, schedule[batch].Type] +

                    // Время выполнения ПТО
                    PreMaintenceStatusAfter(device, batch - 1) * config.preMaintenanceTimes[device],

                    // Момент начала времени выполнения 1 задания на предыдущем приборе
                    startProcessing[device - 1][batch][job] +

                    // Время выполнения 1 задания на предыдущем приборе
                    config.proccessingTime[device - 1, schedule[batch].Type]);

                // Пробегаемся по всем возможным заданиям пакета в позиции batchIndex
                for (job = 1; job < schedule[batch].Size; job++)

                    // Устанавливаем момент начала времени выполнения текущего задания job, как
                    // Максимум, между временем окончания предыдущего задания на текущем приборе и
                    // временем окончания текущего задания на предыдущем приборе
                    startProcessing[device][batch][job] = Math.Max(

                        // Момент начала времени выполнения предыдущего задания
                        startProcessing[device][batch][job - 1] +

                        // Время выполнения предыдущего задания
                        config.proccessingTime[device, schedule[batch].Type],

                        // Момент начала времени выполнения задания на предыдущем приборе
                        startProcessing[device - 1][batch][job] +

                        // Время выполнения задания на предыдущем приборе
                        config.proccessingTime[device - 1, schedule[batch].Type]
                    );
            }
        }

        /// <summary>
        /// Выполняет построение матрицы моментов окончания времени выполнения ПТО.
        /// </summary>
        protected void CalcMatrixTPM()
        {

            // Если установлено логирование и объект для записи существует
            //if (Form1.loggingOn)

            //    // Записываем данные в файл
            //    fstream.Write("Вычисляем матрицу T^pm;");

            matrixTPM?.Clear();

            for (int device = 0; device < config.deviceCount; device++)
            {

                // Инициализируем ПТО для прибора
                matrixTPM.Add(new List<PreMSet>());

                for (int batchIndex = 0; batchIndex < schedule.Count; batchIndex++)

                    // Если для текущей позиции есть ПТО
                    if (HasPreMaintenceAfter(device,batchIndex))

                        // Момент окончания времени выполнения ПТО на позиции batchIndex
                        matrixTPM[device].Add(

                            // Добавляем структуры данных
                            new PreMSet(

                                // Индекс ПЗ после которого будет выполняться ПТО
                                batchIndex,

                                // Момент начала времени выполнения последнего задания в пакете batchIndex на приборе device
                                startProcessing[device][batchIndex].Last() +

                                // Время выполнения задания с типов пакета на позиции batchIndex
                                config.proccessingTime[device, schedule[batchIndex].Type] +

                                // Время выполнения ПТО
                                config.preMaintenanceTimes[device]
                            )
                        );
            }
        }

        public abstract int CalculateCriteria_f2();

        /// <summary>
        /// Данная функция выполняет локальную оптимизацию составов ПЗ
        /// </summary>
        /// <param name="swapCount">Количество перестановок</param>
        /// <returns>true, если была найдено перестановка удовлетворяющая условию надёжности. Иначе false</returns>
        protected bool SearchByPosition(int swapCount = 999999)
        {

            List<Batch> bestSchedule = new List<Batch>(schedule);

            CalcStartProcessing();
            CalcMatrixTPM();

            int bestValue = CalculateCriteria_f2();

            for (int batch = schedule.Count - 1; batch > 0 && swapCount > 0; batch--, swapCount--)
            {

                // Выполняем перестановку
                (schedule[batch - 1], schedule[batch]) = (schedule[batch], schedule[batch - 1]);

                CalcStartProcessing();
                CalcMatrixTPM();

                int newValue = CalculateCriteria_f2();

                if (newValue < bestValue)
                {
                    // Переопределяем лучшее расписание
                    bestSchedule = new List<Batch>(schedule);
                    bestValue = newValue;
                }
            }

            schedule = bestSchedule;
            return true;
        }

    }
}