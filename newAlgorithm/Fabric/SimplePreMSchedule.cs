using ConsoleTables;
using magisterDiplom.Model;
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
            PreMConfig preMConfig,
            string title = "Расписание моментов начала времени выполнения заданий")
        {
            lock (_fileLock)
            {
                if (processingTime == null || processingTime.Count == 0)
                    return this;

                int cols = processingTime[preMConfig.config.deviceCount - 1].Last().Last() + preMConfig.config.proccessingTime[preMConfig.config.deviceCount - 1][schedule.Last().Type] + preMConfig.preMaintenanceTimes.Last();
                string[] columns = new string[cols];
                for (int i = 0; i < cols; i++)
                    columns[i] = $"{i + 1}";

                ConsoleTable table = new ConsoleTable(columns);

                for (int device = 0; device < preMConfig.config.deviceCount; device++)
                {
                    string[] rows = new string[cols];
                    for (int batchIndex = 0; batchIndex < processingTime[device].Count; batchIndex++)
                    {
                        
                        for (int job = 0; job < processingTime[device][batchIndex].Count; job++)
                        {
                            int T = processingTime[device][batchIndex][job];
                            int time = T;
                            rows[time++] = $"[t={schedule[batchIndex].Type}|s={schedule[batchIndex].Size}";
                            for (; time < T + preMConfig.config.proccessingTime[device][schedule[batchIndex].Type] - 1; time++)
                            { 
                                rows[time] = $"t={schedule[batchIndex].Type}|s={schedule[batchIndex].Size}";
                            }
                            rows[time] = $"t={schedule[batchIndex].Type}|s={schedule[batchIndex].Size}]";
                        }

                        if (matrixY[device][batchIndex] == 1)
                        {
                            int T = processingTime[device][batchIndex].Last() + preMConfig.config.proccessingTime[device][schedule[batchIndex].Type];
                            int time = T;
                            rows[time++] = "[#######";
                            for (; time < T + preMConfig.preMaintenanceTimes[device] - 1; time++)
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
        const bool IsDebug_ShiftMatrixY = true;

        /// <summary>
        /// Поток записи в файл
        /// </summary>
        private JsonFileWriter fstream = null;

        /// <summary>
        /// Установит файл записи
        /// </summary>
        /// <param name="filename">Имя файла для записи</param>
        public void SetLogFile(string filename)
        {
            // Создаём объект для записи в файл
            if (fstream != null)
                fstream.FinalizeFile();
            fstream = new JsonFileWriter(filename);
        }

        /// <summary>
        /// Закроет объект для записи в файла
        /// </summary>
        private void UnsetLogFile()
        {
            // Закрываем объект
            if (fstream != null)
                fstream.FinalizeFile();
            fstream = null;
        }

        /// <summary>
        /// Вернёт индекс ПЗ за которым следует последнее ПТО
        /// </summary>
        /// <param name="device">Индекс прибора</param>
        /// <returns>Индекс ПЗ за которым следует последнее ПТО или -1</returns>
        private int GetLastPreMPos(int device)
        {

            // Выполняем обход по всем ПЗ
            for (int batchIndex = this.matrixY[device].Count() - 1; batchIndex >= 0; batchIndex--)

                // Если в текущей позиции существует ПТО
                if (this.matrixY[device][batchIndex] != 0)

                    // Возвращаем индекс данной позиции
                    return batchIndex;

            // Вернём -1
            return -1;
        }

        /// <summary>
        /// Выполняем сдвиг для матрицы ПТО приборов
        /// </summary>
        private void ShiftMatrixY()
        {

            // Если логирование установлена
            if (Form1.loggingOn)
                fstream.Write("ShiftMatrixY start: Улучшаем позиции ПТО");

            // Объявляем индекс прибора
            int bestDevice = -1;

            // Объявляем критерий текущего лучшего расписания
            int f2;

            // Объявляем критерий f2 для текущего расписания со сдвигом
            int new_f2;

            // Объявляем индекс ПЗ за которым следует последнее ПТО
            int last_prem_batch_index;

            // Объявляем индекс последнего ПЗ для текущего расписания
            int last_batch_index;

            // Выполняем обработку в цикле
            do {

                // Обнуляем критерий f2 для текущего расписания
                f2 = int.MaxValue;

                // Если логирование установлено
                if (Form1.loggingOn) {
                    fstream.Write("Новая итерация сдвигов");

                    CalcStartProcessing();
                    CalcMatrixTPM();

                    fstream.Write("1");
                    fstream.WriteMatrixYAsTable(matrixY);
                    fstream.WriteProcessingAsTable(startProcessing, matrixY, schedule, preMConfig);

                    // INFO: Ранее f2 критерий был this.GetPreMUtility();
                    // INFO: Ранее f2 критерий был this.GetSigmaDowntime();
                    fstream.Write("f2 для текущего расписания", this.CalculateCriteria_f2());
                }

                // Для каждого прибора выполняем обработку
                for (int device = 0; device < config.deviceCount; device++)
                {

                    // Вычисляем матрицу моментов начала времени выполнения заданий
                    CalcStartProcessing();

                    // Вычисляем матрицу моментов окончания времени выполнения ПТО
                    CalcMatrixTPM();

                    // Определяем индекс ПЗ за которым следует последнее ПТО
                    last_prem_batch_index = this.GetLastPreMPos(device); // j*

                    // Определяем индекс последнего ПЗ для текущего расписания
                    last_batch_index = this.matrixY[device].Count() - 1; // j^max

                    // Если логирование установлено
                    // TODO: Добавить текст:значение
                    if (Form1.loggingOn)
                        fstream.Write($"Выполняем сдвиг для прибора: {device} j*={last_prem_batch_index}; j^max={last_batch_index}");


                    // Проверяем на необходимость проведения операций перестановки
                    if (last_prem_batch_index == last_batch_index) {
                        if (IsDebug && IsDebug_ShiftMatrixY)
                            fstream.Write($"Пропускаем сдвиг для прибора: {device}");

                        // Пропускаем итерацию для текущего прибора
                        continue;
                    }

                    // Выполняем сдвиг ПТО на следующую позицию
                    this.matrixY[device][last_prem_batch_index] = 0;
                    this.matrixY[device][last_prem_batch_index + 1] = 1;

                    // Вычисляем матрицу моментов начала времени выполнения заданий
                    this.CalcStartProcessing();

                    // Вычисляем матрицу моментов окончания времени выполнения ПТО
                    this.CalcMatrixTPM();

                    // Если логирование установлено
                    if (Form1.loggingOn)
                    {
                        fstream.Write("2");
                        fstream.WriteMatrixYAsTable(matrixY);
                        fstream.WriteProcessingAsTable(startProcessing, matrixY, schedule, preMConfig);
                    }

                    // Если текущее решение не удовлетворяет условию надёжности
                    if (!this.IsSolutionAcceptable())
                    {
                        // Если логирование установлено
                        if (Form1.loggingOn)
                            fstream.Write("РЕШЕНИЕ НЕ ДОПУСТИМО");

                        // Выполняем обратный сдвиг ПТО
                        this.matrixY[device][last_prem_batch_index] = 1;
                        this.matrixY[device][last_prem_batch_index + 1] = 0;

                        // Пропускаем итерацию
                        continue;
                    }

                    // Вычисляем критерий f2 для текущего расписания со сдвигом
                    // INFO: Ранее f2 критерий был this.GetPreMUtility();
                    // INFO: Ранее f2 критерий был this.GetSigmaDowntime();
                    new_f2 = this.CalculateCriteria_f2();

                    // Если логирование установлено
                    if (Form1.loggingOn)
                        fstream.Write($"РЕШЕНИЕ ДОПУСТИМО. G(f2)={f2};new_f2={new_f2}");

                    // Если текущее расписания лучше предыдущего
                    if (new_f2 < f2)
                    {

                        // Запоминаем новый лучший критерий f2
                        f2 = new_f2;

                        // Переопределяем индекс прибора
                        bestDevice = device;
                    }

                    // Выполняем обратный сдвиг ПТО
                    this.matrixY[device][last_prem_batch_index] = 1;
                    this.matrixY[device][last_prem_batch_index + 1] = 0;
                }

                // Если логирование установлено
                if (Form1.loggingOn)
                    fstream.Write("f2", f2);

                // Если улучшений позиций ПТО не было найдено
                if (f2 == int.MaxValue)
                {
                    // Если логирование установлено
                    if (Form1.loggingOn)
                        fstream.Write("Улучшений не было найдено");

                    // Прекращаем обработку
                    break;
                }

                // Если логирование установлено
                if (Form1.loggingOn)
                    fstream.Write("Было найдено улучшение");

                // Определяем индекс ПЗ за которым следует последнее ПТО
                last_prem_batch_index = this.GetLastPreMPos(bestDevice); // j*

                // Выполняем их переопределение
                this.matrixY[bestDevice][last_prem_batch_index] = 0;
                this.matrixY[bestDevice][last_prem_batch_index + 1] = 1;

                // Если логирование установлено
                if (Form1.loggingOn) {
                    fstream.Write("Новое решение:");
                    fstream.Write("3");
                    fstream.WriteMatrixYAsTable(matrixY);
                    fstream.WriteProcessingAsTable(startProcessing, matrixY, schedule, preMConfig);
                }

                // Продолжаем улучшения
            } while (true);

            // Если логирование установлено
            if (Form1.loggingOn)
            {
                fstream.Write("Было найдено решение с помощью сдвигов.");
                fstream.Write("Выполняется заполнение позиций ПТО не найденных с помощью сдвигов.");
            }

            // Для каждого прибора выполняем дополнение для матрицы ПТО 1
            for (int device = 0; device < this.config.deviceCount; device++)
            {
                // Если логирование установлено
                if (Form1.loggingOn) {
                    fstream.Write("Для прибора", device);
                }

                // Определяем индекс ПЗ за которым следует последнее ПТО
                last_prem_batch_index = this.GetLastPreMPos(device); // j*

                // Определяем индекс последнего ПЗ для текущего расписания
                last_batch_index = this.matrixY[device].Count() - 1; // j^max

                // Если матрица Y не оканчивается 1
                if (last_prem_batch_index < last_batch_index) {

                    // Если логирование установлено
                    if (Form1.loggingOn)
                        fstream.Write("ПТО добавляется");

                    // Изменяем индекс последнего ПТО нп 1
                    this.matrixY[device][last_batch_index] = 1;
                }

                // Если логирование установлено
                else if (Form1.loggingOn)
                    fstream.Write("ПТО не добавляется");
            }

            // Если логирование установлено
            if (Form1.loggingOn)
            {
                fstream.WriteProcessingAsTable(startProcessing, matrixY, schedule, preMConfig);
                fstream.WriteMatrixYAsTable(matrixY);
                fstream.Write("ShiftMatrixY stop");
            }
        }

        /// <summary>
        /// Данная функция выполняет локальную оптимизацию составов ПЗ
        /// </summary>
        /// <param name="swapCount">Количество перестановок</param>
        /// <returns>true, если была найдено перестановка удовлетворяющая условию надёжности. Иначе false</returns>
        private bool SearchByPosition(int swapCount = 999999)
        {

            if (Form1.loggingOn)
                fstream.Write($"SearchByPosition start: Изменяем позиции пакета заданий. beta:{preMConfig.beta}; swapCount:{swapCount}{Environment.NewLine}");

            List<Batch> bestSchedule = new List<Batch>(schedule);
            
            // Объявляем значение наилучшего критерия f2
            int bestTime = int.MaxValue;

            CalcStartProcessing();
            CalcMatrixTPM();

            // Если логирование установлено
            if (Form1.loggingOn)
            {
                fstream.Write("Начальное расписание");
                fstream.WriteProcessingAsTable(startProcessing, matrixY, schedule, preMConfig);
            }

            for (int batch = schedule.Count - 1; batch > 0 && swapCount > 0; batch--, swapCount--)
            {

                // Выполняем перестановку
                (schedule[batch - 1], schedule[batch]) = (schedule[batch], schedule[batch - 1]);

                if (Form1.loggingOn)
                {
                    fstream.Write($"Выполняем перестановку {batch} и {batch - 1}");
                    fstream.WriteScheduleAsTable(schedule);
                }

                CalcStartProcessing();
                CalcMatrixTPM();

                int newTime = CalculateCriteria_f2();

                // Если логирование установлено
                if (Form1.loggingOn)
                {
                    fstream.Write("Текущее расписание допустимо");
                    fstream.Write("new_f2 для текущего расписания",newTime);
                }

                if (newTime < bestTime)
                {

                    if (Form1.loggingOn)
                        fstream.Write("Текущее расписание лучше предыдущего. ({newTime} < {bestTime})");
                        
                    // TODO: Избавиться от копирования списка в пользу использования индекса наилучшей позиции
                    // Переопределяем лучшее расписание
                    bestSchedule = new List<Batch>(schedule);

                    bestTime = newTime;
                }
            }

            schedule = bestSchedule;

            if (Form1.loggingOn)
            {
                fstream.Write("Извлекаем лучшее расписание");
                fstream.WriteScheduleAsTable(schedule);
            }

            return true;
        }

        /// <summary>
        /// Функция проверяет допустимость решения
        /// </summary>
        /// <returns>true - если текущее решение допустимо. Иначе False</returns>
        private bool IsSolutionAcceptable()
        {

            // Если логирование установлено
            if (Form1.loggingOn)
                fstream.Write($"IsSolutionAcceptable start: Проверяем допустимость решения. beta:{preMConfig.beta}{Environment.NewLine}");
                
            // Для каджого прибора выполняем обработку
            for (int device = 0; device < config.deviceCount; device++)
            
                // Для каждого ПЗ в расписании выполняем обработку
                for (int batch = 0; batch < schedule.Count; batch++)

                    // Если для данной позиции существует ПТО
                    if (matrixY[device][batch] != 0)
                    {

                        // Вычисляем момент времени окончания ПТО на текущем приборе в текущей позиции
                        int time =

                            // Момент времени начала выполнения последнего задания на текущем приборе в текущей позиции
                            startProcessing[device][batch].Last() +

                            // Время выполнения задания на текущем приборе в текущей позиции 
                            config.proccessingTime[device][schedule[batch].Type];

                        CalcMatrixTPM();

                        // Проверяем ограничение надёжности
                        if (!IsConstraint_CalcSysReliability(time)) {

                            if (Form1.loggingOn)
                                fstream.Write("Ограничение не выполняется");

                            return false;
                        }

                        if (Form1.loggingOn)
                            fstream.Write("Ограничение выполняется");
                    }
            
            return true;
        }

        /// <summary>
        /// Конструктор выполняющий создание экземпляра данного класса 
        /// </summary>
        public SimplePreMSchedule(Config config, PreMConfig preMConfig) {
            this.config = config;
            this.preMConfig = preMConfig;

            SetLogFile("tmp.json");

            // Если флаг оталдки установлен
            if (IsDebug) {

                // Выводим информацию о переданной конфигурационной структуре
                fstream.Write($"{config.ToString()}");
            }

            startProcessing = new Dictionary<int, List<List<int>>>();
            matrixTPM = new List<List<PreMSet>>();
        }

        public override bool Build(List<List<int>> _matrixA)
        {

            return BuildWithoutLogging(_matrixA);

            // Если установлено логирование
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write("Начинаем выполнять операции на нижнем уровне;");

            List<List<int>> matrixA = ListUtils.MatrixIntDeepCopy(_matrixA);

            // Если установлено логирование
            if (Form1.loggingOn)
            {

                // Cоздаём экземпляр класса для работы со строками
                StringBuilder str = new StringBuilder(200);

                // Объявляем временную строку
                str.AppendLine($"Матрица A:");

                // Для каждого типа данных
                for (int _dataType = 0; _dataType < matrixA.Count(); _dataType++)
                {

                    // Добавляем новые данные в строку
                    str.Append($"\tТип {_dataType + 1}: ");

                    // Для каждого пакета в векторе типа _dataType матрицы A
                    for (int _batchIndex = 0; _batchIndex < matrixA[_dataType].Count(); _batchIndex++)

                        // Добавляем в строку данные
                        str.Append($"{matrixA[_dataType][_batchIndex]} ");

                    // Добавляем перевод строки
                    str.Append(Environment.NewLine);
                }

                // Записываем заголовок
                fstream.Write(str.ToString());
            }

            int dataType, maxBatchCount = 0, batch = 0, batchCount = 0;

            // Вычисляем максимальное количество пакетов среди всех типов данных
            calcMaxBatchCount();

            // Если установлено логирование
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write("maxBatchCount", maxBatchCount);

            // Вернёт максимальное количество пакетов среди всех типов данных
            void calcMaxBatchCount()
            {
                // Выполняем обработку по типам
                for (dataType = 0; dataType < this.config.dataTypesCount; dataType++)

                    // Выполняем поиск максимального количество пакетов
                    maxBatchCount = Math.Max(maxBatchCount, matrixA[dataType].Count);
            }

            Dictionary<int, double> m = new Dictionary<int, double>(capacity: this.config.dataTypesCount);
            List<int> dataTypes = new List<int>(capacity: this.config.dataTypesCount);
            for (dataType = 0; dataType < this.config.dataTypesCount; dataType++)
            {
                double sum = 0;
                for (int device = 1; device < this.config.deviceCount; device++)
                    sum +=
                        (double)this.config.proccessingTime[device][dataType] /
                        (double)this.config.proccessingTime[device - 1][dataType];
                m.Add(dataType, sum);
            }

            // Если установлено логирование
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write($"Типы данных:");

            while (m.Any())
            {
                int myDataType = m.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                dataTypes.Add(myDataType);
                // Если установлено логирование
                if (Form1.loggingOn)

                    // Записываем данные в файл
                    fstream.Write($"{myDataType}: {m[myDataType]}");
                m.Remove(myDataType);
            }


            // Если установлено логирование
            if (Form1.loggingOn)
            {

                // Выводим информацию
                fstream.Write("dataTypes:");

                // Для каждого типа
                for (int _dataType = 0; _dataType < this.config.dataTypesCount; _dataType++)

                    // Выводим информацию
                    fstream.Write($"{_dataType}: {dataTypes[_dataType]}");
            }

            // Сортируем матрицу A
            for (dataType = 0; dataType < this.config.dataTypesCount; dataType++)
                matrixA[dataType].Sort();

            // Если установлено логирование
            if (Form1.loggingOn)
            {

                // Cоздаём экземпляр класса для работы со строками
                StringBuilder str = new StringBuilder(200);

                // Объявляем временную строку
                str.AppendLine($"Матрица A:");

                // Для каждого типа данных
                for (int _dataType = 0; _dataType < matrixA.Count(); _dataType++)
                {

                    // Добавляем новые данные в строку
                    str.Append($"\tТип {_dataType + 1}: ");

                    // Для каждого пакета в векторе типа _dataType матрицы A
                    for (int _batchIndex = 0; _batchIndex < matrixA[_dataType].Count(); _batchIndex++)

                        // Добавляем в строку данные
                        str.Append($"{matrixA[_dataType][_batchIndex]} ");

                    // Добавляем перевод строки
                    str.Append(Environment.NewLine);
                }

                // Записываем заголовок
                fstream.Write(str.ToString());
            }

            batch = dataType = 0;

            // Объявляем количество пакетов заданий
            

            // Для каждого типа данных
            for (int _dataType = 0; _dataType < matrixA.Count(); _dataType++)

                // Увеличиваем общее количество пакетов заданий
                batchCount += matrixA[_dataType].Count();

            // П.2 Добавляем 
            this.schedule = new List<Batch>(batchCount) { new Batch(
                dataTypes[dataType],
                matrixA[dataTypes[dataType]][batch]
            )};
            dataType++;

            // Если логирование установлено
            if (Form1.loggingOn) {
                CalcStartProcessing();
                fstream.WriteScheduleAsTable(schedule);
            }
            // П.3 Инициализируем матрицу Y
            this.matrixY = new List<List<int>>(capacity: this.config.deviceCount);
            for (int device = 0; device < this.config.deviceCount; device++)
            {
                this.matrixY.Add(new List<int>());
                this.matrixY[device].Add(1);
            }
            // Если логирование установлено
            if (Form1.loggingOn)
            {
                fstream.WriteMatrixYAsTable(matrixY);
            }

            // Для каждого типа данных выполняем обрабоку
            for (; dataType < this.config.dataTypesCount; dataType++)
            {

                // Добавляем ПЗ в расписание 
                this.schedule.Add(new Batch(dataTypes[dataType], matrixA[dataTypes[dataType]][batch]));
                for (int device = 0; device < this.config.deviceCount; device++)
                    this.matrixY[device].Add(0);
                CalcStartProcessing();

                // Если логирование установлено
                if (Form1.loggingOn)
                {
                    fstream.WriteProcessingAsTable(startProcessing, matrixY, schedule, preMConfig);
                }

                // Если не было найдено расписания удовлетворяющему условию надёжности
                if (!this.SearchByPosition(5)) {

                    // Закрываем файл
                    UnsetLogFile();

                    // Возвращаем флаг неудачи
                    return false;
                }

                // Выполняем оптимизацию для позиций ПТО приборов
                // this.ShiftMatrixY();

                // Проверяем условие надёжности
                // if (!this.IsSolutionAcceptable()) {

                    // Закрываем файл
                    // UnsetLogFile();

                    // Возвращаем флаг неудачи
                    // return false;
                // }
            }

            // Увеличиваем индекс вставляемого пакета задания
            batch++;

            // Выполняем обработку
            while (batch < maxBatchCount)
            {

                // Выполняем обработку для каждого типа данных
                for (dataType = 0; dataType < this.config.dataTypesCount; dataType++)
                {

                    // Если индекс пакета превышает максимальный размер пакетов для типа данных dataType
                    if (batch >= matrixA[dataTypes[dataType]].Count)

                        // Продолжаем обработку для следующего типа данных
                        continue;

                    // Добавляем ПЗ в расписание 
                    this.schedule.Add(new Batch(dataTypes[dataType], matrixA[dataTypes[dataType]][batch]));
                    for (int device = 0; device < this.config.deviceCount; device++)
                        this.matrixY[device].Add(0);

                    // Если не было найдено расписания удовлетворяющему условию надёжности
                    if (!this.SearchByPosition(5)) {

                        // Закрываем файл
                        UnsetLogFile();

                        // Возвращаем флаг неудачи
                        return false;
                    }

                    // Выполняем оптимизацию для позиций ПТО приборов (ШАГ 15)
                    // this.ShiftMatrixY();

                    // Проверяем условие надёжности
                    // if (!this.IsSolutionAcceptable()) {

                        // Закрываем файл
                        // UnsetLogFile();

                        // Возвращаем флаг неудачи
                        // return false;
                    // }
                }

                // Увеличиваем индекс пакета
                batch++;
            }


            // Формируем матрицу со всеми единицами в ПТО
            // = new List<List<int>>(capacity: config.deviceCount);

            for (int device = 0; device < matrixY.Count(); device++)
                for (int batchIndex = 0; batchIndex < matrixY[device].Count(); batchIndex++)
                    matrixY[device][batchIndex] = 1;

            // Возвращяем флаг удачного построения расписания
            bool b = OptimizeMatrixY();

            // Если установлено логирование
            if (Form1.loggingOn)
            {

                // Записываем данные в файл
                fstream.Write("Начинаем выполнять операции на нижнем уровне");
            }

            return b;
        }

        private bool BuildWithoutLogging(List<List<int>> _matrixA)
        {

            List<List<int>> matrixA = ListUtils.MatrixIntDeepCopy(_matrixA);

            for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                matrixA[dataType].Sort();

            int batchCount = 0;

            for (int dataType = 0; dataType < matrixA.Count(); dataType++)
                batchCount += matrixA[dataType].Count();

            schedule = new List<Batch>(batchCount);

            matrixY = new List<List<int>>(capacity: config.deviceCount);
            for (int device = 0; device < config.deviceCount; device++)
            {
                matrixY.Add(new List<int>());
                matrixY[device].Add(1);
            }

            int maxBatchCount = 0;

            for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
                maxBatchCount = Math.Max(maxBatchCount, matrixA[dataType].Count);

            List<int> dataTypes = DataTypesInPriority();

            for (int batch = 0; batch < maxBatchCount; batch++)
            {
                foreach (int dataType in dataTypes)
                {
                    if (batch >= matrixA[dataType].Count)
                        continue;

                    bool success = AddBatchInSchedule(dataType, matrixA[dataType][batch]);
                    if(!success)
                    {
                        return false;
                    }
                }
            }

            for (int device = 0; device < config.deviceCount; device++)
                for (int batch = 0; batch < matrixY[device].Count(); batch++)
                    matrixY[device][batch] = 1;

            bool b = OptimizeMatrixY();
            return b;
        }

        private List<int> DataTypesInPriority()
        {
            Dictionary<int, double> m = new Dictionary<int, double>(capacity: config.dataTypesCount);

            for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
            {
                double sum = 0;
                for (int device = 1; device < config.deviceCount; device++)
                    sum +=
                        (double)config.proccessingTime[device][dataType] /
                        (double)config.proccessingTime[device - 1][dataType];
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

        private bool AddBatchInSchedule(int dataType, int size)
        {
            schedule.Add(new Batch(dataType, size));
            for (int device = 0; device < config.deviceCount; device++)
                matrixY[device].Add(0);

            CalcStartProcessing();

            // Если не было найдено расписания удовлетворяющему условию надёжности
            if (!SearchByPosition(5))
            {
                UnsetLogFile();
                return false;
            }
            return true;
        }

        private bool OptimizeMatrixY()
        {
            if (Form1.loggingOn)
            {
                fstream.Write("OptimizeMatrixY::start");
                fstream.WriteMatrixYAsTable(matrixY);
            }

            CalcStartProcessing();

            if (!IsSolutionAcceptable())
            {
                if (Form1.loggingOn)
                {
                    fstream.Write("OptimizeMatrixY::result");
                    fstream.Write("OptimizeMatrixY::BAD");
                    fstream.WriteMatrixYAsTable(matrixY);
                }

                return false;
            }

            int device = 0;
            for (int batchIndex = 0; batchIndex < matrixY[device].Count() - 1; batchIndex++)
            {
                for (; device < matrixY.Count(); device++)
                {

                    if (Form1.loggingOn)
                    {
                        fstream.Write("OptimizeMatrixY::switch-0");
                        fstream.WriteMatrixYAsTable(matrixY);
                    }

                    matrixY[device][batchIndex] = 0;
                    CalcStartProcessing();

                    if (!IsSolutionAcceptable())
                    {
                        if (Form1.loggingOn)
                        {
                            fstream.Write("OptimizeMatrixY::switch-1-BAD");
                            fstream.WriteMatrixYAsTable(matrixY);
                        }
                        matrixY[device][batchIndex] = 1;
                    }
                    else if (Form1.loggingOn)
                    {
                        fstream.Write("OptimizeMatrixY::switch-0-GOOD");
                        fstream.WriteMatrixYAsTable(matrixY);
                    }
                }
                device = 0;
            }

            if (Form1.loggingOn)
            {
                fstream.Write("OptimizeMatrixY::result");
                fstream.Write("OptimizeMatrixY::GOOD");
                fstream.WriteMatrixYAsTable(matrixY);
            }

            return true;
        }

        /// <summary>
        /// Выполняет построение матрицы моментов окончания времени выполнения ПТО.
        /// </summary>
        public void CalcMatrixTPM()
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn) 

                // Записываем данные в файл
                fstream.Write("Вычисляем матрицу T^pm;");
            
            // Отчищяем матрицу T^pm
            matrixTPM?.Clear();

            for (int device = 0; device < config.deviceCount; device++)
            {

                // Инициализируем ПТО для прибора
                matrixTPM.Add(new List<PreMSet>());

                for (int batchIndex = 0; batchIndex < schedule.Count; batchIndex++)

                    // Если для текущей позиции есть ПТО
                    if (matrixY[device][batchIndex] == 1)

                        // Момент окончания времени выполнения ПТО на позиции batchIndex
                        matrixTPM[device].Add(

                            // Добавляем структуры данных
                            new PreMSet(

                                // Индекс ПЗ после которого будет выполняться ПТО
                                batchIndex,

                                // Момент начала времени выполнения последнего задания в пакете batchIndex на приборе device
                                startProcessing[device][batchIndex].Last() +

                                // Время выполнения задания с типов пакета на позиции batchIndex
                                config.proccessingTime[device][schedule[batchIndex].Type] +

                                // Время выполнения ПТО
                                preMConfig.preMaintenanceTimes[device]
                            )
                        );
            }
        }

        // ВЫРАЖЕНИЯ 1-6
        /// <summary>
        /// Выполняет построение матрицы начала времени выполнения заданий
        /// </summary>
        private void CalcStartProcessing()
        {

            if (Form1.loggingOn)
                fstream.Write($"Вычисляем матрицу T^0l");

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
            startProcessing[device][batch][job] = config.changeoverTime[device][schedule[batch].Type][schedule[batch].Type];

            // Пробегаемся по всем заданиям пакета в первой позиции
            for (job = 1; job < schedule[batch].Size; job++)

                // Устанавливаем момент начала времени выполнения задания job
                startProcessing[device][batch][job] =

                    // Момент начала времени выполнения предыдущего задания
                    startProcessing[device][batch][job - 1] +

                    // Время выполнения предыдущего задания
                    config.proccessingTime[device][schedule[batch].Type];

            // Пробегаемся по всем возможным позициям cо второго пакета
            for (batch = 1; batch < schedule.Count(); batch++)
            {

                job = 0;

                // Момент начала времени выполнения 1 задания в пакете на позиции batch
                startProcessing[device][batch][job] =

                    // Момент начала времени выполнения последнего задания в предыдущем пакете
                    startProcessing[device][batch - 1].Last() +

                    // Время выполнения задания в предыдущем пакете
                    config.proccessingTime[device][schedule[batch - 1].Type] +

                    // Время переналадки с предыдущего типа на текущий
                    config.changeoverTime[device][schedule[batch - 1].Type][schedule[batch].Type] +

                    // Время выполнения ПТО после предыдущего ПЗ
                    preMConfig.preMaintenanceTimes[0] * matrixY[device][batch - 1];

                for (job = 1; job < schedule[batch].Size; job++)

                    // Вычисляем момент начала времени выполнения задания job в позиции batch на 1 приборе
                    startProcessing[device][batch][job] =

                        // Момент начала времени выполнения предыдущего задания
                        startProcessing[device][batch][job - 1] +

                        // Время выполнения предыдущего задания
                        config.proccessingTime[device][schedule[batch].Type];
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
                config.changeoverTime[device][schedule[batch].Type][schedule[batch].Type],

                // Время окончания выполнения 1 задания в 1 пакете на предыдущем приборе
                startProcessing[device - 1][batch][job] + config.proccessingTime[device - 1][schedule[batch].Type]
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
                    config.proccessingTime[device][schedule[batch].Type],

                    // Момент начала времени выполнения текущего задания на предыдущем приборе
                    startProcessing[device - 1][batch][job] +

                    // Время выполнения текущего задания на предыдущем приборе
                    config.proccessingTime[device - 1][schedule[batch].Type]
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
                    config.proccessingTime[device][schedule[batch - 1].Type] +

                    // Время переналадки с предыдущего типа на текущий
                    config.changeoverTime[device][schedule[batch - 1].Type][schedule[batch].Type] +

                    // Время выполнения ПТО
                    preMConfig.preMaintenanceTimes[device] * matrixY[device][batch - 1],

                    // Момент начала времени выполнения 1 задания на предыдущем приборе
                    startProcessing[device - 1][batch][job] +

                    // Время выполнения 1 задания на предыдущем приборе
                    config.proccessingTime[device - 1][schedule[batch].Type]);

                // Пробегаемся по всем возможным заданиям пакета в позиции batchIndex
                for (job = 1; job < schedule[batch].Size; job++)

                    // Устанавливаем момент начала времени выполнения текущего задания job, как
                    // Максимум, между временем окончания предыдущего задания на текущем приборе и
                    // временем окончания текущего задания на предыдущем приборе
                    startProcessing[device][batch][job] = Math.Max(

                        // Момент начала времени выполнения предыдущего задания
                        startProcessing[device][batch][job - 1] +

                        // Время выполнения предыдущего задания
                        config.proccessingTime[device][schedule[batch].Type],

                        // Момент начала времени выполнения задания на предыдущем приборе
                        startProcessing[device - 1][batch][job] +

                        // Время выполнения задания на предыдущем приборе
                        config.proccessingTime[device - 1][schedule[batch].Type]
                    );
            }
        }

        // ВЫРАЖЕНИЕ 7
        /// <summary>
        /// Возвращает простои для переданного индекса прибора, данного расписания
        /// </summary>
        /// <returns>Время простоя для переданного индекса прибора</returns>
        private int GetDowntimeByDevice(int device)
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write($"Вычисляем простои для прибора {device}");

            // Объявляем и инициализируем простои
            int downtime = 0;

            // Подсчитываем простои связанные с наладкой
            downtime += startProcessing[device].First().First();

            // Для кажого задания пакета на первой позиции
            for (int job = 1; job < startProcessing[device].First().Count(); job++)

                // Подсчитываем простои между заданиями
                downtime +=

                    // Момент начала времени выполнения текущего задания
                    startProcessing[device][0][job] -

                    // Момент окончания времени выполнения предыдущего задания
                    (
                        // Момент начала времени выполнения предыдущего задания
                        startProcessing[device][0][job - 1] +

                        // Время выполнения предыдущего задания
                        config.proccessingTime[device][schedule[0].Type]
                    );

            // Для каждого пакета со второго выполняем обработку
            for (int batchIndex = 1; batchIndex < startProcessing[device].Count(); batchIndex++)
            {

                // Подсчитываем простои между пакетами
                downtime +=

                    // Момент начала времени выполнения первого задания текущего пакет
                    startProcessing[device][batchIndex][0] -

                    // Момент начала времени выполнения последнего задания на предыдущем пакете
                    (startProcessing[device][batchIndex - 1].Last() +

                    // Время выполнения задания в предыдущем пакете
                    config.proccessingTime[device][schedule[batchIndex - 1].Type]);

                // Для кажого задания пакета на первой позиции
                for (int job = 1; job < startProcessing[device][batchIndex].Count(); job++)

                    // Подсчитываем простои между заданиями
                    downtime +=

                        // Момент начала времени выполнения текущего задания
                        startProcessing[device][batchIndex][job] -

                        // Момент окончания времени выполнения предыдущего задания
                        (
                            // Момент начала времени выполнения предыдущего задания
                            startProcessing[device][batchIndex][job - 1] +

                            // Время выполнения предыдущего задания
                            config.proccessingTime[device][schedule[batchIndex].Type]
                        );
            }
            
            // Возвращаем результат
            return downtime;
        }

        // ВЫРАЖЕНИЕ 8 ДЛЯ ВСЕХ ПРИБОРОВ
        /// <summary>
        /// Возвращает общие простои для данного расписания
        /// </summary>
        /// <returns>Время простоя</returns>
        public int GetDowntime()
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn && fstream != null)

                // Записываем данные в файл
                fstream.Write("Вычисляем простои для всех приборов");

            // Объявляем и инициализируем простои
            int downtime = 0;

            // Для каждого прибора выполняем обработку
            for (int device = 0; device < config.deviceCount; device++)
            {

                // Подсчитываем простои связанные с наладкой
                downtime += startProcessing[device].First().First();

                // Для кажого задания пакета на первой позиции
                for (int job = 1; job < startProcessing[device].First().Count(); job++)

                    // Подсчитываем простои между заданиями
                    downtime +=

                        // Момент начала времени выполнения текущего задания
                        startProcessing[device][0][job] -

                        // Момент начала времени выполнения предыдущего задания
                        (startProcessing[device][0][job - 1] +

                        // Время выполнения предыдущего задания
                        config.proccessingTime[device][schedule[0].Type]);

                // Для каждого пакета со второго выполняем обработку
                for (int batchIndex = 1; batchIndex < startProcessing[device].Count(); batchIndex++)
                {

                    // Подсчитываем простои между пакетами
                    downtime +=

                        // Момент начала времени выполнения первого задания текущего пакет
                        startProcessing[device][batchIndex][0] -

                        // Момент начала времени выполнения последнего задания на предыдущем пакете
                        (startProcessing[device][batchIndex - 1].Last() +

                        // Время выполнения задания в предыдущем пакете
                        config.proccessingTime[device][schedule[batchIndex - 1].Type]);

                    // Для кажого задания пакета на первой позиции
                    for (int job = 1; job < startProcessing[device][batchIndex].Count(); job++)

                        // Подсчитываем простои между заданиями
                        downtime +=

                            // Момент начала времени выполнения текущего задания
                            startProcessing[device][batchIndex][job] -

                            // Момент начала времени выполнения предыдущего задания
                            (startProcessing[device][batchIndex][job - 1] +

                            // Время выполнения предыдущего задания
                            config.proccessingTime[device][schedule[batchIndex].Type]);
                }
            }

            // Возвращаем результат
            return downtime;
        }

        /// <summary>
        /// Возвращает общие простои для данного расписания без ПТО
        /// </summary>
        /// <returns>Время простоя</returns>
        public int GetDowntimeWithoutPreM()
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn && fstream != null)

                // Записываем данные в файл
                fstream.Write("Вычисляем простои для всех приборов");

            // Объявляем и инициализируем простои
            int downtime = 0;

            // Для каждого прибора выполняем обработку
            for (int device = 0; device < config.deviceCount; device++)
            {

                // Подсчитываем простои связанные с наладкой
                downtime += startProcessing[device].First().First();

                // Для кажого задания пакета на первой позиции
                for (int job = 1; job < startProcessing[device].First().Count(); job++)

                    // Подсчитываем простои между заданиями
                    downtime +=

                        // Момент начала времени выполнения текущего задания
                        startProcessing[device][0][job] -

                        // Момент начала времени выполнения предыдущего задания
                        (startProcessing[device][0][job - 1] +

                        // Время выполнения предыдущего задания
                        config.proccessingTime[device][schedule[0].Type]);

                // Для каждого пакета со второго выполняем обработку
                for (int batchIndex = 1; batchIndex < startProcessing[device].Count(); batchIndex++)
                {

                    // Подсчитываем простои между пакетами
                    downtime +=

                        // Момент начала времени выполнения первого задания текущего пакет
                        startProcessing[device][batchIndex][0] -

                        // Момент начала времени выполнения последнего задания на предыдущем пакете
                        (startProcessing[device][batchIndex - 1].Last() +

                        // Время выполнения задания в предыдущем пакете
                        config.proccessingTime[device][schedule[batchIndex - 1].Type]);

                    // Для кажого задания пакета на первой позиции
                    for (int job = 1; job < startProcessing[device][batchIndex].Count(); job++)

                        // Подсчитываем простои между заданиями
                        downtime +=

                            // Момент начала времени выполнения текущего задания
                            startProcessing[device][batchIndex][job] -

                            // Момент начала времени выполнения предыдущего задания
                            (startProcessing[device][batchIndex][job - 1] +

                            // Время выполнения предыдущего задания
                            config.proccessingTime[device][schedule[batchIndex].Type]);
                }
            }

            // Вычитаем простои связанные с ПТО
            for (int device = 0; device < config.deviceCount; device++)
                for (int batchIndex = 0; batchIndex < matrixY[device].Count() - 1; batchIndex++)
                    downtime -= matrixY[device][batchIndex] * preMConfig.preMaintenanceTimes[device];

            // Возвращаем результат
            return downtime;
        }

        /// <summary>
        /// Функция выполняет подсчёт критерий F2
        /// </summary>
        /// <returns>f2 критерий</returns>
        public int CalculateCriteria_f2(double downtimeC = 1, double premC = 1)
        {
            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn && fstream != null)
            {

                // Записываем данные в файл
                fstream.Write("Вычисляем критерий f2;");

                // Выводим информацию о матрице начал моментов времени выполнения
                fstream.WriteProcessingAsTable(startProcessing, matrixY, schedule, preMConfig);
            }

            // Добавляем простои
            int f2 = 0;
            int prem = 0;

            // Высчитываем простои связанные с ПТО
            for (int device = 0; device < config.deviceCount; device++)
                for (int batchIndex = 0; batchIndex < matrixY[device].Count(); batchIndex++)
                    prem += matrixY[device][batchIndex] * preMConfig.preMaintenanceTimes[device];
            f2 += GetDowntimeWithoutPreM(); // * downtimeC
            f2 += prem; // * premC

            // Возвращяем сигма критерий
            return f2;
        }

        // ВЫРАЖЕНИЕ 8 ДЛЯ ОДНОГО ПРИБОРА
        /// <summary>
        /// Возвращаем полезность для прибора по переданному индексу
        /// </summary>
        /// <returns>Критерий полезности</returns>
        private int GetUtilityByDevice(int device)
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write($"Вычисляем полезность для прибора {device}");

            // Объявляем значение критерия на нижнем уровне
            int sum = 0;

            // Добавляем момент времени окончания всех заданий на приборе
            sum +=

                // Момент начала времени выполнения на последнем задании в последнем пакете
                this.startProcessing[device].Last().Last() +

                // Время выполнения последнего заданий в последенем пакете
                this.config.proccessingTime[device][this.schedule.Last().Type];

            // Вычитаем простои
            sum -= this.GetDowntimeByDevice(device);

            // Возвращаем критерий
            return sum;
        }

        // ВЫРАЖЕНИЕ 8
        /// <summary>
        /// Возвращаем полезность для данного расписания
        /// </summary>
        /// <returns>Время полезности</returns>
        public int GetUtility()
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write("Вычисляем полезность для всех приборов");

            // Объявляем значение критерия на нижнем уровне
            int sum = 0;

            // Для каждого прибора выполняем обработку
            for (int device = 0; device < this.config.deviceCount; device++)
            
                // Добавляем критерий для данного прибора
                sum += this.GetUtilityByDevice(device);
            
            // Возвращаем критерий
            return sum;
        }

        // ВЫРАЖЕНИЕ 9
        /// <summary>
        /// Возвращает сумму полезности и интервалов между ПТО для данного расписания
        /// </summary>
        /// <returns>Сумма полезности и интервалов между ПТО</returns>
        public int GetPreMUtility()
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn) {

                // Записываем данные в файл
                fstream.Write("Вычисляем полезность с учётом ПТО для всех приборов");

                // Выводим информацию о матрице начал моментов времени выполнения
                fstream.WriteProcessingAsTable(startProcessing, matrixY, schedule, preMConfig);

                // Выводим информацию
                fstream.Write("GetPreMUtility start: Вычисляем сумму полезности и итервалов между ПТО");
            }

            // Объявляем значение критерия на нижнем уровне
            int sum = 0;

            // Для каждого прибора выполняем обработку
            for (int device = 0; device < config.deviceCount; device++)
            {

                // Если логирование установлено
                if (Form1.loggingOn) {
                    fstream.Write("device", device);
                    fstream.Write($"Момент окончания последнего задания { this.startProcessing[device].Last().Last() + this.config.proccessingTime[device][this.schedule.Last().Type] }");
                }

                // Добавляем момент времени окончания всех заданий на приборе
                sum +=
                    
                    // Момент начала времени выполнения на последнем задании в последнем пакете
                    this.startProcessing[device].Last().Last() +

                    // Время выполнения последнего заданий в последенем пакете
                    this.config.proccessingTime[device][this.schedule.Last().Type];

                // Если логирование установлено
                if (Form1.loggingOn)
                    fstream.Write($"Простои для данного прибора с учётом ПТО { this.GetDowntimeByDevice(device) }");

                // Вычитаем простои
                sum -= this.GetDowntimeByDevice(device);

                int intervals = matrixTPM[device].First().TimePreM;

                // Для каждого пакета выполняем обработку
                for (int batchIndex = 1; batchIndex < matrixTPM[device].Count(); batchIndex++)

                    // Добавляем интервалы времени между ПТО разных пакетов
                    intervals += matrixTPM[device][batchIndex].TimePreM - matrixTPM[device][batchIndex - 1].TimePreM;

                // Если логирование установлено
                if (Form1.loggingOn)
                    fstream.Write("Интервалы времени между ПТО", intervals);
                
                // Выполняем подсчёт суммы интервалов времени на первом пакете ПТО
                sum += intervals;
            }

            // Если логирование установлено
            if (Form1.loggingOn)
                fstream.Write("Критерий f2", sum);

            // Возвращаем критерий
            return sum;
        }

        // ВЫРАЖЕНИЕ 10
        /// <summary>
        /// Возвращает надёжность, которая определяет вероятность находится ли некий прибор в работоспособном состоянии
        /// </summary>
        /// <param name="activity_time">Время активности прибора с момента последнего ПТО</param>
        /// <param name="device">Индекс прибора для которого расчитывается надёжность</param>
        /// <returns>Надёжность прибора по индексу device</returns>
        private double CalcReliabilityByDevice(int activity_time, int device)
        {

            if (Form1.loggingOn)
                fstream.Write($"Вычисляем надёжность для прибора {device} с временем активности {activity_time}");

            // Выполняем расчёт и возврат доступности
            return (activity_time == 0) ? 1 :
                (double) preMConfig.restoringDevice[device] / (double)(preMConfig.failureRates[device] + preMConfig.restoringDevice[device]) +
                (double) preMConfig.failureRates[device]    / (double)(preMConfig.failureRates[device] + preMConfig.restoringDevice[device]) *
                (double) Math.Exp(-1 * (double)(preMConfig.failureRates[device] + preMConfig.restoringDevice[device]) * (double)activity_time);
        }

        // ВЫРАЖЕНИЕ 11
        /// <summary>
        /// Возвращает надёжность, которая определяет вероятность находится ли некий прибор в работоспособном состоянии
        /// </summary>
        /// <param name="activity_time">Время активности прибора с момента старта КС</param>
        /// <param name="prem_time">Момент времени окончания последнего ПТО</param>
        /// <param name="device">Индекс прибора для которого расчитывается надёжность</param>
        /// <returns>Надёжность прибора по индексу device</returns>
        // private double CalcReliabilityByDevice(int activity_time, int prem_time, int device)
        // {
        // 
        //     // Выполняем расчёт и возврат доступности по выражению 10
        //     return CalcReliabilityByDevice(activity_time - prem_time, device);
        // }

        // Выражение 12
        
        /// <summary>
        /// Функция вернёт время активности прибора от предыдущего ПТО
        /// </summary>
        /// <param name="device">Индекс прибора для которого расчитывается время активности</param>
        /// <param name="time">Крайний момент времени</param>
        /// <returns>Время активности</returns>
        private int GetActivityTimeByDevice(int device, int time)
        {

            if (Form1.loggingOn)
                fstream.Write($"Вычисляем временя активности для прибора {device + 1} для момента времени {time};");

            // Определяем начальный индекс
            int batchIndex = GetBatchIndex(device, time) + 1;

            // Определяем время активности
            int activityTime = 0;

            // Для каждого пакет выполняем обработку
            for (; batchIndex < schedule.Count; batchIndex++)

                // Для каждого задания выполняем обработку
                for (int job = 0; job < startProcessing[device][batchIndex].Count; job++)
                {

                    // Если момент начала времени выполнения выходит за границу
                    if (startProcessing[device][batchIndex][job] >= time)

                        // Вернём время активности
                        return activityTime;

                    // Высчитываем время выполнения
                    int proc_time = config.proccessingTime[device][schedule[batchIndex].Type];

                    // Высчитываем момент начала времени выполнения
                    int start_time = startProcessing[device][batchIndex][job];

                    // Если момент окончания задания выходит за указанные границы
                    if (start_time + proc_time > time)
                    {
                        // Увеличиваем время активности до прибора
                        activityTime += time - start_time;
                        return activityTime;
                    }

                    // Увеличиваем время активности прибора
                    activityTime += proc_time;
                }

            // Возвращаем время активности
            return activityTime;
        }

        // ВЫРАЖЕНИЕ 13

        /// <summary>
        /// Возвращает доступность для всех приборов для указанного момента времени
        /// </summary>
        /// <param name="time">Момент времени для которого выполняется расчёт надёжности</param>
        /// <returns>Доступность для всех приборов</returns>
        private double CalcSysReliability(int time)
        {

            if (Form1.loggingOn)

                fstream.Write($"Вычисляем системную надёжность для момента времени {time}");
            
            double reliability = 1;

            // Объявляем время активности
            int activity_time;

            // Для каждого прибора подсчитываем надёжность
            for (int device = 0; device < config.deviceCount; device++) {

                activity_time = GetActivityTimeByDevice(device, time);

                if (Form1.loggingOn)
                    fstream.Write($"\tДля прибора {device} время активности {activity_time} и надёжность {CalcReliabilityByDevice(activity_time, device):0.000}");
                
                // Если прибор не был активным
                if (activity_time == 0)
                    continue;

                // Выполняем расчёт надёжности
                reliability *= CalcReliabilityByDevice(activity_time, device);
            }

            return reliability;
        }

        // ВЫРАЖЕНИЕ 14
        public override int GetMakespan()
        {
            return startProcessing[config.deviceCount - 1].Last().Last() + config.proccessingTime[config.deviceCount - 1][schedule.Last().Type];
        }

        // ВЫРАЖЕНИЕ 15
        /// <summary>
        /// Возвращает результат совпадения количества заданий
        /// </summary>
        /// <param name="matrixA">Матрица количества заданий каждого типа на пакет[dataTypesCount x mi]</param>
        /// <returns>Если количество заданий совпадают - true, иначе false</returns>
        private bool IsConstraint_JobCount(List<List<int>> matrixA)
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write("Проверяем ограничение 15");

            // Объявляем количество заданий для текущего расписания
            int cur_jobCount = 0;

            // Объявляем необходимое количество заданий 
            int tar_jobCount = 0;

            // Для каждого пакета подсчитываем количество заданий
            for (int batch = 0; batch < this.schedule.Count; batch++)
            
                // Увеличиваем количество заданий в текущем расписании
                cur_jobCount += this.schedule[batch].Size;
            
            // Выполняем обход по типам
            for (int dataType = 0; dataType < matrixA.Count; dataType++)
            

                // Выполняем обход по пакетам
                for (int batch = 0; batch < matrixA[dataType].Count; batch++)

                    // Увеличиваем количество необходимых заданий
                    tar_jobCount += matrixA[dataType][batch];

            // Возвращаем результат сравнения
            return (cur_jobCount == tar_jobCount);
        }

        // ВЫРАЖЕНИЕ 16 = 9

        // ВЫРАЖЕНИЕ 17 = 11

        // ВЫРАЖЕНИЕ 18
        
        /// <summary>
        /// Возвращаем результат расчёта ограничения на общую надёжность
        /// </summary>
        /// <param name="time">Момент времени для которого выполняется расчёт надёжности</param>
        /// <returns>true, если ограничение выполняется. Иначе false</returns>
        private bool IsConstraint_CalcSysReliability(int time)
        {

            if (Form1.loggingOn)

                fstream.Write($"Проверяем ограничение 16 для момента времени {time};");

            double sysTime = CalcSysReliability(time);
            return (sysTime >= preMConfig.beta);
        }

        // ВЫРАЖЕНИЕ 19 ИЗБЫТОЧНО

        // ВЫРАЖЕНИЕ 20 
        
        /// <summary>
        /// Возвращает индекс последнего ПЗ после которого выполняется ПТО до заданного времени
        /// </summary>
        /// <param name="device">Индекс прибора по которому будет выполняться выборка</param>
        /// <param name="time">Крайний момент времени окончания ПТО</param>
        /// <returns>Индекс ПЗ после которого будет выполняться последнее ПТО</returns>
        private int GetBatchIndex(int device, int time)
        {
            
            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write($"Вычисляем индекс последнего ПЗ для прибора {device} для момента времени {time}");

            // Если список пустой
            if (matrixTPM[device].Count == 0)

                // Вернём индекс начальный индекс ПЗ
                return -1;

            // Если в списке первый элемент не удовлетворяет условию
            // TODO: Баг, определяет пакет по окончанию ПТО, когда необходимо по началу ПТО.
            if (matrixTPM[device][0].TimePreM > time)

                // Вернём индекс начальный индекс ПЗ
                return -1;

            // Если в списке первый элемент не удовлетворяет условию
            if (matrixTPM[device][0].TimePreM == time)

                // Вернём индекс начальный индекс ПЗ
                return matrixTPM[device][0].BatchIndex;

            // Объявляем индекс
            int index = 1;

            // Для каждой ПТО выполняем обработку
            for (; index < matrixTPM[device].Count; index++)
            {

                // Если момент окончания ПТО в позиции index не удовлетворяет условиям
                if (matrixTPM[device][index].TimePreM > time)

                    // Возвращаем индекс ПЗ после которого выполнится последнее ПТО
                    return matrixTPM[device][index - 1].BatchIndex;

                // Если момент окончания ПТО в позиции index не удовлетворяет условиям
                if (matrixTPM[device][index].TimePreM == time)

                    // Возвращаем индекс ПЗ после которого выполнится последнее ПТО
                    return matrixTPM[device][index].BatchIndex;
            }

            // Индекс ПЗ после которог выполниться ПТО, последний в списке
            return matrixTPM[device][index - 1].BatchIndex;
        }

        // ВЫРАЖЕНИЕ 21 = 12

        // ВЫРАЖЕНИЕ 22 ИЗБЫТОЧНО

        // ВЫРАЖЕНИЕ 23
        
        /// <summary>
        /// Возвращает результат совпадения количества пакетов заданий
        /// </summary>
        /// <param name="matrixA">Матрица количества заданий каждого типа на пакет[dataTypesCount x mi]</param>
        /// <returns>Если количество пакетов заданий совпадают - true, иначе false</returns>
        private bool IsConstraint_BatchCount(List<List<int>> matrixA)
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write("Проверяем ограничение 21");

            // Объявляем количество пакетов заданий
            int cur_batchCount = this.schedule.Count;

            // Объявляем необходимое количество пакетов заданий 
            int tar_batchCount = 0;

            // Выполняем обход по типам
            for (int dataType = 0; dataType < matrixA.Count; dataType++)
            
                // Увеличиваем количество пакетов
                tar_batchCount += matrixA[dataType].Count;

            // Возвращаем true
            return (cur_batchCount == tar_batchCount);
        }

        // ВЫРАЖЕНИЕ 24
        /// <summary>
        /// Возвращаем результат совпадения одного пакета на позицию расписания
        /// </summary>
        /// <returns>Если пакет на позицию 1, то true. Иначе false</returns>
        private bool IsConstraint_OneBatchOnPos()
        {
            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write($"Проверяем ограничение 24");

            // Существующая реализация расписания обязывает иметь один пакет на позицию
            return true;
        }

        // ВЫРАЖЕНИЕ 25
        /// <summary>
        /// Возвращает результат совпадения количества пакетов заданий каждого типа
        /// </summary>
        /// <param name="matrixA">Матрица количества заданий каждого типа на пакет[dataTypesCount x mi]</param>
        /// <returns>Если количество пакетов заданий по типам совпадают - true, иначе false</returns>
        private bool IsConstraint_BatchCountByType(List<List<int>> matrixA)
        {

            // Если установлено логирование и объект для записи существует
            if (Form1.loggingOn)

                // Записываем данные в файл
                fstream.Write("Проверяем ограничение 25");

            // Объявляем количество пакетов заданий
            int cur_batchCountByType;

            // Объявляем необходимое количество пакетов заданий 
            int tar_batchCountByType;

            // Выполняем обход по типам
            for (int dataType = 0; dataType < matrixA.Count; dataType++)
            {

                // Увеличиваем количество пакетов
                tar_batchCountByType = matrixA[dataType].Count;

                // Обнуляем значение количества пакето заданий заданного типа
                cur_batchCountByType = 0;

                // Для каждого пакета заданий выполняем обработку
                for (int batch = 0; batch < this.schedule.Count; batch++)

                    // Увеличиваем количество пакетов заданий текущего типа
                    cur_batchCountByType += (this.schedule[batch].Type == dataType) ? 1 : 0;

                // Выполяем проверку
                if (tar_batchCountByType != cur_batchCountByType)

                    // Возвращаем false
                    return false;
            }

            // Возвращаем true
            return true;
        }

        /// <summary>
        /// Вернёт тип данных по переданному индексу ПЗ
        /// </summary>
        /// <param name="batchIndex">Индекс ПЗ</param>
        /// <returns>Тип данных</returns>
        public int GetDataTypeByBatchIndex(int batchIndex)
        {
            return this.schedule[batchIndex].Type;
        }

        public override List<List<int>> GetMatrixP()
        {

            // Объявляем матрицу
            List<List<int>> res = new List<List<int>>(config.dataTypesCount);

            // Инициализируем матрицу
            for (int dataType = 0; dataType < this.config.dataTypesCount; dataType++)

                // Инициализируем строку матрицы нулями
                res.Add(ListUtils.InitVectorInt(this.schedule.Count));

            // Для каждого элемента матрицы schedule
            for (int batchIndex = 0; batchIndex < this.schedule.Count; batchIndex++)

                // Заполняем элементы матрицы количества заданий в пакетах
                res[this.schedule[batchIndex].Type][batchIndex] = 1;

            // Возвращаем результат
            return res;
        }

        public override List<List<int>> GetMatrixR()
        {
            
            // Объявляем матрицу
            List<List<int>> res = new List<List<int>>(this.config.dataTypesCount);

            // Инициализируем матрицу
            for (int dataType = 0; dataType < config.dataTypesCount; dataType++)
            
                // Инициализируем строку матрицы нулями
                res.Add(ListUtils.InitVectorInt(schedule.Count));

            // Для каждого элемента матрицы schedule
            for (int batchIndex = 0; batchIndex < this.schedule.Count; batchIndex++)

                // Заполняем элементы матрицы количества заданий в пакетах
                res[this.schedule[batchIndex].Type][batchIndex] = this.schedule[batchIndex].Size;

            // Возвращаем результат
            return res;
        }

        public override List<List<int>> GetMatrixTPM()
        {

            CalcMatrixTPM();

            // Объявляем матрицу
            List<List<int>> res = new List<List<int>>(matrixTPM.Count);

            // Инициализируем матрицу
            for (int device = 0; device < config.deviceCount; device++) {

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

        public override List<List<int>> GetMatrixY()
        {
            return matrixY;
        }

        public override Dictionary<int, List<List<int>>> GetStartProcessing()
        {
            CalcStartProcessing();
            return startProcessing;
        }
    }
}
