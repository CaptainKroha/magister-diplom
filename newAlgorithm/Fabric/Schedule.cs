using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using System.Linq;
using newAlgorithm.Model;
using System.Collections.Generic;

namespace magisterDiplom
{

    /// <summary>
    /// Абстрактный класс содержащий общие функции для расписания
    /// </summary>
    public abstract class Schedule
    {

        /// <summary>
        /// Конфигурационная структура содержащая информацию о конвейерной системе
        /// </summary>
        private protected Configuration config;

        protected Schedule(Configuration configuration)
        {
            config = configuration;
        }

        /// <summary>
        /// Матрица порядка и количества пакетов заданий [deviceCount]
        /// </summary>
        protected List<Batch> schedule;

        /// <summary>
        /// Словарь соответствий приборов и матриц моментов начала времени выполнения заданий
        /// </summary>
        protected Dictionary<int, List<List<int>>> startProcessing = new Dictionary<int, List<List<int>>>();

        public Matrix R_matrix
        { 
            get {
                var res = new Matrix(config.dataTypesCount, ScheduleSize());
                for (int batch = 0; batch < ScheduleSize(); batch++)
                    res[schedule[batch].Type, batch] = schedule[batch].Size;
                return res;
            } 
        }

        public Matrix P_matrix
        {
            get {
                var res = new Matrix(config.dataTypesCount, ScheduleSize());
                for (int batch = 0; batch < ScheduleSize(); batch++)
                    res[schedule[batch].Type, batch] = 1;
                return res;
            }

        }

        public int MakeSpan
        {
            get {
                return startProcessing[config.deviceCount - 1].Last().Last() 
                    + config.proccessingTime[config.deviceCount - 1, schedule.Last().Type];
            }
        }

        /// <summary>
        /// Возвращает 3 мерную матрицу моментов времени выполнения заданий
        /// </summary>
        /// <returns>Словарь соответствия прибора к матрице моментов времени выполнения заданий в пакетах</returns>
        public Dictionary<int, List<List<int>>> GetStartProcessing()
        {
            CalcStartProcessing();
            return startProcessing;
        }

        protected abstract void CalcStartProcessing();

        protected abstract int F2_criteria();

        public abstract void Add(int dataType, int size);

        public abstract bool Optimize();

        public abstract void Update(int batchesCount);

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

        #region Старый интерфейс

        /// <summary>
        /// Возвращает матрицу количества заданий в пакетах
        /// </summary>
        /// <returns>Матрица количества заданий в пакетах</returns>
        public List<List<int>> GetMatrixR()
        {
            return Matrix.ToListList(R_matrix);
        }

        /// <summary>
        /// Возвращает матрицу порядка пакетов заданий
        /// </summary>
        /// <returns>Матрица порядка пакетов заданий</returns>
        public List<List<int>> GetMatrixP()
        {
            return Matrix.ToListList(P_matrix);
        }

        /// <summary>
        /// Возвращает критерий оптимизации makespan, определяющий время выполнения всех заданий в конвейерной системе
        /// </summary>
        /// <returns>Makespan - время выполнения всех заданий в системе</returns>
        public int GetMakespan()
        {
            return MakeSpan;
        }

        #endregion

    }
}