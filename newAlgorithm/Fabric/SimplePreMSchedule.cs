using ConsoleTables;
using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using magisterDiplom.Utils;
using newAlgorithm;
using newAlgorithm.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
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
        //const bool IsDebug_ShiftMatrixY = true;

        /// <summary>
        /// Матрица порядка ПТО приборов
        /// </summary>
        protected MatrixY _matrixY;

        public SimplePreMSchedule(PreMConfiguration configuration) : base(configuration)
        {
            //SetLogFile("tmp.json");

            //// Если флаг оталдки установлен
            //if (IsDebug)
            //{

            //    // Выводим информацию о переданной конфигурационной структуре
            //    fstream.Write($"{config.ToString()}");
            //}
        }

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

        public override void Update(int batchesCount)
        {
            base.Update(batchesCount);
            _matrixY = new MatrixY(config.deviceCount);

        }

        public override bool Optimize()
        {

            Calculate();
            if (SolutionUnacceptable())
            {
                return false;
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

            return true;
        }       
        
        /// <summary>
        /// Вернёт тип данных по переданному индексу ПЗ
        /// </summary>
        /// <param name="batchIndex">Индекс ПЗ</param>
        /// <returns>Тип данных</returns>
        public int GetDataTypeByBatchIndex(int batchIndex)
        {
            return schedule[batchIndex].Type;
        }

        /// <summary>
        /// Возвращает матрицу ПТО приборов
        /// </summary>
        /// <returns>Матрица ПТО приборов</returns>
        public List<List<int>> GetMatrixY()
        {
            return MatrixY.ToListList(_matrixY);
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

        protected override int PreMaintenceStatusAfter(int device, int packet)
        {
            return _matrixY.PreMaintenceStatusAfter(device, packet);
        }

        //public override bool Build(List<List<int>> _matrixA)
        //{
        //    return true;

        // return BuildWithoutLogging(_matrixA);

        // Если установлено логирование
        //if (Form1.loggingOn)

        //    // Записываем данные в файл
        //    fstream.Write("Начинаем выполнять операции на нижнем уровне;");

        //List<List<int>> matrixA = ListUtils.MatrixIntDeepCopy(_matrixA);

        //// Если установлено логирование
        //if (Form1.loggingOn)
        //{

        //    // Cоздаём экземпляр класса для работы со строками
        //    StringBuilder str = new StringBuilder(200);

        //    // Объявляем временную строку
        //    str.AppendLine($"Матрица A:");

        //    // Для каждого типа данных
        //    for (int _dataType = 0; _dataType < matrixA.Count(); _dataType++)
        //    {

        //        // Добавляем новые данные в строку
        //        str.Append($"\tТип {_dataType + 1}: ");

        //        // Для каждого пакета в векторе типа _dataType матрицы A
        //        for (int _batchIndex = 0; _batchIndex < matrixA[_dataType].Count(); _batchIndex++)

        //            // Добавляем в строку данные
        //            str.Append($"{matrixA[_dataType][_batchIndex]} ");

        //        // Добавляем перевод строки
        //        str.Append(Environment.NewLine);
        //    }

        //    // Записываем заголовок
        //    fstream.Write(str.ToString());
        //}

        //int dataType, maxBatchCount = 0, batch = 0, batchCount = 0;

        //// Вычисляем максимальное количество пакетов среди всех типов данных
        //calcMaxBatchCount();

        //// Если установлено логирование
        //if (Form1.loggingOn)

        //    // Записываем данные в файл
        //    fstream.Write("maxBatchCount", maxBatchCount);

        //// Вернёт максимальное количество пакетов среди всех типов данных
        //void calcMaxBatchCount()
        //{
        //    // Выполняем обработку по типам
        //    for (dataType = 0; dataType < this.config.dataTypesCount; dataType++)

        //        // Выполняем поиск максимального количество пакетов
        //        maxBatchCount = Math.Max(maxBatchCount, matrixA[dataType].Count);
        //}

        //Dictionary<int, double> m = new Dictionary<int, double>(capacity: this.config.dataTypesCount);
        //List<int> dataTypes = new List<int>(capacity: this.config.dataTypesCount);
        //for (dataType = 0; dataType < this.config.dataTypesCount; dataType++)
        //{
        //    double sum = 0;
        //    for (int device = 1; device < this.config.deviceCount; device++)
        //        sum +=
        //            (double)this.config.proccessingTime[device][dataType] /
        //            (double)this.config.proccessingTime[device - 1][dataType];
        //    m.Add(dataType, sum);
        //}

        //// Если установлено логирование
        //if (Form1.loggingOn)

        //    // Записываем данные в файл
        //    fstream.Write($"Типы данных:");

        //while (m.Any())
        //{
        //    int myDataType = m.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        //    dataTypes.Add(myDataType);
        //    // Если установлено логирование
        //    if (Form1.loggingOn)

        //        // Записываем данные в файл
        //        fstream.Write($"{myDataType}: {m[myDataType]}");
        //    m.Remove(myDataType);
        //}


        //// Если установлено логирование
        //if (Form1.loggingOn)
        //{

        //    // Выводим информацию
        //    fstream.Write("dataTypes:");

        //    // Для каждого типа
        //    for (int _dataType = 0; _dataType < this.config.dataTypesCount; _dataType++)

        //        // Выводим информацию
        //        fstream.Write($"{_dataType}: {dataTypes[_dataType]}");
        //}

        //// Сортируем матрицу A
        //for (dataType = 0; dataType < this.config.dataTypesCount; dataType++)
        //    matrixA[dataType].Sort();

        //// Если установлено логирование
        //if (Form1.loggingOn)
        //{

        //    // Cоздаём экземпляр класса для работы со строками
        //    StringBuilder str = new StringBuilder(200);

        //    // Объявляем временную строку
        //    str.AppendLine($"Матрица A:");

        //    // Для каждого типа данных
        //    for (int _dataType = 0; _dataType < matrixA.Count(); _dataType++)
        //    {

        //        // Добавляем новые данные в строку
        //        str.Append($"\tТип {_dataType + 1}: ");

        //        // Для каждого пакета в векторе типа _dataType матрицы A
        //        for (int _batchIndex = 0; _batchIndex < matrixA[_dataType].Count(); _batchIndex++)

        //            // Добавляем в строку данные
        //            str.Append($"{matrixA[_dataType][_batchIndex]} ");

        //        // Добавляем перевод строки
        //        str.Append(Environment.NewLine);
        //    }

        //    // Записываем заголовок
        //    fstream.Write(str.ToString());
        //}

        //batch = dataType = 0;

        //// Объявляем количество пакетов заданий


        //// Для каждого типа данных
        //for (int _dataType = 0; _dataType < matrixA.Count(); _dataType++)

        //    // Увеличиваем общее количество пакетов заданий
        //    batchCount += matrixA[_dataType].Count();

        //// П.2 Добавляем 
        //this.schedule = new List<Batch>(batchCount) { new Batch(
        //    dataTypes[dataType],
        //    matrixA[dataTypes[dataType]][batch]
        //)};
        //dataType++;

        //// Если логирование установлено
        //if (Form1.loggingOn) {
        //    CalcStartProcessing();
        //    fstream.WriteScheduleAsTable(schedule);
        //}
        //// П.3 Инициализируем матрицу Y
        //this.matrixY = new List<List<int>>(capacity: this.config.deviceCount);
        //for (int device = 0; device < this.config.deviceCount; device++)
        //{
        //    this.matrixY.Add(new List<int>());
        //    this.matrixY[device].Add(1);
        //}
        //// Если логирование установлено
        //if (Form1.loggingOn)
        //{
        //    fstream.WriteMatrixYAsTable(matrixY);
        //}

        //// Для каждого типа данных выполняем обрабоку
        //for (; dataType < this.config.dataTypesCount; dataType++)
        //{

        //    // Добавляем ПЗ в расписание 
        //    this.schedule.Add(new Batch(dataTypes[dataType], matrixA[dataTypes[dataType]][batch]));
        //    for (int device = 0; device < this.config.deviceCount; device++)
        //        this.matrixY[device].Add(0);
        //    CalcStartProcessing();

        //    // Если логирование установлено
        //    if (Form1.loggingOn)
        //    {
        //        fstream.WriteProcessingAsTable(startProcessing, matrixY, schedule, config);
        //    }

        //    // Если не было найдено расписания удовлетворяющему условию надёжности
        //    if (!this.SearchByPosition(5)) {

        //        // Закрываем файл
        //        UnsetLogFile();

        //        // Возвращаем флаг неудачи
        //        return false;
        //    }

        //    // Выполняем оптимизацию для позиций ПТО приборов
        //    // this.ShiftMatrixY();

        //    // Проверяем условие надёжности
        //    // if (!this.IsSolutionAcceptable()) {

        //        // Закрываем файл
        //        // UnsetLogFile();

        //        // Возвращаем флаг неудачи
        //        // return false;
        //    // }
        //}

        //// Увеличиваем индекс вставляемого пакета задания
        //batch++;

        //// Выполняем обработку
        //while (batch < maxBatchCount)
        //{

        //    // Выполняем обработку для каждого типа данных
        //    for (dataType = 0; dataType < this.config.dataTypesCount; dataType++)
        //    {

        //        // Если индекс пакета превышает максимальный размер пакетов для типа данных dataType
        //        if (batch >= matrixA[dataTypes[dataType]].Count)

        //            // Продолжаем обработку для следующего типа данных
        //            continue;

        //        // Добавляем ПЗ в расписание 
        //        this.schedule.Add(new Batch(dataTypes[dataType], matrixA[dataTypes[dataType]][batch]));
        //        for (int device = 0; device < this.config.deviceCount; device++)
        //            this.matrixY[device].Add(0);

        //        // Если не было найдено расписания удовлетворяющему условию надёжности
        //        if (!this.SearchByPosition(5)) {

        //            // Закрываем файл
        //            UnsetLogFile();

        //            // Возвращаем флаг неудачи
        //            return false;
        //        }

        //        // Выполняем оптимизацию для позиций ПТО приборов (ШАГ 15)
        //        // this.ShiftMatrixY();

        //        // Проверяем условие надёжности
        //        // if (!this.IsSolutionAcceptable()) {

        //            // Закрываем файл
        //            // UnsetLogFile();

        //            // Возвращаем флаг неудачи
        //            // return false;
        //        // }
        //    }

        //    // Увеличиваем индекс пакета
        //    batch++;
        //}


        //// Формируем матрицу со всеми единицами в ПТО
        //// = new List<List<int>>(capacity: config.deviceCount);

        //for (int device = 0; device < matrixY.Count(); device++)
        //    for (int batchIndex = 0; batchIndex < matrixY[device].Count(); batchIndex++)
        //        matrixY[device][batchIndex] = 1;

        //// Возвращяем флаг удачного построения расписания
        //bool b = Optimize();

        //// Если установлено логирование
        //if (Form1.loggingOn)
        //{

        //    // Записываем данные в файл
        //    fstream.Write("Начинаем выполнять операции на нижнем уровне");
        //}

        //return b;
        //}

        //private bool BuildWithoutLogging(List<List<int>> _matrixA)
        //{

        //    List<List<int>> matrixA = ListUtils.MatrixIntDeepCopy(_matrixA);

        //    for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
        //        matrixA[dataType].Sort();

        //    int batchCount = 0;

        //    for (int dataType = 0; dataType < matrixA.Count(); dataType++)
        //        batchCount += matrixA[dataType].Count();

        //    schedule = new List<Batch>(batchCount);

        //    matrixY = new List<List<int>>(capacity: config.deviceCount);
        //    for (int device = 0; device < config.deviceCount; device++)
        //    {
        //        matrixY.Add(new List<int>());
        //        matrixY[device].Add(1);
        //    }

        //    int maxBatchCount = 0;

        //    for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
        //        maxBatchCount = Math.Max(maxBatchCount, matrixA[dataType].Count);

        //    List<int> dataTypes = DataTypesInPriority();

        //    for (int batch = 0; batch < maxBatchCount; batch++)
        //    {
        //        foreach (int dataType in dataTypes)
        //        {
        //            if (batch >= matrixA[dataType].Count)
        //                continue;

        //            bool success = AddBatchInSchedule(dataType, matrixA[dataType][batch]);
        //            if(!success)
        //            {
        //                return false;
        //            }
        //        }
        //    }

        //    for (int device = 0; device < config.deviceCount; device++)
        //        for (int batch = 0; batch < matrixY[device].Count(); batch++)
        //            matrixY[device][batch] = 1;

        //    bool b = Optimize();
        //    return b;
        //}

    }
}
