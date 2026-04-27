using magisterDiplom;
using magisterDiplom.Model;
using magisterDiplom.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using magisterDiplom.Model.Configuration;

namespace newAlgorithm
{

    public partial class Form1 : Form
    {

        #region Поля данных

        /// <summary>
        /// Данная переменная отвечает за выбор способа обработки данных и определено перечислением SelectionType
        /// </summary>
        private SelectoinType selectoinType;

        /// <summary>
        /// Данная переменная определяет количество данных каждого типа
        /// </summary>
        int batchCount;

        /// <summary>
        /// Данная переменная определяет размер буфера перед приборами
        /// </summary>
        int buffer;

        /// <summary>
        /// Данная переменная определяет значения для генерации рандомных данных
        /// </summary>
        int randomValue;

        /// <summary>
        /// Данная переменная определяет фиксированные ли партии
        /// </summary>
        bool isFixedBatches;

        /// <summary>
        /// Данная переменная определяет использование оптимизации
        /// </summary>
        bool isOptimization;

        /// <summary>
        /// TODO: Написать описание
        /// </summary>
        int generationCount;

        /// <summary>
        /// Данная переменная определяет размер списка из хромосом
        /// </summary>
        int xromossomiSize;

        /// <summary>
        /// Переменная сообщает программе о необходимости визуализации процесса с помощью Excel
        /// </summary>
        public static bool vizualizationOn;

        /// <summary>
        /// Переменная сообщает программе о необходимости генерации лог файлов
        /// </summary>
        public static bool loggingOn;

        /// <summary>
        /// Переменная сообщает программе о необходимости отрисовки диаграммы Ганта
        /// </summary>
        public static bool gantaOn;

        /// <summary>
        /// Переменная сообщает программе о необходимости отображения информации о не найденных решениях
        /// </summary>
        public static bool showND;

        // TODO: разобраться со статическими типами данных
        public static int buff;
        public static bool direct;
        public static List<List<int>> compositionSets;
        public static List<List<int>> timeSets;

        #endregion

        /// <summary>
        /// Данный конструктор выполняет инициализацию данных для работы формы
        /// </summary>
        public Form1()
        {

            // Вызываем сгенерированную функцию инициализации графических компонентов
            InitializeComponent();

            // Вызываем самописную функцию инициализации данных и графических компонентов
            InitializeForm();
        }

        #region Обработка событий

        #region Обработчики нажатия кнопок

        /// <summary>
        /// Данная функция вызывается при нажание на кнопку "Второй метод"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button3_Click(object sender, EventArgs e)
        {

            // Выполняем перестроение матриц
            TablesRebuild();

            // TODO: Перед вызовом данной функции необходимо выполнить присваивание для Form1.compositionSets и Form1.timeSets

            // Инициализируем вектор длиной dataTypesCount, каждый элемент которого будет равен batchCount
            List<int> batchCountList = CreateBatchCountList();

            // Создаём экземпляр конфигурационной структуры
            Config config = GetConfig();

            // Инициализируем расписание
            Shedule.deviceCount = config.deviceCount;
            Shedule.proccessingTime = config.proccessingTime;
            Shedule.changeoverTime = GetChangeoverTimeList();

            // Формируем первый уровень
            var firstLevel = new FirstLevel(config, batchCountList);

            // Выполняем генерацию данных для ввсех типов вторым алгоритмом
            firstLevel.GenetateSolutionForAllTypesSecondAlgorithm();
        }

        /// <summary>
        ///  Данная функция вызывается при нажание на кнопку "Получить решение"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {

            // Выполняем перестроение матриц
            TablesRebuild();

            // Инициализируем вектор длиной dataTypesCount, каждый элемент которого будет равен batchCount
            List<int> batchCountList = CreateBatchCountList();

            // Создаём экземпляр конфигурационной структуры
            Config config = GetConfig();

            // Инициализируем расписание
            Shedule.deviceCount = config.deviceCount;
            Shedule.proccessingTime = config.proccessingTime;
            Shedule.changeoverTime = GetChangeoverTimeList();

            // Формируем первый уровень
            var firstLevel = new FirstLevel(config, batchCountList);

            // Генерируем решение и записываем его в файл
            firstLevel.GenetateSolutionForAllTypes("outputFirstAlgorithm.txt");
        }

        /// <summary>
        /// Данная функция вызывается при нажатии кнопки "Га + Группы"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OldSecondLevelAll_Click(object sender, EventArgs e)
        {

            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Выполняем перестроение матриц
            TablesRebuild();

            // Формируем вектор из значений { 2, 4, 8, 16, 32 }, для генерации матриц выполнения и переналадки
            int[] array = { 2, 4, 8, 16, 32 };

            using (var file = new StreamWriter("Га, оптимизации групп " + isOptimization + " N=" + DataType + "L=" + Device + ".txt", false))
            {

                // Перебираем каждой элемент вектора, как время переналадки приборов
                foreach (int _maxChangeoverTime in array)
                {

                    // Перебираем каждой элемент вектора, как время выполнения задания
                    foreach (int _maxProccessingTime in array)
                    {
                        for (var tz = 50; tz <= 200; tz += 50)
                        {
                            for (var countGroup = 2; countGroup <= 8; countGroup += 2)
                            {

                                // Инициализируем вектор длиной dataTypesCount, каждый элемент которого будет равен batchCount
                                List<int> batchCountList = CreateBatchCountList();

                                // Выполняем настройку расписания
                                Shedule.deviceCount = Device;
                                Shedule.changeoverTime = OldChangeoverTimeGenerator(_maxChangeoverTime, Device, DataType);
                                Shedule.proccessingTime = OldProccessingTimeGenerator(_maxProccessingTime, Device, DataType);

                                // Создаём экземпляр конфигурационной структуры
                                Config config = GetConfig();

                                // Создаём экземпляр класса GAA
                                var gaa = new GAA(DataType, batchCountList, isFixedBatches, batchCount);

                                // Определяем количество хромосом в списке из хромосом
                                gaa.SetXrom(xromossomiSize);
                                var sostav = gaa.calcFitnessList();

                                //var firstLevel = new FirstLevel(_countType, batchCountList, checkBox1.Checked);
                                //firstLevel.GenetateSolutionForAllTypes("outputFirstAlgorithm.txt");
                                var oldSecondLevel = new OldSecondLevel(tz, countGroup);

                                var listInt = !isOptimization
                                    ? oldSecondLevel.CalcFitnessList(sostav, out int criteria, out int flCrit)
                                    : oldSecondLevel.CalcOptimalFitnessList(sostav, out criteria, out flCrit);

                                // Выводим информацию в файл
                                file.WriteLine($"Tz = {tz}");
                                file.WriteLine($"Tp = {_maxChangeoverTime}");
                                file.WriteLine($"Z = {countGroup}");
                                file.WriteLine($"Crit = {criteria}");
                                file.WriteLine($"fLCrit = {flCrit}");
                                file.WriteLine($"To = {_maxProccessingTime}");
                                file.WriteLine($"{listInt.Sum()} = {listInt.Select(i => i.ToString())}");
                            }
                        }
                    }
                }
            }
            MessageBox.Show(@"Решения найдены");
        }

        /// <summary>
        /// Данная функция вызывается при нажатии кнопки "Трёхуровневая"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OldSecondLevelButton_Click(object sender, EventArgs e)
        {

            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Выполняем перестроение матриц
            TablesRebuild();

            // Формируем вектор из значений { 2, 4, 8, 16, 32 }, для генерации матриц выполнения и переналадки
            var array = new[] { 2, 4, 8, 16, 32 };

            using (var file = new StreamWriter("Фиксированные партии - " + isFixedBatches + ", оптимизации групп " + isOptimization + " N=" + DataType + " L=" + Device + ".txt", false))
            {

                // Перебираем каждой элемент вектора, как время переналадки приборов
                foreach (var _maxChangeoverTime in array)
                {

                    // Перебираем каждой элемент вектора, как время выполнения задания
                    foreach (var _maxProccessingTime in array)
                    {
                        for (var tz = 50; tz <= 200; tz += 50)
                        {
                            for (var countGroup = 2; countGroup <= 8; countGroup += 2)
                            {

                                // Инициализируем вектор длиной dataTypesCount, каждый элемент которого будет равен batchCount
                                List<int> batchCountList = CreateBatchCountList();

                                // Формируем расписание
                                Shedule.deviceCount = Device;
                                Shedule.changeoverTime = OldChangeoverTimeGenerator(_maxChangeoverTime, Device, DataType);
                                Shedule.proccessingTime = OldProccessingTimeGenerator(_maxProccessingTime, Device, DataType);

                                // Создаём экземпляр конфигурационной структуры
                                Config config = GetConfig();

                                // Формируем первый уровень
                                var firstLevel = new FirstLevel(config, batchCountList);

                                firstLevel.GenetateSolutionForAllTypes("outputFirstAlgorithm.txt");
                                var oldSecondLevel = new OldSecondLevel(tz, countGroup);

                                var listInt = !isOptimization
                                    ? oldSecondLevel.CalcFitnessList(firstLevel.PrimeMatrixA, out int criteria, out int flCrit)
                                    : oldSecondLevel.CalcOptimalFitnessList(firstLevel.PrimeMatrixA, out criteria, out flCrit);

                                // Выводим информацию в файл
                                file.WriteLine($"Tz = {tz}");
                                file.WriteLine($"Tp = {_maxChangeoverTime}");
                                file.WriteLine($"Z = {countGroup}");
                                file.WriteLine($"Crit = {criteria}");
                                file.WriteLine($"fLCrit = {flCrit}");
                                file.WriteLine($"To = {_maxProccessingTime}");
                                file.WriteLine($"{listInt.Sum()} = {listInt.Select(i => i.ToString())}");
                            }
                        }
                    }
                }
            }
            MessageBox.Show(@"Решения найдены");
        }

        /// <summary>
        /// Данная фукнция вызывается при нажатии кнопки "Тестовый прогон"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button4_Click(object sender, EventArgs e)
        {

            // Выполняем перестроение матриц
            TablesRebuild();

            Form1.direct = checkBox_deadline_on.Checked;
            int[] N_komplect_sostav = { 2, 4 };

            // Формируем вектор из возможных количеств типов данных
            int[] _dataTypesCountArray = { 5, 10 };

            // Формируем вектор из возможных количеств приборов в конвейерной системе
            int[] _deviceCountArray = { 5, 10 };

            // Формируем вектор из значений { 2, 4, 8, 16, 32 }, для генерации матриц выполнения и переналадки
            int[] time = { 2, 4, 8, 16, 32 };

            var file = "testFile_";
            var str = "direct";
            if (!Form1.direct)
            {
                str = "first_task";
            }

            int n_kom;
            int n_kom_q;
            try
            {
                n_kom = Convert.ToInt32(textBox1.Text);
                n_kom_q = Convert.ToInt32(textBox2.Text);
            }
            catch (Exception)
            {
                n_kom = 2;
                n_kom_q = 2;
            }
            var rand = new Random((int)DateTime.Now.ToBinary());
            int temp;
            var fileOut = new StreamWriter(file + "All_" + str + "_" + n_kom + "_" + n_kom_q + "_new.txt", true);

            timeSets = new List<List<int>>();
            //foreach(var n_kom_q in N_komplect_for_type)
            {
                for (int i = 0; i < n_kom; i++)
                {
                    temp = (rand.Next(10) > 5) ? 150 : 100;

                    timeSets.Add(new List<int>());
                    for (int j = 0; j < n_kom_q; j++)
                    {
                        timeSets[i].Add((j + 1) * temp);
                    }
                }
                foreach (var n_kom_s in N_komplect_sostav)
                {
                    foreach (int _dataTypesCount in _dataTypesCountArray)
                    {
                        compositionSets = new List<List<int>>();
                        for (int i = 0; i < n_kom; i++)
                        {
                            compositionSets.Add(new List<int>());
                            for (var dataType = 0; dataType < _dataTypesCount; dataType++)
                            {
                                temp = rand.Next(10);
                                if (temp > 5)
                                {
                                    compositionSets[i].Add(n_kom_s);
                                }
                                else
                                {
                                    compositionSets[i].Add(2);
                                }
                            }
                        }

                        foreach (var _deviceCount in _deviceCountArray)
                        {

                            // Выводим информацию в файл
                            fileOut.WriteLine($"Kq = {n_kom_q}");
                            fileOut.WriteLine($"Kqs= {n_kom_s}");
                            fileOut.WriteLine($"N  = {_dataTypesCount};\tL = {_deviceCount}");
                            fileOut.WriteLine($"Times");
                            fileOut.WriteLine(ListUtils.MatrixIntToString(timeSets, "\t"));
                            fileOut.WriteLine($"Compositions");
                            fileOut.WriteLine(ListUtils.MatrixIntToString(compositionSets, "\t"));

                            // Перебираем каждой элемент вектора, как время выполнения задания
                            foreach (var _maxProccessingTime in time)
                            {

                                // Перебираем каждой элемент вектора, как время переналадки приборов
                                foreach (var _maxChangeoverTime in time)
                                {

                                    // Формируем расписание
                                    Shedule.deviceCount = _deviceCount;
                                    Shedule.changeoverTime = OldChangeoverTimeGenerator(_maxChangeoverTime, _deviceCount, _dataTypesCount);
                                    Shedule.proccessingTime = OldProccessingTimeGenerator(_maxProccessingTime, _deviceCount, _dataTypesCount);
                                    
                                    // Создаём экземпляр конфигурационной структуры
                                    Config config = new Config(
                                        _dataTypesCount,
                                        _deviceCount,
                                        buffer,
                                        OldProccessingTimeGenerator(_maxProccessingTime, _deviceCount, _dataTypesCount),
                                        Config.ChangeoverTimeConverter(OldChangeoverTimeGenerator(_maxChangeoverTime, _deviceCount, _dataTypesCount)),
                                        isFixedBatches
                                    );
                                    
                                    // Создаём вектор длиной в количество типов данных наполненный нулями
                                    List<int> _batchCountList = CreateBatchCountList(0, _dataTypesCount);

                                    for (var dataType = 0; dataType < _dataTypesCount; dataType++)
                                    {
                                        batchCount = 0;
                                        for (int i = 0; i < n_kom; i++)
                                        {
                                            batchCount += compositionSets[i][dataType];
                                        }
                                        _batchCountList[dataType] = batchCount * n_kom_q;
                                    }

                                    var firstLevel = new FirstLevel(config, _batchCountList);
                                    var result = firstLevel.GenetateSolutionForAllTypesSecondAlgorithm();

                                    //var gaa = new GAA(_countType, listCountButches, checkBox1.Checked, batchCount);
                                    //var resultGaa = gaa.calcSetsFitnessList(checkBox2.Checked, GenerationCounter.Value, (int)numericUpDown2.Value);

                                    //fileOut.WriteLine(first + "\t" + top + "\t" + resultGaa);
                                    fileOut.WriteLine($"first:{result[0]};\ttop:{result[1]}.\t");
                                }
                            }
                        }
                    }
                }
            }
            fileOut.Close();

            MessageBox.Show("Тестовый прогон завершен");
        }

        /// <summary>
        /// Данная функция вызывается при нажатии кнопки "Прогон"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button5Click(object sender, EventArgs e)
        {


            // Выполняем перестроение матриц
            TablesRebuild();

            // Инициализируем выходной файл на добавление
            StreamWriter file_output_method_GAA = new StreamWriter("outputGAASimpleResult.txt", true);

            // Фиксируем начало обработки
            file_output_method_GAA.WriteLine($"Дата и время запуска: {DateTime.Now:O}\n{{");

            // Формируем массив данных [8, 12, 16, 24, 32]
            int[] array = { 8, 12, 16, 24, 32 };

            // Для каждого элемента из массива array выполняем обработку
            foreach (int _batchCount in array)
            {

                // Для следующих количеств типов данных выполняем обработку
                for (int _dataTypesCount = 5; _dataTypesCount <= 10; _dataTypesCount += 5)
                {

                    // Для следующих количеств приборов (сегментов) выполняем обработку
                    for (int _deviceCount = 5; _deviceCount <= 10; _deviceCount += 5)
                    {

                        // Выводим информацию в выходной файл
                        file_output_method_GAA.WriteLine("\tbatchCount(CB)   \t= {0}", _batchCount);
                        file_output_method_GAA.WriteLine("\tconveyorLength(L)\t= {0}", _deviceCount);
                        file_output_method_GAA.WriteLine("\tdataTypesCount(N)\t= {0}", _dataTypesCount);

                        // Для следующих максимальных времён выполнения обработки выполняем обработку
                        for (int _maxProccessingTime = 2; _maxProccessingTime <= 32; _maxProccessingTime *= 2)
                        {
                            // Выводим информацию в выходной файл
                            file_output_method_GAA.WriteLine($"\t{{\n\t\tmaxProccessingTime\t= {_maxProccessingTime}");

                            // Для следующих максимальных времён переналадки приборов выполняем обработку
                            for (int _maxChangeoverTime = 2; _maxChangeoverTime <= 32; _maxChangeoverTime *= 2)
                            {

                                // Выводим информацию в выходной файл
                                file_output_method_GAA.WriteLine($"\t\t{{\n\t\t\tmaxChangeoverTime\t= {_maxChangeoverTime}");

                                // Формируем расписание
                                Shedule.deviceCount = _deviceCount;
                                Shedule.changeoverTime = OldChangeoverTimeGenerator(_maxChangeoverTime, _deviceCount, _dataTypesCount);
                                Shedule.proccessingTime = OldProccessingTimeGenerator(_maxProccessingTime, _deviceCount, _dataTypesCount);

                                // Создаём экземпляр конфигурационной структуры
                                Config config = new Config(
                                    _dataTypesCount,
                                    _deviceCount,
                                    buffer,
                                    OldProccessingTimeGenerator(_maxProccessingTime, _deviceCount, _dataTypesCount),
                                    Config.ChangeoverTimeConverter(OldChangeoverTimeGenerator(_maxChangeoverTime, _deviceCount, _dataTypesCount)),
                                    isFixedBatches
                                );

                                // Инициализируем вектор длиной _dataTypesCount, каждый элемент которого будет равен _batchCount
                                List<int> batchCountList = CreateBatchCountList(_batchCount, _dataTypesCount);

                                var gaa = new GAA(_dataTypesCount, batchCountList, isFixedBatches, _batchCount);
                                gaa.SetXrom((int)numeric_xromossomi_size.Value);
                                var countSourceKit = gaa.calcFitnessList();
                                var result = gaa.getSelectionPopulation(selectoinType, out int s);

                                using (var file_outputGAA = new StreamWriter("outputGAA.txt", true))
                                {
                                    foreach (var elem in gaa.nabor)
                                    {

                                        file_outputGAA.WriteLine(ListUtils.MatrixIntToString(elem.GenList, " "));
                                        file_outputGAA.WriteLine("_________________________");
                                        file_outputGAA.WriteLine(gaa._fitnesslist[_maxChangeoverTime]);
                                        file_outputGAA.WriteLine("+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=");
                                        file_outputGAA.WriteLine("Kit" + countSourceKit[_maxChangeoverTime]);
                                        file_outputGAA.WriteLine("+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=");
                                        file_outputGAA.WriteLine("_________________________");

                                    }

                                    file_outputGAA.WriteLine("***************************");
                                    file_outputGAA.WriteLine(result);
                                    file_outputGAA.WriteLine("***************************");

                                }

                                // Выводим информацию в выходной файл
                                file_output_method_GAA.WriteLine("\t\t\tresult\t= {0}", result);
                                file_output_method_GAA.WriteLine($"\t\t}}");
                            }

                            // Выводим информацию в выходной файл
                            file_output_method_GAA.WriteLine($"\t}}");
                        }
                    }
                }
            }

            // Фиксируем конец обработки
            file_output_method_GAA.WriteLine($"}}\nДата и время завершения: {DateTime.Now:O}");
            file_output_method_GAA.Close();

            MessageBox.Show("Данные успешно записаны", "Успешное завершение", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Данная функция вызывается при нажатии кнопки "Формирование Гаа"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button2_Click(object sender, EventArgs e)
        {

            // Выполняем перестроение матриц
            TablesRebuild();

            // Создаём экземпляр класса Random для генерации рандомных данных
            var rand = new Random(0);

            // Формируем вектор из возможных количеств типов данных
            int[] _dataTypesCountArray = { 5, 10 };

            // Формируем вектор из возможных количеств приборов в конвейерной системе
            int[] _deviceCountArray = { 5, 10 };

            // Формируем вектор из значений { 2, 4, 8, 16, 32 }, для генерации матриц выполнения и переналадки
            int[] time = { 2, 4, 8, 16, 32 };

            int[] N_komplect_type = { 2 };
            int[] N_komplect_for_type = { 2, 4 };
            int[] N_komplect_sostav = { 2, 4 };

            using (var file = new StreamWriter("11 - " + checkBox_deadline_on.Checked + ".txt", false))
            {
                foreach (var n_kom in N_komplect_type)
                {
                    compositionSets = new List<List<int>>();
                    timeSets = new List<List<int>>();
                    foreach (var n_kom_q in N_komplect_for_type)
                    {

                        for (int i = 0; i < n_kom; i++)
                        {
                            var temp = rand.Next(10);
                            temp = (temp > 5) ? 150 : 100;

                            compositionSets.Add(new List<int>());
                            timeSets.Add(new List<int>());
                            for (int j = 0; j < n_kom_q; j++)
                                timeSets[i].Add((j + 1) * temp);

                        }

                        foreach (var n_kom_s in N_komplect_sostav)
                        {
                            foreach (var _dataTypesCount in _dataTypesCountArray)
                            {

                                for (int i = 0; i < n_kom; i++)
                                    for (var dataType = 0; dataType < _dataTypesCount; dataType++)
                                        compositionSets[i].Add((rand.Next(10) > 5) ? n_kom_s : 2);

                                foreach (var _deviceCount in _deviceCountArray)
                                {
                                    file.WriteLine($"Kn =\t{n_kom}");
                                    file.WriteLine($"Kq =\t{n_kom_q}");
                                    file.WriteLine($"Kqs=\t{n_kom_s}");
                                    file.WriteLine($"N  =\t{_dataTypesCount};\tL = {_deviceCount}");

                                    // Перебираем каждой элемент вектора, как время выполнения задания
                                    foreach (var _maxProccessingTime in time)
                                    {
                                        
                                        // Перебираем каждой элемент вектора, как время переналадки приборов
                                        foreach (var _maxChangeoverTime in time)
                                        {

                                            // Формируем расписание
                                            Shedule.deviceCount = _deviceCount;
                                            Shedule.changeoverTime = OldChangeoverTimeGenerator(_maxChangeoverTime, _deviceCount, _dataTypesCount);
                                            Shedule.proccessingTime = OldProccessingTimeGenerator(_maxProccessingTime, _deviceCount, _dataTypesCount);

                                            // Создаём экземпляр конфигурационной структуры
                                            Config config = new Config(
                                                _dataTypesCount,
                                                _deviceCount,
                                                buffer,
                                                OldProccessingTimeGenerator(_maxProccessingTime, _deviceCount, _dataTypesCount),
                                                Config.ChangeoverTimeConverter(OldChangeoverTimeGenerator(_maxChangeoverTime, _deviceCount, _dataTypesCount)),
                                                isFixedBatches
                                            );

                                            // Создаём вектор длиной dataTypesCount из 0
                                            var batchCountList = CreateBatchCountList(0, _dataTypesCount);

                                            for (var dataType = 0; dataType < _dataTypesCount; dataType++)
                                            {
                                                batchCount = 0;
                                                for (int i = 0; i < n_kom; i++)
                                                    batchCount += compositionSets[i][dataType] * n_kom_q;

                                                batchCountList[dataType] = batchCount;
                                            }

                                            var gaa = new GAA(_dataTypesCount, batchCountList, isFixedBatches, batchCount);
                                            
                                            var result = gaa.calcSetsFitnessList(checkBox_deadline_on.Checked, generationCount, xromossomiSize);

                                            file.WriteLine(result);
                                        }
                                    }
                                    file.WriteLine();
                                }
                            }
                        }
                    }
                }
            }

            MessageBox.Show("Данные успешно записаны", "Успешное завершение", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Данная функция вызывается при нажатии кнопки "Тест комплектов"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetsBtn_Click(object sender, EventArgs e)
        {

            // Выполняем перестроение матриц
            TablesRebuild();
        }

        /// <summary>
        /// Данная функция обрабатываем копирования данных в таблицы dataGridView_changeover_time с первого устройства на все остальные
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyPreprocessingTime_Click(object sender, EventArgs e)
        {

            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Для всех строк после первого устройства выполняем обработку
            for (int rowIndex = DataType; rowIndex < DataType * Device; rowIndex++)
            {

                // Определяем обрабатываемую строку
                var row = dataGridView_changeover_time.Rows[rowIndex];

                // Для всех столбцов выполняем выполняем обработку
                for (int columnIndex = 0; columnIndex < row.Cells.Count; columnIndex++)

                    // Определяем новое значение в ячейке, как скопированное с первого устройства
                    dataGridView_changeover_time.Rows[rowIndex].Cells[columnIndex].Value = dataGridView_changeover_time.Rows[rowIndex % DataType].Cells[columnIndex].Value;
            }
        }

        private void maintenceDurationSetButton_Click(object sender, EventArgs e)
        {
            int preMaintenceDuration = (int)preMaintenceDurationSetValue.Value;
            int devicesCount = (int)numeric_device_count.Value;
            List<int> preMaintenceTimes = new List<int>();
            for (int i = 0; i < devicesCount; i++)
            {
                preMaintenceTimes.Add(preMaintenceDuration);
            }
            SetPreMaintenanceTimes(preMaintenceTimes);
        }

        private void failureRatesSetButton_Click(object sender, EventArgs e)
        {
            double failureRate = (double)failureRatesSetValue.Value;
            int devicesCount = (int)numeric_device_count.Value;
            List<double> failureRates = new List<double>();
            for (int i = 0; i < devicesCount; i++)
            {
                failureRates.Add(failureRate);
            }
            SetFailureRates(failureRates);
        }

        private void restoreRatesSetButton_Click(object sender, EventArgs e)
        {
            double restoringRate = (double)restoreRatesSetValue.Value;
            int devicesCount = (int)numeric_device_count.Value;
            List<double> restoringRates = new List<double>();
            for (int i = 0; i < devicesCount; i++)
            {
                restoringRates.Add(restoringRate);
            }
            SetRestoringDevice(restoringRates);
        }

        /// <summary>
        /// Фуния открывает диалоговое окно, для импорта параметров системы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_import_Click(object sender, EventArgs e)
        {

            // Объявляем конфигурационную структуру данных
            PreMConfig preMConfig;

            // Объявляем строку
            string jsonText;

            // Открываем диалоговое окно для открытия файла
            {

                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    Filter = "json files (*.json)|*.json",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                // Пытаемя открыть диалоговое окно
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {

                    // Читаем данные из json файла
                    jsonText = File.ReadAllText(fileDialog.FileName);

                    // Пытаемся десереализировать данные
                    try
                    {

                        // Десереализируем данные
                        preMConfig = JsonConvert.DeserializeObject<PreMConfig>(jsonText);

                        // Импортируем данные из конфигурационной ПТО структуры
                        ImportFromPreMConfig(preMConfig);

                        // Обрабатываем ошибку десереализации
                    }
                    catch (JsonException ex)
                    {

                        // Выводим информационное сообщение
                        MessageBox.Show("Указанный имеет некорректный формат.", "Ошибка");

                        // Прекращяем обработку
                        return;
                    }

                }
                else
                {

                    // Выводим информационное сообщение
                    MessageBox.Show("Указан некорректный файл.", "Предупреждение");

                    // Прекращяем обработку
                    return;
                }
            }
        }

        /// <summary>
        /// Функция открывает диалоговое окно, для экспорта параметров системы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_export_Click(object sender, EventArgs e)
        {

            // Объявляем конфигурационную структуру данных
            PreMConfig preMConfig = GetPreMConfig();

            // Сереализируем данные
            string jsonText = JsonConvert.SerializeObject(preMConfig, Formatting.Indented);

            // Открываем диалоговое окно для сохранения файла
            {
                Stream myStream;
                SaveFileDialog fileDialog = new SaveFileDialog
                {
                    Filter = "json files (*.json)|*.json",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                // Пытаемя открыть диалоговое окно
                if (fileDialog.ShowDialog() == DialogResult.OK)
                    if ((myStream = fileDialog.OpenFile()) != null)
                    {
                        byte[] buffer = Encoding.Default.GetBytes($"{jsonText}");
                        myStream.Write(buffer, 0, buffer.Length);
                        myStream.Close();
                    }
            }
        }

        private void GetPreMSolution_Click(object sender, EventArgs e)
        {

            // Получаем экземпляр конфигурационной структуры для ПТО
            PreMConfig preMConfig = GetPreMConfig();

            // Инициализируем вектор длиной dataTypesCount, каждый элемент которого будет равен batchCount
            List<int> batchCountList = CreateBatchCountList();

            // Формируем первый уровень
            var firstLevel = new FirstLevel(preMConfig.config, batchCountList);

            PreMConfiguration configuration = new PreMConfiguration(preMConfig);

            // Выполняем генерацию данных для всех типов вторым алгоритмом
            firstLevel.GenetateSolutionWithPremaintenance("PreMaintenance", configuration);
        }

        private void getPreMSolutionOpt_Click(object sender, EventArgs e)
        {
            // Получаем экземпляр конфигурационной структуры для ПТО
            PreMConfig preMConfig = GetPreMConfig();

            // Инициализируем вектор длиной dataTypesCount, каждый элемент которого будет равен batchCount
            List<int> batchCountList = CreateBatchCountList();

            // Формируем первый уровень
            var firstLevel = new FirstLevel(preMConfig.config, batchCountList);

            // Выполняем генерацию данных для всех типов вторым алгоритмом
            firstLevel.GenetateSolutionWithPremaintenance("PreMaintenance", new PreMConfiguration(preMConfig));
        }

        private void getTypedPreMSolutionBtn_Click(object sender, EventArgs e)
        {
            var firstLevel = new FirstLevel(GetConfig(), CreateBatchCountList());
            firstLevel.GenetateSolutionWithTypedPremaintenance("TypedPreM", TypedPreMConfiguration());
        }

        private void TypedPreM_ConfSave_Btn_Click(object sender, EventArgs e)
        {
            // Объявляем конфигурационную структуру данных
            var preMConfig = TypedPreMConfiguration();

            // Открываем диалоговое окно для сохранения файла
            {
                Stream myStream;
                SaveFileDialog fileDialog = new SaveFileDialog
                {
                    Filter = "json files (*.json)|*.json",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                // Пытаемя открыть диалоговое окно
                if (fileDialog.ShowDialog() == DialogResult.OK)
                    if ((myStream = fileDialog.OpenFile()) != null)
                    {
                        byte[] buffer = Encoding.Default.GetBytes($"{jsonText}");
                        myStream.Write(buffer, 0, buffer.Length);
                        myStream.Close();
                    }
            }
        }

        private void TypedPreM_ConfLoad_Btn_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region Обработка рандомизаций и выбора вкладки

        /// <summary>
        /// Данная функция выполняет перестроение таблиц
        /// </summary>
        private void TablesRebuild() {

            // Устанавливаем рандомные значения времени выполнения
            RandomizeProcessingTime_Click();

            // Устанавливаем рандомные значения времени переналдки
            RandomizeChangeoverTime_Click();

            // Устанавливаем рандомные значения времени ПТО
            RandomizePreMaintenanceTime_Click();
        }

        /// <summary>
        /// Данная функция обрабатываем переключение между вкладками "Установка параметров" и "Установка времени"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_time_setup_Selecting(object sender, TabControlCancelEventArgs e) { }

        /// <summary>
        /// Данная функция выполняет рандомизацию таблицы слева (таблица времени обработки)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RandomizeProcessingTime_Click(object sender = null, EventArgs e = null)
        {

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Объявляем и инициализируем количество приборов
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Объявляем и инциализируем рандомные значения
            int _randomValue = randomValue / 2;

            // Объявляем матрицу времени выполнения
            List<List<int>> processingTime = new List<List<int>>(Device);
            
            // Для каждого прибора
            for (int device = 0; device < Device; device++) {

                // Добавляем новую строку
                processingTime.Add(new List<int>(DataType));

                // Чередуем два рандомных значения _randomValue и randomValue с каждой новой строкой
                int value = (device % 2 == 0) ? _randomValue : randomValue;

                // Для каждого типа данных
                for (int dataType = 0; dataType < DataType; dataType++) {


                    // Чередуем два рандомных значения _randomValue и randomValue с каждой новой колонкой
                    value = (value == _randomValue) ? randomValue : _randomValue;

                    // Добавляем новое значение
                    processingTime[device].Add(value);
                }
            }

            // Устанавливаем матрицу времени выполнения
            SetProcessingTime(processingTime);
        }

        /// <summary>
        /// Данная функция выполняет рандомизацию таблицы посередине (таблица времени переналадки)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RandomizeChangeoverTime_Click(object sender = null, EventArgs e = null)
        {

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Объявляем и инициализируем количество приборов
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Объявляем и инциализируем рандомные значения
            int _randomValue = randomValue / 2;

            // Объявляем матрицу времени выполнения
            List<List<List<int>>> changeoverTime = new List<List<List<int>>>(Device);

            // Для каждого устройства
            for (int device = 0; device < Device; device++)
            {

                // Добавляем строку
                changeoverTime.Add(new List<List<int>>(DataType));

                // Для каждого типа данных
                for (int fromDataType = 0; fromDataType < DataType; fromDataType++)
                {

                    // Добавляем строку
                    changeoverTime[device].Add(new List<int>(DataType));

                    // Чередуем два рандомных значения _randomValue и randomValue с каждой новой строкой
                    int value = (fromDataType % 2 == 0) ? _randomValue : randomValue;

                    // Для каждого типа данных
                    for (int toDataType = 0; toDataType < DataType; toDataType++)
                    {

                        // Когда индекс по строке равен индексу по столбцу
                        if (fromDataType == toDataType) {

                            // Присваиваем значение 0
                            changeoverTime[device][fromDataType].Add(0);

                            // Продолжаем обработку
                            continue;
                        }

                        // Чередуем два рандомных значения _randomValue и randomValue с каждой новой колонкой
                        value = (value == _randomValue) ? randomValue : _randomValue;

                        // Присваиваем значение value
                        changeoverTime[device][fromDataType].Add(value);
                    }
                }
            }

            // Устанавливаем данные в таблице
            SetChangeoverTime(changeoverTime);
        }

        /// <summary>
        /// Данная функция выполняем рандомизацию таблицы справа (таблица времени ПТО)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RandomizePreMaintenanceTime_Click(object sender = null, EventArgs e = null)
        {

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Объявляем и инициализируем количество приборов
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Объявляем и инициализируем вектор длительностей ПТО
            List<int> preMaintenanceTimes = new List<int>(Device);

            // Объявляем и инициализируем вектор интенсивности отказов
            List<double> restoringDevice = new List<double>(Device);

            // Объявляем и инициализируем вектор интенсивности восстановлени
            List<double> failureRates = new List<double>(Device);

            // Создаём объект для генерации рандомного значения
            var random = new Random(randomValue);

            // Формируем второе рандомное значение, как половина от введённого
            int _randomValue = randomValue / 2;

            // Для каждого устройства
            for (int device = 0; device < Device; device++) {

                // Рандомим значение длительностей ПТО
                preMaintenanceTimes.Add((device % 2 == 0) ? randomValue : _randomValue);

                // Рандомим значение интенсивности восстановления
                restoringDevice.Add(random.NextDouble() % 0.01);

                // Рандомим значение интенсивности отказов
                failureRates.Add(random.NextDouble());
            }

            // Устанавливаем значение таблицы длительностей ПТО
            SetPreMaintenanceTimes(preMaintenanceTimes);

            // Устанавливаем значение таблицы длительностей ПТО
            SetRestoringDevice(restoringDevice);

            // Устанавливаем значение таблицы длительностей ПТО
            SetFailureRates(failureRates);
        }

        #endregion

        #region Обработчики изменения значений полей

        #region Выбор способа обработки данных

        /// <summary>
        /// Данная функция обрабатывает выбор обработки данных и отмечает выбора, как SelectoinType.TournamentSelection (Турнирная селекция)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_TournamentSelection_change(object sender, EventArgs e)
        {
            selectoinType = SelectoinType.TournamentSelection;
        }

        /// <summary>
        /// Данная функция обрабатывает выбор обработки данных и отмечает выбора, как SelectoinType.RouletteMethod (Метод рулетки)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_RouletteMethod_change(object sender, EventArgs e)
        {
            selectoinType = SelectoinType.RouletteMethod;
        }

        /// <summary>
        /// Данная функция обрабатывает выбор обработки данных и отмечает выбора, как SelectoinType.UniformRanking (Равномерное ранжирование)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_UniformRanking_change(object sender, EventArgs e)
        {
            selectoinType = SelectoinType.UniformRanking;
        }

        /// <summary>
        /// Данная функция обрабатывает выбор обработки данных и отмечает выбора, как SelectoinType.SigmaClipping (Сигма отсечение)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_SigmaClipping_change(object sender, EventArgs e)
        {
            selectoinType = SelectoinType.SigmaClipping;
        }

        #endregion

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента numeric_batch_count и записывает их в batchCount
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Numeric_batch_count_ValueChanged(object sender, EventArgs e)
        {
            batchCount = Convert.ToInt32(numeric_batch_count.Value);
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента numeric_max_changeover_time и записывает их в maxChangeoverTime
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Numeric_max_changeover_time_ValueChanged(object sender, EventArgs e)
        {
            // maxChangeoverTime = Convert.ToInt32(numeric_max_changeover_time.Value);
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента numeric_max_proccessing_time и записывает их в maxProccessingTime
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Numeric_max_proccessing_time_ValueChanged(object sender, EventArgs e)
        {
            // maxProccessingTime = Convert.ToInt32(numeric_max_proccessing_time.Value);
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента numeric_data_types_count и записывает их в dataTypesCount
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Numeric_data_types_count_ValueChanged(object sender, EventArgs e)
        {
            TablesRebuild();
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента numeric_buffer и записывает их в buffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Numeric_buffer_ValueChanged(object sender, EventArgs e)
        {
            buffer = Convert.ToInt32(numeric_buffer.Value);
            Form1.buff = buffer;
            TablesRebuild();
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента numeric_device_count и записывает их в deviceCount
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Numeric_device_count_ValueChanged(object sender, EventArgs e)
        {
            TablesRebuild();
            UpdateDevicePremTypeDGV(dataGridView_preMDuration);
            UpdateDevicePremTypeDGV(dataGridView_preMCosts);
            UpdatePreMaintenceGeneralDGV();
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента checkBox_fixed_batches и записывает их в isFixedBatches
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_fixed_batches_CheckedChanged(object sender, EventArgs e)
        {
            isFixedBatches = checkBox_fixed_batches.Checked;
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента checkBox_optimization и записывает их в isOptimization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_optimization_CheckedChanged(object sender, EventArgs e)
        {
            isOptimization = checkBox_optimization.Checked;
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента checkBox_visualization и записывает их в vizualizationOn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_visualization_CheckedChanged(object sender, EventArgs e)
        {
            Form1.vizualizationOn = checkBox_visualization.Checked;
            if (!Form1.vizualizationOn) { 
                checkBox_ganta.Checked = false;
                Form1.gantaOn = false;
                checkBoxHideND.Checked = false;
                Form1.showND = false;
            }
            checkBox_ganta.Enabled = Form1.vizualizationOn;
            checkBoxHideND.Enabled = Form1.vizualizationOn;
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента checkBox_logging и записывает их в loggingOn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_logging_CheckedChanged(object sender, EventArgs e)
        {
            Form1.loggingOn = checkBox_logging.Checked;
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента checkBox_ganta и записывает их в gantaOn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_ganta_CheckedChanged(object sender, EventArgs e)
        {
            Form1.gantaOn = checkBox_ganta.Checked;
        }

        /// <summary>
        /// Функция-обработчик обновления флага checkBoxHideND
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxHideND_CheckedChanged(object sender, EventArgs e)
        {
            Form1.showND = !checkBoxHideND.Checked;
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента numeric_random и записывает их в randomValue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Numeric_random_ValueChanged(object sender, EventArgs e)
        {
            randomValue = Convert.ToInt32(numeric_random.Value);
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента numeric_generation_count и записывает их в generation_count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Numeric_generation_count_ValueChanged(object sender, EventArgs e)
        {
            generationCount = Convert.ToInt32(numeric_generation_count.Value);
        }

        /// <summary>
        /// Данная функция определяет входные параметры с графического компонента numeric_xromossomi_size и записывает их в xromossomiSize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            xromossomiSize = Convert.ToInt32(numeric_xromossomi_size.Value);
        }

        /// <summary>
        /// Обработчик изменения количества типов ПТО
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numeric_preM_types_count_ValueChanged(object sender, EventArgs e)
        {
            UpdateDevicePremTypeDGV(dataGridView_preMDuration);
            UpdateDevicePremTypeDGV(dataGridView_preMCosts);
        }

        /// <summary>
        /// Функция управляет значением нижнего порога надёжности
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void betaValue_TextChanged(object sender, EventArgs e)
        {

            // Преобразуем входные данные в число типа Double
            double.TryParse(betaValue.Text, out double beta);

            // Если beta будет больше 1
            if (beta > (double)1.0)
            {
                MessageBox.Show("Ошибка. Значение beta не может быть больше 1");
                betaValue.Text = (0.9999).ToString();
                return;
            }

            // Если beta будет меньше 0
            if (beta < (double)0.0)
            {
                MessageBox.Show("Ошибка. Значение beta не может быть меньше 0");
                betaValue.Text = (0.0001).ToString();
                return;
            }
        }

        #endregion

        #endregion

        #region Вспомогательные функции

        /// <summary>
        /// Данная функция возвращает вектор длиной в количество типов данных состоящий из максимального размера каждого типа данных.
        /// Например для _dataTypesCount = 3 и _batchCount = 2 вернёт [2,2,2].
        /// Если вызвать функцию с аргументами по умолчанию, то в качестве значений _batchCount и _dataTypesCount будут 
        /// взяты локальные значения batchCount и dataTypesCount.
        /// </summary>
        /// <param name="_batchCount">Максимальное количество элементов для любого типа данных</param>
        /// <param name="_dataTypesCount">Максимальное количество типов данных</param>
        /// <returns>Вектор длиной _dataTypesCount, каждый элемент которого равен _batchCount</returns>
        private List<int> CreateBatchCountList(int _batchCount = -1, int _dataTypesCount = 0)
        {
            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Проверяем аргументы на корректные параметры
            _batchCount = _batchCount == -1 ? batchCount : _batchCount;
            _dataTypesCount = _dataTypesCount == 0 ? DataType : _dataTypesCount;

            // Объявляем переменную списка содержащую количество элементов каждого типа
            List<int> batchCountList = new List<int>(_dataTypesCount);
            batchCountList.AddRange(Enumerable.Repeat(_batchCount, _dataTypesCount));

            // Возвращаем список количества элементов каждого типа
            return batchCountList;
        }

        /// <summary>
        /// Данная функция генерирует матрицу времён выполнения заданий. Данный участок кода использовался в более ранней версии программы и назывался RandomTime
        /// <param name="_maxChangeoverTime">Устанавливаем ограничения для генерции данных</param>
        /// <param name="_deviceCount">Устанавливаем количество приборов для размерности матрицы</param>
        /// <param name="_dataTypesCount">Устанавливаем количество типов данных для размерности матрицы</param>
        /// <returns>Возвращаем матрицу времён выполнения заданий</returns>
        private List<List<int>> OldProccessingTimeGenerator(int _maxChangeoverTime, int _deviceCount, int _dataTypesCount)
        {
            // Объявляем переменную для хранения матрицы времени выполнения заданий
            List<List<int>> _proccessingTime = new List<List<int>>();

            // Инициализируем переменную для генерации данных
            int count = 0;

            // Для каждого устройства выполняем генерацию
            for (var device = 0; device < _deviceCount; device++)
            {

                // Выполняем инициализацию матрицы
                _proccessingTime.Add(new List<int>());

                // Для каждого типа данных выполняем генерацию
                for (var dataType = 0; dataType < _dataTypesCount; dataType++)
                    _proccessingTime[device].Add((count++ % 2 == 0) ? 2 : _maxChangeoverTime);
            }

            // Возвращаем матрицу времён выполнения
            return _proccessingTime;
        }

        /// <summary>
        /// Данная функция генерирует матрицу времён переналадки приборов. Данный участок кода использовался в более ранней версии программы и назывался RandomTime
        /// <param name="_maxChangeoverTime">Устанавливаем ограничения для генерции данных</param>
        /// <param name="_deviceCount">Устанавливаем количество приборов для размерности матрицы</param>
        /// <param name="_dataTypesCount">Устанавливаем количество типов данных для размерности матрицы</param>
        /// <returns>Возвращаем матрицу времён переналадки приборов</returns>
        private List<List<List<int>>> OldChangeoverTimeGenerator(int _maxChangeoverTime, int _deviceCount, int _dataTypesCount)
        {
            // Объявляем переменную для хранения матрицы времени переналадки приборов
            List<List<List<int>>> _changeoverTime = new List<List<List<int>>>();

            // Инициализируем переменную для генерации данных
            int count = 0;

            // Для каждого устройства выполняем генерацию
            for (var device = 0; device < _deviceCount; device++)
            {

                // Выполняем инициализацию матрицы
                _changeoverTime.Add(new List<List<int>>());

                // Для каждого типа данных выполняем генерацию
                for (var row_dataType = 0; row_dataType < _dataTypesCount; row_dataType++)
                {

                    // Выполняем инициализацию матрицы
                    _changeoverTime[device].Add(new List<int>());

                    // Для каждого типа данных выполняем генерацию
                    for (var col_dataType = 0; col_dataType < _dataTypesCount; col_dataType++)
                        _changeoverTime[device][row_dataType].Add((count % 2 == 0) ? 2 : _maxChangeoverTime);

                    // Увеличиваем счётчик
                    count++;
                }
            }

            // Возвращаем матрицу времён переналадки приборов
            return _changeoverTime;
        }

        /// <summary>
        /// Данная функция инициализирует все необходимые, для работы приложения, переменные
        /// </summary>
        private void InitializeForm()
        {
            // Инициализируем переменные с внешних компонентов
            // maxChangeoverTime = Convert.ToInt32(numeric_max_changeover_time.Value);
            // maxProccessingTime = Convert.ToInt32(numeric_max_proccessing_time.Value);
            xromossomiSize = Convert.ToInt32(numeric_xromossomi_size.Value);
            batchCount = Convert.ToInt32(numeric_batch_count.Value);
            randomValue = Convert.ToInt32(numeric_random.Value);
            buffer = Convert.ToInt32(numeric_buffer.Value);
            isFixedBatches = checkBox_fixed_batches.Checked;
            isOptimization = checkBox_optimization.Checked;
            selectoinType = SelectoinType.Undefined;
            Form1.vizualizationOn = false;
            Form1.buff = buffer;

            // Выполняем проверку на наличие Excel на компьютере
            if (Type.GetTypeFromProgID("Excel.Application") == null)

                // Выключаем графический компонент checkBox_visualization, если Excel нет на компьютере
                checkBox_visualization.Enabled = false;

            // Выполняем инициализацию внутренних таблиц
            TablesRebuild();
            UpdateDevicePremTypeDGV(dataGridView_preMDuration);
            UpdateDevicePremTypeDGV(dataGridView_preMCosts);
            UpdatePreMaintenceGeneralDGV();
        }

        #endregion

        /// <summary>
        /// Установим матрицу времени выполнения заданий
        /// </summary>
        /// <param name="processingTime">Матрица времени выполнения заданий</param>
        private void SetProcessingTime(List<List<int>> processingTime)
        {

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Отчищаяем матрицу времени выполнения
            dataGridView_processing_time.Rows.Clear();
            dataGridView_processing_time.Columns.Clear();

            // Устанавливаем количество строк и колонок для матрицы времени выполнения
            dataGridView_processing_time.RowCount = Device;
            dataGridView_processing_time.ColumnCount = DataType;

            // Для каждого типа данных
            for (int dataType = 0; dataType < DataType; dataType++)

                // Устанавливаем заголовок строки
                dataGridView_processing_time.Columns[dataType].HeaderCell.Value = $"Тип {dataType + 1}";

            // Для каждого прибора
            for (int device = 0; device < Device; device++)
            {

                // Устанавливаем заголовок строки
                dataGridView_processing_time.Rows[device].HeaderCell.Value = $"Прибор {device + 1}";

                // Для каждого типа данных
                for (int dataType = 0; dataType < DataType; dataType++)

                    // Устанавливаем значение матрицы времени выполнения
                    dataGridView_processing_time.Rows[device].Cells[dataType].Value = processingTime[device][dataType];
            }
        }

        /// <summary>
        /// Функция вернёт матрицу времени выполнения
        /// </summary>
        /// <returns>Матрица времени выполнения</returns>
        private List<List<int>> GetProcessingTime()
        {

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Объявляем матрицу время выполнения
            List<List<int>> processingTime = new List<List<int>>(Device);

            // Для каждого прибора
            for (int device = 0; device < Device; device++) {

                // Добавляем новую строку
                processingTime.Add(new List<int> (DataType));

                // Для каждого типа данных
                for (int dataType = 0; dataType < DataType; dataType++)

                    // Добавляем элемент в строку
                    processingTime[device].Add(Convert.ToInt32(this.dataGridView_processing_time.Rows[device].Cells[dataType].Value)); 
            }

            // Возвращяем матрицу
            return processingTime;
        }

        /// <summary>
        /// Функция установит матрицу времени переналадки
        /// </summary>
        /// <param name="changeoverTime">Матрица времени переналадки</param>
        private void SetChangeoverTime(List<List<List<int>>> changeoverTime)
        {

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Отчищаяем матрицу времени переналадки
            dataGridView_changeover_time.Rows.Clear();
            dataGridView_changeover_time.Columns.Clear();

            // Устанавливаем количество строк и колонок для матрицы времени переналадки
            dataGridView_changeover_time.RowCount = Device * DataType;
            dataGridView_changeover_time.ColumnCount = DataType;
            
            // Для каждой строки
            for (int deviceAndDataType = 0; deviceAndDataType < Device * DataType; deviceAndDataType++)

                // Устанавливаем заголовок строки
                dataGridView_changeover_time.Rows[deviceAndDataType].HeaderCell.Value = $"Тип {(deviceAndDataType % DataType)+1}";

            // Для каждого столбца
            for (int dataType = 0; dataType < DataType; dataType++)

                // Устанавливаем заголовок строки
                dataGridView_changeover_time.Columns[dataType].HeaderCell.Value = $"Тип {dataType + 1}";

            // Для каждого прибора
            for (int device = 0; device < Device; device++)
            
                // Для каждого типа данных
                for (int fromDataType = 0; fromDataType < DataType; fromDataType++)

                    // Для каждого типа данных
                    for (int toDataType = 0; toDataType < DataType; toDataType++)

                        // Устанавливаем значение матрицы времени переналадки
                        dataGridView_changeover_time.Rows[device * DataType + fromDataType].Cells[toDataType].Value = changeoverTime[device][fromDataType][toDataType];
        }

        /// <summary>
        /// Функция установит матрицу времени переналадки
        /// </summary>
        /// <param name="changeoverTime">Матрица времени переналадки</param>
        private void SetChangeoverTime(Dictionary<int, List<List<int>>> changeoverTime)
        {

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Отчищаяем матрицу времени переналадки
            dataGridView_changeover_time.Rows.Clear();
            dataGridView_changeover_time.Columns.Clear();

            // Устанавливаем количество строк и колонок для матрицы времени переналадки
            dataGridView_changeover_time.RowCount = Device * DataType;
            dataGridView_changeover_time.ColumnCount = DataType;

            // Для каждой строки
            for (int deviceAndDataType = 0; deviceAndDataType < Device * DataType; deviceAndDataType++)

                // Устанавливаем заголовок строки
                dataGridView_changeover_time.Rows[deviceAndDataType].HeaderCell.Value = $"Тип {(deviceAndDataType % DataType) + 1}";

            // Для каждого столбца
            for (int dataType = 0; dataType < DataType; dataType++)

                // Устанавливаем заголовок строки
                dataGridView_changeover_time.Columns[dataType].HeaderCell.Value = $"Тип {dataType + 1}";

            // Для каждого прибора
            for (int device = 0; device < Device; device++)

                // Для каждого типа данных
                for (int fromDataType = 0; fromDataType < DataType; fromDataType++)

                    // Для каждого типа данных
                    for (int toDataType = 0; toDataType < DataType; toDataType++)

                        // Устанавливаем значение матрицы времени переналадки
                        dataGridView_changeover_time.Rows[device * DataType + fromDataType].Cells[toDataType].Value = changeoverTime[device][fromDataType][toDataType];
        }

        /// <summary>
        /// Функция вернёт матриц времени переналадки
        /// </summary>
        /// <returns></returns>
        private List<List<List<int>>> GetChangeoverTimeList()
        {

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Объявляем матрицу времени переналадки
            List<List<List<int>>> changeoverTime = new List<List<List<int>>>(Device);

            // Для каждого прибора
            for (var device = 0; device < Device; device++)
            {

                // Добавляем новую матрицу
                changeoverTime.Add(new List<List<int>>(DataType));

                // Для каждого типа данных
                for (var fromDataType = 0; fromDataType < DataType; fromDataType++)
                {

                    // Добавляем новую строку
                    changeoverTime[device].Add(new List<int>(DataType));

                    // Для каждого типа данных
                    for (var toDataType = 0; toDataType < DataType; toDataType++)

                        // Добавляем значения в матрицу
                        changeoverTime[device][fromDataType].Add(Convert.ToInt32(dataGridView_changeover_time.Rows[device * DataType + fromDataType].Cells[toDataType].Value));
                }
            }

            // Вернём матрицу
            return changeoverTime;
        }

        /// <summary>
        /// Функция вернёт матриц времени переналадки
        /// </summary>
        /// <returns>Матрица переналадки</returns>
        private Dictionary<int, List<List<int>>> GetChangeoverTimeDict()
        {

            // Объявляем и инициализируем количество приборов
            int Device = Convert.ToInt32(numeric_device_count.Value);

            // Объявляем и инициализируем количество типов данных
            int DataType = Convert.ToInt32(numeric_data_types_count.Value);

            // Объявляем матрицу времени переналадки
            Dictionary<int, List<List<int>>> changeoverTime = new Dictionary<int, List<List<int>>>(Device);

            // Для каждого прибора
            for (var device = 0; device < Device; device++)
            {

                // Добавляем новую матрицу
                changeoverTime.Add(device, new List<List<int>>(DataType));

                // Для каждого типа данных
                for (var fromDataType = 0; fromDataType < DataType; fromDataType++)
                {

                    // Добавляем новую строку
                    changeoverTime[device].Add(new List<int>(DataType));

                    // Для каждого типа данных
                    for (var toDataType = 0; toDataType < DataType; toDataType++)

                        // Добавляем значения в матрицу
                        changeoverTime[device][fromDataType].Add(Convert.ToInt32(dataGridView_changeover_time.Rows[device * DataType + fromDataType].Cells[toDataType].Value));
                }
            }

            // Вернём матрицу
            return changeoverTime;
        }

        /// <summary>
        /// Функция установит длительности ПТО
        /// </summary>
        /// <param name="preMaintenanceTimes">Вектор длительностей ПТО</param>
        private void SetPreMaintenanceTimes(List<int> preMaintenanceTimes)
        {

            // Отчищаяем матрицу времени длительностей ПТО
            dataGridView_pre_maintenance.Rows.Clear();
            dataGridView_pre_maintenance.Columns.Clear();

            // Устанавливаем количество строк и колонок для матрицы времени длительностей ПТО
            dataGridView_pre_maintenance.RowCount = preMaintenanceTimes.Count;
            dataGridView_pre_maintenance.ColumnCount = 1;

            // Устанавливаем заголовок столбца
            dataGridView_pre_maintenance.Columns[0].HeaderCell.Value = $"Время";

            // Для каждого прибора
            for (int device = 0; device < preMaintenanceTimes.Count; device++) {

                // Устанавливаем заголовки строк
                dataGridView_pre_maintenance.Rows[device].HeaderCell.Value = $"Прибор {device + 1}";

                // Устанавливаем значения таблицы
                dataGridView_pre_maintenance.Rows[device].Cells[0].Value = preMaintenanceTimes[device];
            }
        }

        /// <summary>
        /// Функция вернёт длительности ПТО
        /// </summary>
        /// <returns>Вектори длительностей ПТО</returns>
        private List<int> GetPreMaintenanceTimes()
        {

            // Объявляем вектор длительностей ПТО
            List<int> preMaintenanceTimes = new List<int>(dataGridView_pre_maintenance.RowCount);

            // Для каждого прибора
            for (int device = 0; device < dataGridView_pre_maintenance.RowCount; device++)

                // Считываем матрицу длительностей ПТО
                preMaintenanceTimes.Add(Convert.ToInt32(dataGridView_pre_maintenance.Rows[device].Cells[0].Value));

            // Вовзращяем вектор длительностей ПТО
            return preMaintenanceTimes; 
        }

        /// <summary>
        /// Функция установит вектор интенсивности восстановления
        /// </summary>
        /// <param name="restoringDevice">Вектор интенсивности восстановления</param>
        private void SetRestoringDevice(List<double> restoringDevice)
        {

            // Отчищаяем вектор интенсивности восстановления
            dataGridView_preM_restoringDevice.Rows.Clear();
            dataGridView_preM_restoringDevice.Columns.Clear();

            // Устанавливаем количество строк и колонок для вектора интенсивности восстановления
            dataGridView_preM_restoringDevice.RowCount = restoringDevice.Count;
            dataGridView_preM_restoringDevice.ColumnCount = 1;

            // Для каждого прибора
            for (int device = 0; device < restoringDevice.Count; device++) {

                // Устанавливаем заголовки строк
                dataGridView_preM_restoringDevice.Rows[device].HeaderCell.Value = $"Прибор {device + 1}";

                // Установим значение для таблицы
                dataGridView_preM_restoringDevice.Rows[device].Cells[0].Value = restoringDevice[device];
            }
        }

        /// <summary>
        /// Функция вернёт вектор интенсивности восстановления
        /// </summary>
        /// <returns>Вектор интенсивности восстановления</returns>
        private List<double> GetRestoringDevice()
        {

            // Объявляем вектор интенсивности восстановления
            List<double> restoringDevice = new List<double>(Convert.ToInt32(dataGridView_preM_restoringDevice.RowCount));

            // Для каждого прибора
            for (int device = 0; device < dataGridView_preM_restoringDevice.RowCount; device++)

                // Считываем вектор интенсивности восстановления
                restoringDevice.Add(Convert.ToDouble(dataGridView_preM_restoringDevice.Rows[device].Cells[0].Value));

            // Вовзращяем вектор интенсивности восстановления
            return restoringDevice;
        }

        /// <summary>
        /// Функция установит вектор интенсивности отказов
        /// </summary>
        /// <param name="failureRates">Вектор интенсивности отказов</param>
        private void SetFailureRates(List<double> failureRates)
        {

            // Отчищаяем вектор интенсивности отказов
            dataGridView_preM_failureRates.Rows.Clear();
            dataGridView_preM_failureRates.Columns.Clear();

            // Устанавливаем количество строк и колонок для вектора интенсивности отказов
            dataGridView_preM_failureRates.RowCount = failureRates.Count;
            dataGridView_preM_failureRates.ColumnCount = 1;

            // Для каждого прибора
            for (int device = 0; device < failureRates.Count; device++) {

                // Устанавливаем заголовки строк
                dataGridView_preM_failureRates.Rows[device].HeaderCell.Value = $"Прибор {device + 1}";

                // Установим значение для таблицы
                dataGridView_preM_failureRates.Rows[device].Cells[0].Value = failureRates[device];
            }
        }

        /// <summary>
        /// Функция вернёт вектор интенсивности отказов
        /// </summary>
        /// <returns>Вектор интенсивности отказов</returns>
        private List<double> GetFailureRates()
        {

            // Объявляем вектор интенсивности отказов
            List<double> failureRates= new List<double>(Convert.ToInt32(dataGridView_preM_failureRates.RowCount));

            // Для каждого прибора
            for (int device = 0; device < dataGridView_preM_failureRates.RowCount; device++)

                // Считываем вектор интенсивности отказов
                failureRates.Add(Convert.ToDouble(dataGridView_preM_failureRates.Rows[device].Cells[0].Value));

            // Вовзращяем вектор интенсивности отказов
            return failureRates;
        }

        private void SetPreMaintenceDurations(List<List<int>> data)
        {
            SetDevicePremTypeDGV(dataGridView_preMDuration, data);
        }

        private List<List<int>> GetPreMaintenceDurations()
        {
            return GetDevicePremTypeDGV(dataGridView_preMDuration);
        }

        private void SetPreMaintenceCosts(List<List<int>> data)
        {
            SetDevicePremTypeDGV(dataGridView_preMCosts, data);
        }

        private List<List<int>> GetPreMaintenceCosts()
        {
            return GetDevicePremTypeDGV(dataGridView_preMCosts);
        }

        private void UpdateDevicePremTypeDGV(DataGridView dataGridView)
        {
            int deviceCount = (int) numeric_device_count.Value;
            int preMTypeCount = (int) numeric_preM_types_count.Value;

            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();

            dataGridView.RowCount = deviceCount;
            dataGridView.ColumnCount = preMTypeCount;

            if(deviceCount == 0 || preMTypeCount == 0)
            {
                return;
            }

            for (int device = 0; device < deviceCount; device++)
            {
                dataGridView.Rows[device].HeaderCell.Value = $"Прибор {device + 1}";
                for (int preMType = 0; preMType < preMTypeCount; preMType++)
                {
                    dataGridView.Columns[preMType].HeaderCell.Value = $"Тип ПТО {preMType + 1}";
                    dataGridView.Rows[device].Cells[preMType].Value = 0;
                }
            }
        }

        private void SetDevicePremTypeDGV(DataGridView dataGridView, List<List<int>> data)
        {
            UpdateDevicePremTypeDGV(dataGridView);

            //TODO: проверка на соответствие размерностей матриц

            for (int device = 0; device < data.Count; device++)
            {
                for (int preMType = 0; preMType < data[0].Count; preMType++)
                {
                    dataGridView.Rows[device].Cells[preMType].Value = data[device][preMType];
                }
            }
        }

        private List<List<int>> GetDevicePremTypeDGV(DataGridView dataGridView)
        {
            var result = new List<List<int>>(dataGridView.RowCount);
            for (int device = 0; device < dataGridView.RowCount; device++)
            {
                result.Add(new List<int>(dataGridView.ColumnCount));
                for (int preMType = 0; preMType < dataGridView.ColumnCount; preMType++)
                {
                    result[device].Add(Convert.ToInt32(dataGridView.Rows[device].Cells[preMType].Value));
                }
            }
            return result;
        }
        
        private void UpdatePreMaintenceGeneralDGV()
        {
            int deviceCount = (int) numeric_device_count.Value;

            dataGridView_preM_general.Rows.Clear();
            dataGridView_preM_general.RowCount = deviceCount;

            for (int device = 0; device < deviceCount; device++)
            {
                dataGridView_preM_general.Rows[device].HeaderCell.Value = $"Прибор {device + 1}";
            }
        }

        private void SetPreMaintenceGeneral(List<List<int>> data)
        {
            UpdatePreMaintenceGeneralDGV();
            SetInactionCosts(data[0]);
        }

        private List<int> GetInactionCosts()
        {
            var result = new List<int>(dataGridView_preM_general.RowCount);
            int inactionCostColumnIndex = 0;
            for(int device = 0; device < dataGridView_preM_general.RowCount; device++)
            {
                result.Add(Convert.ToInt32(dataGridView_preM_general.Rows[device].Cells[inactionCostColumnIndex].Value));
            }
            return result;
        }

        private void SetInactionCosts(List<int> data)
        {
            int inactionCostColumnIndex = 0;
            for(int device = 0; device < dataGridView_preM_general.RowCount; device++)
            {
                dataGridView_preM_general.Rows[device].Cells[inactionCostColumnIndex].Value = data[device];
            }
        }

        /// <summary>
        /// Импортируем данные из конфигурационной структуры
        /// </summary>
        /// <param name="config">Конфигурационная структура</param>
        /// <returns>True - если импорт был успешен и False иначе </returns>
        private bool ImportFromConfig(Config config)
        {

            // Устанавливаем размер буфера
            this.numeric_data_types_count.Value = config.dataTypesCount;

            // Устанавливаем количество приборов
            this.numeric_device_count.Value = config.deviceCount;

            // Устанавливаем размер буфера
            this.numeric_buffer.Value = config.buffer;

            // Устанавливаем количества заданий для каждого типа
            this.numeric_batch_count.Value = config.batchCount;

            // Устнавливаем матрицу времени выполнения
            SetProcessingTime(config.proccessingTime);

            // Устнавливаем матрицу времени переналадки
            SetChangeoverTime(config.changeoverTime);
            
            // Вернём флаг успешной загрузки данных
            return true;
        }

        /// <summary>
        /// Импортируем данные из конфигурационной ПТО структуры
        /// </summary>
        /// <param name="preMConfig">Конфигурационная ПТО структура</param>
        /// <returns>True - если импорт был успешен и False иначе </returns>
        private bool ImportFromPreMConfig(PreMConfig preMConfig)
        {

            // Импортируем данные из конфигурационной структуры
            ImportFromConfig(preMConfig.config);

            // Устанавливаем вектор интенсивности отказов
            SetFailureRates(preMConfig.failureRates);

            // Устанавливаем вектор интенсивности восстановления
            SetRestoringDevice(preMConfig.restoringDevice);

            // Устанавливаем вектор длительности ПТО
            SetPreMaintenanceTimes(preMConfig.preMaintenanceTimes);

            // Устанавливаем значение beta
            this.betaValue.Text = preMConfig.beta.ToString();

            // Вернём флаг успешной загрузки данных
            return true;
        }

        /// <summary>
        /// Функция вернёт конфигурационную структуру
        /// </summary>
        /// <returns>Конфигурационная структура</returns>
        private Config GetConfig()
        {

            // Возвращяем генерируемый конфигурационную структуру
            return new Config(
                Convert.ToInt32(numeric_data_types_count.Value),
                Convert.ToInt32(numeric_device_count.Value),
                Convert.ToInt32(numeric_buffer.Value),
                GetProcessingTime(),
                GetChangeoverTimeDict(),
                checkBox_fixed_batches.Checked,
                Convert.ToInt32(numeric_batch_count.Value)
            );
        }

        /// <summary>
        /// Функция вернёт конфигурационную ПТО структуру
        /// </summary>
        /// <returns>Конфигурационная ПТО структура</returns>
        private PreMConfig GetPreMConfig()
        {

            // Получаем нижний порог надёжности
            double.TryParse(betaValue.Text, out double beta);

            // Возвращяем генерируемый конфигурационную ПТО структуру
            return new PreMConfig(
                GetConfig(),
                GetPreMaintenanceTimes(),
                GetFailureRates(),
                GetRestoringDevice(),
                beta
            );
        }

        private TypedPreMConfiguration TypedPreMConfiguration()
        {
            var config = new TypedPreMConfiguration(GetPreMConfig())
            {
                InactionCosts = GetInactionCosts(),
                PreMaintenceTypesCount = (int)numeric_preM_types_count.Value,
                PreMaintenanceCosts = new Model.Matrix(GetPreMaintenceCosts()),
                PreMaintenanceDurations = new Model.Matrix(GetPreMaintenceDurations())
            };
            return config;

        }

    }
}