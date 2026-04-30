using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using magisterDiplom.Utils;
using newAlgorithm;
using newAlgorithm.Model;
using System.Collections.Generic;
using System.Linq;
using Batch = magisterDiplom.Model.Batch;

namespace magisterDiplom
{

    /// <summary>
    /// Абстрактный класс содержащий общие функции для расписания
    /// </summary>
    public abstract class Schedule
    {

        public class SecondLevelOutput
        {
            public bool Success { get; private set; }

            public int F2_Criteria { get; private set; } = 0;

            public List<List<int>> R_Matrix { get; private set; } = null;

            public List<List<int>> P_Matrix { get; private set; } = null;

            public Dictionary<int, List<List<int>>> StartProcessing { get; private set; } = null;

            public SecondLevelOutput(Schedule schedule)
            {
                Success = schedule.success;

                if (!Success)
                {
                    return;
                }

                F2_Criteria = schedule.F2_criteria();
                StartProcessing = schedule.startProcessing;

                P_Matrix = new List<List<int>>(schedule.config.dataTypesCount);
                for(int dataType = 0; dataType < schedule.config.dataTypesCount; ++dataType)
                {
                    P_Matrix.Add(new List<int>(schedule.ScheduleSize()));
                    for (int batch = 0; batch < schedule.ScheduleSize(); batch++)
                        P_Matrix[dataType].Add(0);
                }

                for (int batch = 0; batch < schedule.ScheduleSize(); batch++)
                    P_Matrix[schedule.BatchType(batch)][batch] = 1;

                R_Matrix = new List<List<int>>(schedule.config.dataTypesCount);
                for (int dataType = 0; dataType < schedule.config.dataTypesCount; ++dataType)
                {
                    R_Matrix.Add(new List<int>(schedule.ScheduleSize()));
                    for (int batch = 0; batch < schedule.ScheduleSize(); batch++)
                        R_Matrix[dataType].Add(0);
                }

                for (int batch = 0; batch < schedule.ScheduleSize(); batch++)
                    R_Matrix[schedule.BatchType(batch)][batch] = schedule.BatchSize(batch);
            }

            public int BatchType(int batch)
            {
                for(int row = 0; row < P_Matrix.Count; ++row)
                {
                    if(P_Matrix[row][batch] == 1)
                    {
                        return row;
                    }
                }

                return -1;
            }

        }

        /// <summary>
        /// Конфигурационная структура содержащая информацию о конвейерной системе
        /// </summary>
        private protected Configuration config;
        protected List<Batch> schedule;

        protected bool success;

        /// <summary>
        /// Словарь соответствий приборов и матриц моментов начала времени выполнения заданий
        /// </summary>
        protected Dictionary<int, List<List<int>>> startProcessing = new Dictionary<int, List<List<int>>>();

        protected readonly ILogger _logger;

        public Schedule(Configuration configuration, ILogger logger)
        {
            config = configuration;
            _logger = logger;
            _logger.Print(configuration.ToString());
        }

        public int MakeSpan
        {
            get {
                return startProcessing[config.deviceCount - 1].Last().Last() 
                    + config.proccessingTime[config.deviceCount - 1, schedule.Last().Type];
            }
        }

        protected abstract void CalcStartProcessing();

        protected abstract int F2_criteria();

        public abstract void Add(int dataType, int size);

        public abstract void Optimize();

        public abstract void Update(int batchesCount);

        public abstract SecondLevelOutput Result();

        public List<int> DataTypesInPriority()
        {
            Dictionary<int, double> m = new Dictionary<int, double>(capacity: config.dataTypesCount);

            for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
            {
                double sum = 0;
                for (int device = 1; device < config.deviceCount; device++)
                    sum +=
                        (double)config.proccessingTime[device, dataType] /
                        (double)config.proccessingTime[device - 1, dataType];
                m.Add(dataType, sum);
            }

            List<int> dataTypes = new List<int>(capacity: config.dataTypesCount);

            while (m.Any())
            {
                int myDataType = m.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                dataTypes.Add(myDataType);
                m.Remove(myDataType);
            }

            return dataTypes;
        }

        protected virtual void Calculate()
        {
            CalcStartProcessing();
        }

        /// <summary>
        /// Данная функция выполняет локальную оптимизацию составов ПЗ
        /// </summary>
        /// <param name="swapCount">Количество перестановок</param>
        /// <returns>true, если была найдено перестановка удовлетворяющая условию надёжности. Иначе false</returns>
        protected bool OptimizeLocaly(int swapCount = 999999)
        {

            List<Batch> bestSchedule = new List<Batch>(schedule);

            Calculate();
            int bestValue = F2_criteria();

            for (int batch = ScheduleSize() - 1; batch > 0 && swapCount > 0; batch--, swapCount--)
            {

                // Выполняем перестановку
                (schedule[batch - 1], schedule[batch]) = (schedule[batch], schedule[batch - 1]);

                Calculate();
                int newValue = F2_criteria();

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

        protected int ScheduleSize()
        {
            return schedule.Count;
        }

        protected int BatchType(int batch)
        {
            return schedule[batch].Type;
        }

        protected int BatchSize(int batch)
        {
            return schedule[batch].Size;
        }

        protected int CompletionTimeLastJobOfBatch(int device, int batch)
        {
            return JobCompletionTime(device, batch, BatchSize(batch) - 1);
        }

        protected int JobCompletionTime(int device, int batch, int job)
        {
            return startProcessing[device][batch][job] + config.proccessingTime[device, schedule[batch].Type];
        }

        protected int ChangeoverDuration(int device, int fromBatch, int toBatch)
        {
            return config.changeoverTime[device][schedule[fromBatch].Type, schedule[toBatch].Type];
        }

    }
}