using ConsoleTables;
using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Batch = magisterDiplom.Model.Batch;

namespace magisterDiplom.Fabric
{
    public class JsonFileWriter
    {
        private readonly string _filePath;
        private readonly object _fileLock = new object();
        private string globalTabulation = "\t";

        public void Tab(bool add = true)
        {
            if (add)
                globalTabulation = globalTabulation + "\t";
            else if (globalTabulation.Length != 0)
                globalTabulation.Remove(globalTabulation.Length - 1, 1);
        }

        public JsonFileWriter(string filePath = "output.json")
        {
            _filePath = filePath;
            // Очищаем файл при создании экземпляра
            lock (_fileLock)
            {
                File.WriteAllText(_filePath, "[\n");
            }
        }

        // Метод для завершения файла
        public void FinalizeFile()
        {
            lock (_fileLock)
            {
                // Удаляем последнюю запятую, если она есть
                var content = File.ReadAllText(_filePath);
                if (content.EndsWith(",\n"))
                {
                    content = content.Substring(0, content.Length - 2) + "\n";
                    File.WriteAllText(_filePath, content);
                }

                File.AppendAllText(_filePath, "]");
            }
        }

        public JsonFileWriter WriteNewObject(string title, string[] datas)
        {
            // File.AppendAllText(_filePath, jsonString);

            return this;
        }

        // Метод для сплошного текста
        public JsonFileWriter Write(string text, bool isSafe = true)
        {
            lock (_fileLock)
            {
                string jsonString;

                if (isSafe)
                    jsonString = $"{globalTabulation}\"{EscapeString(text)}\",\n";
                else
                    jsonString = $"{globalTabulation}\"{text}\",\n";

                File.AppendAllText(_filePath, jsonString);
            }
            return this;
        }

        // Метод для записи пары "ключ-значение"
        public JsonFileWriter Write(string key, object value, bool isSafe = true)
        {
            lock (_fileLock)
            {
                string jsonValue;

                if (value is string strValue)
                {
                    if (isSafe)
                        jsonValue = $"{globalTabulation}\"{EscapeString(strValue)}\"";
                    else
                        jsonValue = $"{globalTabulation}\"{strValue}\"";
                }
                else if (value is bool boolValue)
                {
                    var t = boolValue ? "true" : "false";
                    jsonValue = $"{globalTabulation}{t}";
                }
                else if (value is int || value is long || value is float ||
                         value is double || value is decimal || value is short ||
                         value is byte || value is sbyte || value is ushort ||
                         value is uint || value is ulong)
                {
                    jsonValue = $"{globalTabulation}{Convert.ToString(value).Replace(',', '.')}";
                }
                else
                {
                    if (isSafe)
                        jsonValue = $"{globalTabulation}\"{EscapeString(value.ToString())}\"";
                    else
                        jsonValue = $"{globalTabulation}\"{value.ToString()}\"";
                }

                string jsonPair;

                if (isSafe)
                    jsonPair = $"{globalTabulation}\"{EscapeString(key)}\":{jsonValue},\n";
                else
                    jsonPair = $"{globalTabulation}\"{key}\":{jsonValue},\n";

                File.AppendAllText(_filePath, jsonPair);
            }
            return this;
        }


        // Метод для записи пары "ключ-значение"
        public JsonFileWriter MegaWrite(string key, object value)
        {
            lock (_fileLock)
            {
                if (!(value is string strValue))
                    return this;

                string jsonValue = $"{strValue}";

                // Cоздаём экземпляр класса для работы со строками
                StringBuilder str = new StringBuilder(jsonValue.Length + 1000);

                string[] lines = jsonValue.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length - 1; i++)
                    str.Append(Environment.NewLine + globalTabulation + "\t" + "\"" + lines[i] + "\",");
                str.Append(Environment.NewLine + globalTabulation + "\t" + "\"" + lines[lines.Length - 1] + "\"" + Environment.NewLine + globalTabulation);

                string jsonPair = $"{globalTabulation}\"{key}\":{Environment.NewLine}{globalTabulation}{{{str.ToString()}}},\n";

                File.AppendAllText(_filePath, jsonPair);
            }
            return this;
        }

        public JsonFileWriter WriteMatrixYAsTable(List<List<int>> matrix, string title = "Матрица порядка ПТО")
        {
            lock (_fileLock)
            {
                if (matrix == null || matrix.Count == 0 || matrix[0].Count == 0)
                    return this;

                string[] columns = new string[matrix[0].Count + 1];
                columns[0] = "Прибор\\ПЗ";
                for (int i = 1; i < matrix[0].Count + 1; i++)
                    columns[i] = $"{i}";

                ConsoleTable table = new ConsoleTable(columns);

                for (int i = 0; i < matrix.Count; i++)
                {
                    int[] row = new int[matrix[i].Count + 1];
                    row[0] = i + 1;
                    for (int j = 0; j < matrix[i].Count; j++)
                        row[j + 1] = matrix[i][j];
                    table.AddRow(Array.ConvertAll(row, x => (object)x));
                }

                MegaWrite(title, table.ToStringAlternative()); 
            }
            return this;
        }

        public JsonFileWriter WriteScheduleAsTable(List<Batch> schedule, string title = "Расписание")
        {
            lock (_fileLock)
            {
                if (schedule == null || schedule.Count == 0)
                    return this;

                string[] columns = new string[schedule.Count];
                for (int i = 0; i < schedule.Count; i++)
                    columns[i] = $"{i + 1}";

                ConsoleTable table = new ConsoleTable(columns);

                int[] rowsType = new int[schedule.Count];
                for (int i = 0; i < schedule.Count; i++)
                {
                    rowsType[i] = schedule[i].Type;
                }
                table.AddRow(Array.ConvertAll(rowsType, x => (object)x));

                int[] rowsSize = new int[schedule.Count];
                for (int i = 0; i < schedule.Count; i++)
                {
                    rowsSize[i] = schedule[i].Size;
                }
                table.AddRow(Array.ConvertAll(rowsSize, x => (object)x));

                MegaWrite(title, table.ToStringAlternative());
            }
            return this;
        }

        public JsonFileWriter WriteProcessingAsTable(
            Dictionary<int, List<List<int>>> processingTime,
            List<List<int>> matrixY,
            List<Batch> schedule,
            PreMConfiguration config,
            string title = "Расписание моментов начала времени выполнения заданий")
        {
            lock (_fileLock)
            {
                if (processingTime == null || processingTime.Count == 0)
                    return this;

                int cols = processingTime[config.deviceCount - 1].Last().Last() + config.proccessingTime[config.deviceCount - 1, schedule.Last().Type] + config.preMaintenanceTimes.Last();
                string[] columns = new string[cols];
                for (int i = 0; i < cols; i++)
                    columns[i] = $"{i + 1}";

                ConsoleTable table = new ConsoleTable(columns);

                for (int device = 0; device < config.deviceCount; device++)
                {
                    string[] rows = new string[cols];
                    for (int batchIndex = 0; batchIndex < processingTime[device].Count; batchIndex++)
                    {
                        
                        for (int job = 0; job < processingTime[device][batchIndex].Count; job++)
                        {
                            int T = processingTime[device][batchIndex][job];
                            int time = T;
                            rows[time++] = $"[t={schedule[batchIndex].Type}|s={schedule[batchIndex].Size}";
                            for (; time < T + config.proccessingTime[device, schedule[batchIndex].Type] - 1; time++)
                            { 
                                rows[time] = $"t={schedule[batchIndex].Type}|s={schedule[batchIndex].Size}";
                            }
                            rows[time] = $"t={schedule[batchIndex].Type}|s={schedule[batchIndex].Size}]";
                        }

                        if (matrixY[device][batchIndex] == 1)
                        {
                            int T = processingTime[device][batchIndex].Last() + config.proccessingTime[device, schedule[batchIndex].Type];
                            int time = T;
                            rows[time++] = "[#######";
                            for (; time < T + config.preMaintenanceTimes[device] - 1; time++)
                                rows[time] = "#######";
                            rows[time] = "#######]";
                        }
                    }
                    table.AddRow(Array.ConvertAll(rows, x => (object)x));
                }
                MegaWrite(title, table.ToStringAlternative());
            }
            return this;
        }

        // Экранирование специальных символов
        private static string EscapeString(string input)
        {
            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    };

    /// <summary>
    /// Класс простого расписания с ПТО, наследуется от PreMSchedule
    /// </summary>
    public class SimplePreMSchedule : PreMSchedule
    {

        public class SimplePreMaintenceSecondLevelOutput : PreMaintenceSecondLevelOutput
        {

            public List<List<int>> Y_Matrix { get; private set; } = null;
            
            public SimplePreMaintenceSecondLevelOutput(SimplePreMSchedule schedule) : base(schedule)
            {
                if (!Success)
                {
                    return;
                }

                Y_Matrix = schedule._matrixY.ToListList();
            }


        }

        /// <summary>
        /// Матрица порядка ПТО приборов
        /// </summary>
        protected MatrixY _matrixY;

        /// <summary>
        /// Поток записи в файл
        /// </summary>
        //private JsonFileWriter fstream = null;

        /// <summary>
        /// Установит файл записи
        /// </summary>
        /// <param name="filename">Имя файла для записи</param>
        //public void SetLogFile(string filename)
        //{
        //    // Создаём объект для записи в файл
        //    if (fstream != null)
        //        fstream.FinalizeFile();
        //    fstream = new JsonFileWriter(filename);
        //}

        /// <summary>
        /// Закроет объект для записи в файла
        /// </summary>
        //private void UnsetLogFile()
        //{
        //    // Закрываем объект
        //    if (fstream != null)
        //        fstream.FinalizeFile();
        //    fstream = null;
        //}

        #region Программный интерфейс

        public SimplePreMSchedule(PreMConfiguration configuration) : base(configuration)
        {
            //SetLogFile("tmp.json");
        }

        public override void Update(int batchesCount)
        {
            base.Update(batchesCount);
            _matrixY = new MatrixY(config.deviceCount);

        }

        public override void Optimize()
        {

            Calculate();
            if (SolutionUnacceptable())
            {
                success = false;
                return;
            }

            for (int batch = 0; batch < ScheduleSize() - 1; batch++)
            {
                for (int device = 0; device < config.deviceCount; device++)
                {
                    _matrixY.UnsetPreMaintence(device, batch);
                    Calculate();
                    if (SolutionUnacceptable())
                    {
                        _matrixY.SetPreMaintence(device, batch);
                    }
                }
            }

            success = true;
        }

        public override SecondLevelOutput Result()
        {
            return new SimplePreMaintenceSecondLevelOutput(this);
        }

        #endregion

        #region Служебные переопределенные процедуры

        protected override int F2_criteria()
        {
            return TotalPreMaintenceDuration() + TotalInactionDuration();
        }

        protected override void AddColumnY()
        {
            _matrixY.AddColumn();
        }

        protected override void AddPreMaintenceAfterLastBatch()
        {
            _matrixY.SetPreMaintenceLastPacketAllDevices();
        }

        protected override bool HasPreMaintenceAfter(int device, int packet)
        {
            return _matrixY.PreMaintenceStatusAfter(device, packet) == 1;
        }
        
        protected override int PreMaintanceDurationAfter(int device, int packet)
        {
            return _matrixY.PreMaintenceStatusAfter(device, packet) * config.preMaintenanceTimes[device];
        }

        #endregion

        #region Служебные методы
        protected int TotalPreMaintenceDuration()
        {
            var result = 0;
            for (int device = 0; device < config.deviceCount; device++)
                for (int batch = 0; batch < ScheduleSize(); batch++)
                    result += PreMaintanceDurationAfter(device, batch);
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

        #endregion

    }
}
