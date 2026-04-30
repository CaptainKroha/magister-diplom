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

        public class PreMaintenceSecondLevelOutput : SecondLevelOutput
        {
            public List<List<int>> Tpm_Matrix { get; private set; } = null;

            public PreMaintenceSecondLevelOutput(PreMSchedule schedule) : base(schedule)
            {
                if (!Success)
                {
                    return;
                }

                Tpm_Matrix = new List<List<int>>(schedule.config.deviceCount);

                for (int device = 0; device < schedule.config.deviceCount; device++)
                {
                    Tpm_Matrix.Add(new List<int>(schedule.ScheduleSize()));
                    for (int batch = 0; batch < schedule.ScheduleSize(); batch++)
                        Tpm_Matrix[device].Add(0);

                    foreach(var preMSet in schedule.matrixTPM[device])
                    {
                        Tpm_Matrix[device][preMSet.BatchIndex] = preMSet.TimePreM;
                    }
                }
            }

        }

        /// <summary>
        /// Конфигурационная структура содержащая информацию о ПТО
        /// </summary>
        private protected readonly new PreMConfiguration config;

        /// <summary>
        /// Матрица моментов времени окончания ПТО приборов
        /// </summary>
        protected List<List<PreMSet>> matrixTPM = new List<List<PreMSet>>();

        public PreMSchedule(PreMConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
            config = configuration;
        }

        public override void Add(int dataType, int size)
        {
            schedule.Add(new Batch(dataType, size));

            AddColumnY();

            if(ScheduleSize() > 1)
            {
                OptimizeLocaly(5);
            }
            
            AddPreMaintenceAfterLastBatch();
        }

        protected bool SolutionUnacceptable()
        {

            for (int device = 0; device < config.deviceCount; device++)
            {   
                for (int batch = 0; batch < ScheduleSize(); batch++)
                {
                    int time = CompletionTimeLastJobOfBatch(device, batch);
                    double systemReliability = SystemReliabilityBy(time);

                    // Проверяем ограничение надёжности
                    if (systemReliability < config.beta)
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

        public override SecondLevelOutput Result()
        {
            return new PreMaintenceSecondLevelOutput(this);
        }

        #region Служебные методы

        #region Служебные методы переопределяемые в наследниках

        protected abstract void AddColumnY();

        protected abstract void AddPreMaintenceAfterLastBatch();

        protected abstract bool HasPreMaintenceAfter(int device, int packet);

        protected abstract int PreMaintanceDurationAfter(int device, int packet);

        #endregion

        protected void CalcMatrixTPM()
        {
            matrixTPM?.Clear();

            for (int device = 0; device < config.deviceCount; device++)
            {
                matrixTPM.Add(new List<PreMSet>());
                for (int batch = 0; batch < ScheduleSize(); batch++)
                {
                    if (!HasPreMaintenceAfter(device, batch))
                    {
                        continue;
                    }
                    matrixTPM[device].Add(new PreMSet(batch, PreMaintenceCompletionTimeAfter(device, batch)));
                }
            }
        }

        protected override void Calculate()
        {
            base.Calculate();
            CalcMatrixTPM();
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

        protected double SystemReliabilityBy(int time)
        {
            double result = 1;
            for (int device = 0; device < config.deviceCount; device++)
            {
                result *= DeviceReliabilityBy(device, time);
            }
            return result;
        }

        private void CalcStartProcessingFirstDevice()
        {
            int device = 0, batch = 0, job = 0;

            // Устанавливаем момент начала времени выполнения 1 задания в 1 пакете на 1 приборе, как наладку
            startProcessing[device][batch][job] = ChangeoverDuration(device, batch, batch);

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

                    CompletionTimeLastJobOfBatch(device, batch - 1) +

                    // Время переналадки с предыдущего типа на текущий
                    ChangeoverDuration(device, batch - 1, batch) +

                    // Время выполнения ПТО после предыдущего ПЗ
                    PreMaintanceDurationAfter(device, batch -  1); ;

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
                ChangeoverDuration(device, batch, batch),

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

                    CompletionTimeLastJobOfBatch(device, batch - 1) +

                    // Время переналадки с предыдущего типа на текущий
                    ChangeoverDuration(device, batch - 1, batch) +

                    // Время выполнения ПТО
                    PreMaintanceDurationAfter(device, batch - 1),

                    JobCompletionTime(device - 1, batch, job));

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
                result += startProcessing[device][batch].First() - CompletionTimeLastJobOfBatch(device, batch - 1);
            }

            return result;
        }

        protected int DeviceInactionDurationBetweenJobsInBatch(int device, int batch)
        {
            var result = 0;

            for (int job = 1; job < BatchSize(batch); ++job)
            {
                result += startProcessing[device][batch][job] - JobCompletionTime(device, batch, job - 1);
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
            return CompletionTimeLastJobOfBatch(device, batch) + PreMaintanceDurationAfter(device, batch);
        }

        #endregion

    }
}