using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using magisterDiplom.Utils;
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
        /// Выполняет построение матрицы моментов окончания времени выполнения ПТО.
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

                for (int batch = 0; batch < ScheduleSize(); batch++)

                    // Если для текущей позиции есть ПТО
                    if (HasPreMaintenceAfter(device, batch))

                        // Момент окончания времени выполнения ПТО на позиции batchIndex
                        matrixTPM[device].Add(
                            new PreMSet(
                                batch,
                                PreMaintenceCompletionTimeAfter(device, batch)
                            )
                        );
            }
        }

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
                for (int batch = 0; batch < ScheduleSize(); batch++)

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

        protected override void Calculate()
        {
            base.Calculate();
            CalcMatrixTPM();
        }

        public override void Add(int dataType, int size)
        {
            schedule.Add(new Batch(dataType, size));
            AddColumnY();
            OptimizeLocaly(5);
            if (schedule.Capacity != ScheduleSize())
            {
                AddPreMaintenceAfterLastBatch();
            }  
        }

        protected bool SolutionUnacceptable()
        {
            for (int device = 0; device < config.deviceCount; device++)
            {   
                for (int batch = 0; batch < ScheduleSize(); batch++)
                {
                    int time = startProcessing[device][batch].Last() + config.proccessingTime[device, BatchType(batch)];

                    // Проверяем ограничение надёжности
                    if (SystemReliabilityBy(time) < config.beta)
                    {
                        return true;
                    }
                }
            }
                    

            return false;
        }

        public override void Update(int batchesCount)
        {
            schedule = new List<Batch>(batchesCount);
        }

        protected override void CalcStartProcessing()
        {

            startProcessing.Clear();

            // Инициалиизруем матрицу заданий в пакете
            List<List<int>> times = new List<List<int>>();
            for (int batch = 0; batch < ScheduleSize(); batch++)
                times.Add(ListUtils.InitVectorInt(schedule[batch].Size));

            for (int device = 0; device < config.deviceCount; device++)
                startProcessing.Add(device, ListUtils.MatrixIntDeepCopy(times));

            CalcStartProcessingFirstDevice();

            for (int device = 1; device < config.deviceCount; device++)
            {
                CalcStartProcessingNDevice(device);
            }
        }

        protected override int F2_criteria()
        {
            return PreMaintenceDuration() + TotalInactionDuration();
        }

        protected double SystemReliabilityBy(int time)
        {
            double result = 1;
            for(int device = 0; device < config.deviceCount - 1; device++)
            {
                result *= DeviceReliabilityBy(device, time);
            }
            return result;
        }

        #region Для переопределения

        protected abstract void AddColumnY();

        protected abstract void AddPreMaintenceAfterLastBatch();

        protected abstract bool HasPreMaintenceAfter(int device, int packet);

        protected abstract int PreMaintenceStatusAfter(int device, int packet);

        #endregion

        #region Служебные методы

        private void CalcStartProcessingFirstDevice()
        {
            int device = 0, batch = 0, job = 0;

            // Устанавливаем момент начала времени выполнения 1 задания в 1 пакете на 1 приборе, как наладку
            startProcessing[device][batch][job] = config.changeoverTime[device][schedule[batch].Type, schedule[batch].Type];

            for (job = 1; job < schedule[batch].Size; job++)
            {
                startProcessing[device][batch][job] = JobCompletionTime(device, batch, job - 1);
            }
                

            // Пробегаемся по всем возможным позициям cо второго пакета
            for (batch = 1; batch < ScheduleSize(); batch++)
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
                { 
                    startProcessing[device][batch][job] = JobCompletionTime(device, batch, job - 1); 
                }
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
                JobCompletionTime(device - 1, batch, job)
            );

            // Пробегаемся по всем возможным заданиям пакета в позиции batchIndex
            for (job = 1; job < schedule[batch].Size; job++)

                // Устанавливаем момент начала времени выполнения текущего задания job, как
                // Максимум, между временем окончания предыдущего задания на текущем приборе и
                // временем окончания текущего задания на предыдущем приборе
                startProcessing[device][batch][job] = Math.Max(
                    JobCompletionTime(device, batch, job - 1),
                    JobCompletionTime(device - 1, batch, job)
                );

            // Пробегаемся по всем возможным позициям пакетов
            for (batch = 1; batch < ScheduleSize(); batch++)
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
                        JobCompletionTime(device, batch, job - 1),
                        JobCompletionTime(device - 1, batch, job)
                    );
            }
        }

        protected int PreMaintenceDuration()
        {
            var result = 0;
            for (int device = 0; device < config.deviceCount; device++)
                for (int batch = 0; batch < ScheduleSize(); batch++)
                    result += PreMaintenceStatusAfter(device, batch) * config.preMaintenanceTimes[device];
            return result;
        }

        protected int TotalInactionDuration()
        {
            var result = 0;
            for (int device = 0; device < config.deviceCount; device++)
            {
                result += DeviceInactionDuration(device);
            }
            return result;
        }

        protected int DeviceInactionDuration(int device)
        {
            // Ожидание первого задания
            int result = startProcessing[device].First().First();

            result += DeviceInactionDurationBetweenBatches(device);

            for (int batch = 0; batch < ScheduleSize(); ++batch)
            {
                result += DeviceInactionDurationBetweenJobsInBatch(device, batch);
            }

            return result;
        }

        protected int DeviceInactionDurationBetweenBatches(int device)
        {
            var result = 0;

            for (int batch = 1; batch < ScheduleSize(); ++batch)
            {
                result += startProcessing[device][batch].First()
                    - (startProcessing[device][batch - 1].Last() + config.proccessingTime[device, schedule[batch - 1].Type]);
            }

            return result;
        }

        protected int DeviceInactionDurationBetweenJobsInBatch(int device, int batch)
        {
            var result = 0;

            for (int job = 1; job < schedule[batch].Size; ++job)
            {
                result += startProcessing[device][batch][job]
                    - (startProcessing[device][batch][job - 1] + config.proccessingTime[device, schedule[batch].Type]);
            }

            return result;
        }

        protected double DeviceReliabilityBy(int device, int time)
        {
            int deviceActivityTime = DeviceActivityTimeBy(device, time);
            return (deviceActivityTime == 0) ? 1 :
                config.restoringDevice[device] / (config.failureRates[device] + config.restoringDevice[device]) +
                config.failureRates[device] / (config.failureRates[device] + config.restoringDevice[device]) *
                Math.Exp(-1.0 * (config.failureRates[device] + config.restoringDevice[device]) * deviceActivityTime);
        }

        protected int DeviceActivityTimeBy(int device, int time)
        {
            int batchBeforeLastPreMaintence = -1;
            foreach(var preMaintence in matrixTPM[device])
            {
                if(preMaintence.TimePreM > time)
                {
                    break;
                }

                batchBeforeLastPreMaintence = preMaintence.BatchIndex;
            }

            int activityTime = 0;
            for (int batch = batchBeforeLastPreMaintence + 1; batch < schedule.Count; batch++)
            {
                for (int job = 0; job < schedule[batch].Size; job++)
                {

                    // Если момент начала времени выполнения выходит за границу
                    if (startProcessing[device][batch][job] >= time)

                        // Вернём время активности
                        return activityTime;

                    // Высчитываем время выполнения
                    int proc_time = config.proccessingTime[device, schedule[batch].Type];

                    // Высчитываем момент начала времени выполнения
                    int start_time = startProcessing[device][batch][job];

                    // Если момент окончания задания выходит за указанные границы
                    if (start_time + proc_time > time)
                    {
                        activityTime += time - start_time;
                        return activityTime;
                    }

                    activityTime += proc_time;
                }
            }

            return activityTime;
        }

        protected int PreMaintenceCompletionTimeAfter(int device, int batch)
        {
            // Момент начала времени выполнения последнего задания в пакете batchIndex на приборе device
            return startProcessing[device][batch].Last() +

                // Время выполнения задания с типов пакета на позиции batchIndex
                config.proccessingTime[device, schedule[batch].Type] +

                // Время выполнения ПТО
                config.preMaintenanceTimes[device];
        }

        protected int JobCompletionTime(int device, int batch, int job)
        {
            return startProcessing[device][batch][job] + config.proccessingTime[device, schedule[batch].Type];
        }

        #endregion

    }
}