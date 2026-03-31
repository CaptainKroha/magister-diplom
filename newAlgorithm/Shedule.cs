using magisterDiplom.Model;
using magisterDiplom.Utils;
using newAlgorithm.Model;
using newAlgorithm.Service;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.PerformanceData;
using System.Drawing.Drawing2D;
using System.Linq;

namespace newAlgorithm
{
    class Shedule
    {

        /// <summary>
        /// Данная структура данных содержит информацию о конфигурации конвейерной системы
        /// Данная переменная определяет длину конвейера, как количество приборов
        /// TODO: Убрать статическое определение количества приборов, с целью масштабируемости
        /// NOTE: Данная переменная присваивается только из файлов Form1.cs и Shedule.cs 
        /// </summary>
        public static int deviceCount;

        /// <summary>
        /// Данный двумерный список представляет из себя матрицу выполнения задания на приборе l типа i
        /// TODO: Убрать статическое определение матрицы выполнения задания, с целью масштабируемости
        /// </summary>
        public static List<List<int>> proccessingTime;

        /// <summary>
        /// Данный трёхмерный список представляет из себя матрицу переналадки прибора l с задания i на задание j
        /// TODO: Убрать статическое определение матрицы переналадки приборов, с целью масштабируемости
        /// TODO: Переопределить трёхмерный матрица, как словарь с двумерными матрицами
        /// </summary>
        public static List<List<List<int>>> changeoverTime;

        /// <summary>
        /// Матрица количества данных i-ых типов в партиях,
        /// занимающих в последовательностях pi_l j-ые позиции
        /// Данная матрица имеет представление, где в строке 
        /// (строка есть позиция) все элементы 0 (столбец есть тип данных),
        /// кроме одно например для матрицы 3x3:
        /// [ [3,0,0],
        ///   [0,3,0],
        ///   [0,0,3],]
        /// TODO: Данная реализация подразумеваем перевернутую матрицу R, где количество строк, это возможные позиции в последовательность, а количество столбцов, это типы данных, необходимо будет исправить данную оплошность
        /// </summary>
        private List<List<int>> matrixR;

        /// <summary>
        /// Данная переменная определяет время выполнения пакета в последней позиции на последнем устройстве
        /// </summary>
        private int timeOfLastScheduleExecution;

        /// <summary>
        /// Данный словарь определяет для каждого прибора собственную матрицу начала времени обработки. Каждая строка матрицы представляет из себя вектор заданий.
        /// </summary>
        private Dictionary<int, List<List<int>>> startProcessing; // [deviceCount] : [maxBatchesPositions x jobCount]

        /// <summary>
        /// Данный словарь определяет для каждого прибора собственную матрицу конца времени обработки. Каждая строка матрицы представляет из себя вектор длиной в количество заданий и каждый элемент вектора это время конца обработки.
        /// </summary>
        private Dictionary<int, List<List<int>>> stopProcessing; // [deviceCount] : [maxBatchesPositions x jobCount]

        /// <summary>
        /// Экземпляр класса визуализации для отображения в Excel
        /// </summary>
        public static Visualizer viz;

        /// <summary>
        /// Представляем матрицу R в виде списка классов SheduleElement
        /// </summary>
        private List<SheduleElement> matrixRWithTime;

        /// <summary>
        /// Данный конструктор вовзращает экземпляр данного класса на основе переданного параметра матрицы R
        /// </summary>
        /// <param name="matrixR">Матрица R - количества данных i-ых типов в партиях занимающих в последовательности pi_l j-е позиции</param>
        /// <param name="deviceCount">Количество приборов в конвейерной системе</param>
        public Shedule(List<List<int>> matrixR, int deviceCount)
        {

            // Если матрицу уже существует - отчищаем её
            this.matrixR?.Clear();

            // Выполняем переопределение новой матрицы
            this.matrixR = matrixR;

            // Определяем новое количество приборов
            Shedule.deviceCount = deviceCount;

            // Инициализируем экземпляр класс для визуализации
            if (Form1.vizualizationOn)
                viz = new Visualizer(deviceCount, matrixR[0].Count);
        }

        /// <summary>
        /// Данный конструктор вовзращает экземпляр данного класса на основе переданной матрицы A
        /// </summary>
        /// <param name="matrixA">Матрица составов партий - матрица А</param>
        public Shedule(List<List<int>> matrixA)
        {
            InitMatrixR(matrixA);

            // Инициализируем экземпляр класс для визуализации
            if (Form1.vizualizationOn)
                viz = new Visualizer(deviceCount, matrixR[0].Count);
        }

        /// <summary>
        /// Данная функция инициализируем матрицу R с помощью переданной в неё матрицы A
        /// </summary>
        /// <param name="matrixA">Матрица составов партий - матрица A </param>
        private void InitMatrixR(IReadOnlyList<List<int>> matrixA)
        {

            // Отчищаем существующие матрицы R, если они есть
            matrixR?.Clear();

            // Подсчитываем количество элементов в каждой строке (sum(mi))
            // batchesList представляет из себя множество партий с одним типом
            // например, скажем для типа 1, это может быть [10, 2]. Тогда 
            // batchesList.Count = mi = 2 - Количество партий данных 1-ого типа
            // Тогда сумма всех количеств партий (batchesForAllDataTypes, так же
            // известное, как n_p), представляет из себя количество возможных позиций
            // партий в последовательности pi_l
            var maxBatchesPositions = matrixA.Sum(batchesList => batchesList.Count);

            // Объявляем максимальный количество партий (max(mi))
            var maxBatchSize = 0;

            // Высчитываем максимальный размер колонки, 
            foreach (var mi in matrixA)
                maxBatchSize = (mi.Count > maxBatchSize) ? mi.Count : maxBatchSize;

            // Создаём матрицу из 0, как [maxBatchesPositions x dataTypesCount] = [n_p x n]
            matrixR = ListUtils.InitMatrixInt(maxBatchesPositions, matrixA.Count, 0);
            
            // Данная переменная описывает позицию в последовательности pi_l
            var batchIndex = 0;

            // Пробегаемся по всем возможным пакетам - колонкам(max(mi)) матрицы R
            for (var batchSize = 0; batchSize < maxBatchSize; batchSize++)

                // Пробегаемся по всем типам данных матрицы A
                for (var dataType = 0; dataType < matrixA.Count; dataType++)

                    // Если количество партий данных i-ого типа (mi) > (h) индекс пакета
                    if (matrixA[dataType].Count > batchSize)

                        // Выполяем присвоение
                        matrixR[batchIndex++][dataType] = matrixA[dataType][batchSize];
            
        }

        /// <summary>
        /// Выполяем построение матрицы R со временем окончания заданий
        /// </summary>
        public void BuildMatrixRWithTime()
        {

            // Если матрица уже существует, отчищаем её
            matrixRWithTime?.Clear();

            // Инициализируем матрицу R со временем
            matrixRWithTime = new List<SheduleElement>();

            // Для последнего прибора вычисляем матрицу R со временем
            for (int batchIndex = 0; batchIndex < stopProcessing[stopProcessing.Count - 1].Count; batchIndex++)
            {

                // Получаем тип данных для матрицы R
                var dataType = ReturnRDataType(batchIndex);
                var jobCount = matrixR[batchIndex][dataType];
                var stopProcessingVector = stopProcessing[stopProcessing.Count - 1][batchIndex];

                // Для каждой позиции создаём элемент расписания
                SheduleElement element = new SheduleElement(
                    jobCount,
                    dataType,
                    stopProcessingVector
                );

                // Добавляем элемент в список
                matrixRWithTime.Add(element);
            }
        }

        /// <summary>
        /// Возвращаем список из элементов ScheduleElement
        /// </summary>
        /// <returns>Список из элементов ScheduleElement</returns>
        public List<SheduleElement> ReturnMatrixRWithTime()
        {

            // Если матрица не существует, вызываем функцию построения матрицы
            if (matrixRWithTime == null)
                BuildMatrixRWithTime();

            // Возвращаем список матрицы R с временем
            return matrixRWithTime;
        }

        /// <summary>
        /// Данная функция выполняет инициализацию startProcessing и stopProcessing в зависимости от матрицы matrixR
        /// </summary>
        private void CalculateShedule()
        {

            // Проверка на инициализацию словаря
            startProcessing?.Clear();
            
            // Проверка на инициализацию словаря
            stopProcessing?.Clear();

            // Выполняем инициализацию словарей
            startProcessing = new Dictionary<int, List<List<int>>>();
            stopProcessing = new Dictionary<int, List<List<int>>>();

            // Для каждого прибора выполняем перебор
            for (var device = 0; device < deviceCount; device++) { 
                List<List<int>> startProccessingMatrix = new List<List<int>>();
                List<List<int>> stopProccessingMatrix = new List<List<int>>();

                // Для каждой возможной позиции выполняем обработку
                for (var batchIndex = 0; batchIndex < matrixR.Count; batchIndex++)//количество партий
                {

                    // Получаем количество заданий в позиции batchIndex
                    var jobCount = ReturnRJobCount(batchIndex);
                    if (jobCount == -1)
                        jobCount = 1;

                    // Для временных матриц выполняем инициализацию 
                    startProccessingMatrix.Add(ListUtils.InitVectorInt(jobCount, 0));
                    stopProccessingMatrix.Add(ListUtils.InitVectorInt(jobCount, 0));
                }

                // Добавляем матрицы в словари
                startProcessing.Add(device, startProccessingMatrix);
                stopProcessing.Add(device, stopProccessingMatrix);
            }

            var previousBatchIndex = 0;
            var previousJob = 0;
            var previousDataType = 0;

            // Для каждого прибора выполняем высчитывание времён конца и начала обработки партий
            for (var device = 0; device < deviceCount; device++)
            {

                // Для каждой возможной позиции выполняем обработку
                for (var batchIndex = 0; batchIndex < matrixR.Count; batchIndex++)
                {

                    // Получаем индекс ненулевого элемента в строке
                    var dataType = ReturnRDataType(batchIndex);

                    // Перебираем все задания в партии
                    for (var job = 0; job < matrixR[batchIndex][dataType]; job++)
                    {

                        // Для каждого требования высчитываем время переналадки
                        var timeToSwitch = (dataType == previousDataType && batchIndex != 0) ? 0 : changeoverTime[0][previousDataType][dataType];

                        // Выполяем высчитывания времён начала обработки пакета batch на приборе device в позиции position
                        if (device > 0)

                            // Время начала выполнения партии batch на приборе device в позиции position
                            // высчитываем, как время конца выполнения предыдущей партии previousBatch на приборе device в предыдущей позиции 
                            //                + время переналадки на выполнение партии данного типа, которое может быть равно 0 или
                            // как время конца выполнения данной партии batch на предыдущем приборе device - 1 в текущей позиции position
                            startProcessing[device][batchIndex][job] = Math.Max(stopProcessing[device][previousBatchIndex][previousJob] + timeToSwitch, stopProcessing[device - 1][batchIndex][job]);
                        
                        else

                            // Время начала выполнения партии batch на приборе device в позиции position
                            // высчитываем, как время конца выполнения предыдущей партии previousBatch на приборе device в предыдущей позиции 
                            //                + время переналадки на выполнение партии данного типа, которое может быть равно 0.
                            startProcessing[device][batchIndex][job] = stopProcessing[device][previousBatchIndex][previousJob] + timeToSwitch;

                        stopProcessing[device][batchIndex][job] = startProcessing[device][batchIndex][job] + proccessingTime[device][dataType];

                        // Определяем данную переменную, как время выполнения пакета в последней позиции на последнем устройстве
                        timeOfLastScheduleExecution = stopProcessing[device][batchIndex][job];
                        previousBatchIndex = batchIndex;
                        previousJob = job;
                        previousDataType = dataType;
                    }
                }

                previousBatchIndex = 0;
                previousJob = 0;
                previousDataType = 0;
            }
        }

        /// <summary>
        /// Данная функция выполняет высчитывание расписания с буфером
        /// </summary>
        /// <param name="bufferSize">Размер буфера</param>
        /// <param name="dataTypesCount">Количество типов данных</param>
        private void CalculateSheduleWithBufer(int bufferSize, int dataTypesCount)
        {


            List<List<int>> newMatrixR = new List<List<int>>();
            List<List<int>> newMatrixP = new List<List<int>>();

            // Функия вернёт для переданного индекса ПЗ его тип
            int GetTypeByBatchIndex(int batchIndex)
            {

                // Из узла матрицы R вытаскиваем тип данных и количество заданий данного типа
                for (int dataType = 0; dataType < newMatrixR.Count; dataType++)

                    // Если для текущей позиции найден ПЗ
                    if (newMatrixR[dataType][batchIndex] != 0)

                        // Возвращяем тип
                        return dataType;

                // Ошибка
                return -1;
            }

            for (int dataType = 0; dataType < this.matrixR[0].Count(); dataType++)
            {
                newMatrixR.Add(new List<int>());
                newMatrixP.Add(new List<int>());
                for (int batchIndex = 0; batchIndex < this.matrixR.Count(); batchIndex++)
                {
                    newMatrixR[dataType].Add(this.matrixR[batchIndex][dataType]);
                    newMatrixP[dataType].Add((this.matrixR[batchIndex][dataType] != 0) ? 1 : 0);
                }
            }

            Dictionary<int, List<List<int>>> GetTrueDictionaryTime(){
                Dictionary<int, List<List<int>>> trueChangeoverTime = new Dictionary<int, List<List<int>>>(deviceCount);
                for (int device = 0; device < deviceCount; device++) {
                    trueChangeoverTime.Add(device, new List<List<int>>());
                    for (int fromDataType = 0; fromDataType < dataTypesCount; fromDataType++){
                        trueChangeoverTime[device].Add(new List<int>());
                        for (int toDataType = 0; toDataType < dataTypesCount; toDataType++) {
                            trueChangeoverTime[device][fromDataType].Add(changeoverTime[device][fromDataType][toDataType]);
                        }
                    }
                }
                return trueChangeoverTime;
            }

            Dictionary<int, List<List<int>>> tnMatrix = new Dictionary<int, List<List<int>>>(deviceCount);
            for (int device = 0; device < deviceCount; device++)
            {
                tnMatrix.Add(device, new List<List<int>>());
                for (int batchIndex = 0; batchIndex < newMatrixR[0].Count; batchIndex++)
                {
                    tnMatrix[device].Add(new List<int>());
                    for (int job = 0; job < newMatrixR[GetTypeByBatchIndex(batchIndex)][batchIndex]; job++)
                    {
                        tnMatrix[device][batchIndex].Add(0);
                    }
                }
            }

            // Выполняем построение матрицы времён начала заданий
            CalculationService.CalculateTnMatrix(
                newMatrixR,
                newMatrixP,
                proccessingTime,
                GetTrueDictionaryTime(),
                bufferSize,
                ref tnMatrix
            );

            // Если визуализация включена отображаем Excel
            if (Form1.vizualizationOn)
            {
                viz.CreateExcelAppList(deviceCount, dataTypesCount);
                // viz.Visualize(tnMatrix, proccessingTimeMatrix, rMatrix);
            }

            // Из последнего элемента матрицы достаём время начала обработки последнего задания
            int startTime = tnMatrix[deviceCount - 1].Last().Last();

            // Определяем время обработки последнего задания в системе
            int procTime = proccessingTime[deviceCount - 1][GetTypeByBatchIndex(newMatrixR[0].Count - 1)];

            // Определяем новое время выполнения последнего задания в расписании
            timeOfLastScheduleExecution = startTime + procTime;
        }

        /// <summary>
        /// Данная функция возвращает тип данных в матрице R в позиции batchIndex
        /// </summary>
        /// <param name="batchIndex">Индекс партии в последовательности</param>
        /// <returns>Идентификатор типа данных или -1 в случае неудачи</returns>
        public int ReturnRDataType(int batchIndex)
        {
            for (var dataType = 0; dataType < matrixR[batchIndex].Count; dataType++)
                if (matrixR[batchIndex][dataType] > 0)
                    return dataType;
            return -1;
        }

        /// <summary>
        /// Данная функция возвращает количество заданий из матрице R в позиции batchIndex
        /// </summary>
        /// <param name="batchIndex">Индекс партии в последовательности, так же известный, как h</param>
        /// <returns>Количество заданий или -1 в случае неудачи</returns>
        public int ReturnRJobCount(int batchIndex)
        {
            for (var dataType = 0; dataType < matrixR[batchIndex].Count; dataType++)
                if (matrixR[batchIndex][dataType] > 0)
                    return matrixR[batchIndex][dataType];
            return -1;
        }

        /// <summary>
        /// Выполняем построения расписания, что подразумевает поиск наилучшего времени выполнения всех пакетов заданий в разных позициях
        /// </summary>
        public void ConstructShedule(int swapCount = 0)
        {

            // Если значение количество перестановок != 0,
            // то включаем перестановки
            bool swapping = (swapCount != 0);

            // Высчитываем временные критерии
            CalculateShedule();

            // Выполняем копирование матрицы R
            var bestMatrixR = ListUtils.MatrixIntDeepCopy(matrixR);

            // Сохраняем лучший результат времени выполнения последнего задания
            var bestTime = timeOfLastScheduleExecution;

            // Определяем индекс последнего элемента для перестановок
            int swapIndex = matrixR.Count - 1;

            // До тех пор, пока не выполнили все перестановки
            while(swapIndex - 1 >= 0)
            {

                // Если счётчик перестановок включён и количество перестановок закончилось
                if (swapping && swapCount <= 0)

                    // Прекаращаем обработку
                    break;

                // Выполняем перестановку двух позиций рядом
                ListUtils.MatrixIntRowSwap(matrixR, swapIndex, swapIndex - 1);

                // Для новой матрицы выполняем высчитывание временных критерией
                CalculateShedule();

                // Если высчитаное время хуже наилучшего
                if (timeOfLastScheduleExecution > bestTime)
                {
                    // Уменьшаем количество перестановок и индекс смещения
                    swapCount--;
                    swapIndex--;

                    // Пропускаем итерацию
                    continue;
                }

                // Переопределяем наилучшее время и матрицу R
                bestTime = timeOfLastScheduleExecution;
                bestMatrixR = ListUtils.MatrixIntDeepCopy(matrixR);

                // Уменьшаем количество перестановок и индекс смещения
                swapCount--;
                swapIndex--;
            }

            // Переопределяем общее время и матрицу R
            timeOfLastScheduleExecution = bestTime;
            matrixR = bestMatrixR;
        }

        /// <summary>
        /// Данная функция выполняет построение расписания с буфером
        /// </summary>
        /// <param name="bufferSize">Размер буфера</param>
        /// <param name="dataTypesCount">Количество типов данных</param>
        /// <returns></returns>
        public void ConstructSheduleWithBuffer(int bufferSize, int dataTypesCount, int swapCount = 0)
        {

            // Если значение количество перестановок != 0,
            // то включаем перестановки
            bool swapping = (swapCount != 0);

            // Высчитываем временные критерии
            CalculateSheduleWithBufer(bufferSize, dataTypesCount);

            // Выполняем копирование матрицы R и 
            // Сохраняем лучший результат времени выполнения последнего задания
            var bestMatrixR = ListUtils.MatrixIntDeepCopy(matrixR);
            var bestTime = timeOfLastScheduleExecution;

            // Определяем индекс последнего элемента для перестановок
            int swapIndex = matrixR.Count - 1;

            // До тех пор, пока не выполнили все перестановки
            while (swapIndex - 1 >= 0)
            {

                // Если счётчик перестановок включён и количество перестановок закончилось
                if (swapping && swapCount <= 0)

                    // Прекаращаем обработку
                    break;

                // Выполняем перестановку двух позиций рядом
                ListUtils.MatrixIntRowSwap(matrixR, swapIndex, swapIndex - 1);

                // Для новой матрицы выполняем высчитывание временных критерией
                CalculateSheduleWithBufer(bufferSize, dataTypesCount);

                // Если высчитаное время хуже наилучшего
                if (bestTime <= timeOfLastScheduleExecution)
                {
                    // Уменьшаем количество перестановок и индекс смещения
                    swapCount--;
                    swapIndex--;

                    // Пропускаем итерацию
                    continue;
                }

                // Переопределяем наилучшее время и матрицу R
                bestTime = timeOfLastScheduleExecution;
                bestMatrixR = ListUtils.MatrixIntDeepCopy(matrixR);

                // Уменьшаем количество перестановок и индекс смещения
                swapCount--;
                swapIndex--;
            }

            // Переопределяем общее время и матрицу R
            timeOfLastScheduleExecution = bestTime;
            matrixR = bestMatrixR;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tz">Бог видит, tz это Тарчок Запоздалый(Неизвестная переменная)</param>
        /// <param name="crit">Переменная для записи критерий</param>
        /// <returns>Время выполнения всех заданий</returns>
        public int GetTimeWithCriterium(int tz, out int crit)
        {

            // Данная переменная необходима для подсчёта времени выполнения всех заданий в системе
            var proccessingTime = 0;

            // Выполяем построение расписания
            ConstructShedule();

            // Для каждого прибора выполняем обработку
            for (int device = 0; device < deviceCount; device++)
            {

                // Для каждой партии выполняем обработку
                for (int batchIndex = 0; batchIndex < startProcessing[device].Count; batchIndex++)
                
                    // Для каждого задания выполняем обработку
                    for (int job = 0; job < startProcessing[device][batchIndex].Count; job++)

                        // Сумируем разницу между времен конца и временем начала обработки задания job в партии batchIndex на приборе device
                        proccessingTime += stopProcessing[device][batchIndex][job] - startProcessing[device][batchIndex][job];
                    
                // Отнимаем время начала первого задания в первом пакете (наладку)
                proccessingTime -= startProcessing[device][0][0];
            }

            // Переопределяем критерий
            crit = (tz * deviceCount) - proccessingTime;
            
            // Возвращаем время выполнения последнего задания в последнем пакете на последнем приборе
            return timeOfLastScheduleExecution;
        }

        /// <summary>
        /// Возвращаем значение времени выполнения всех заданий
        /// </summary>
        /// <returns>Время выполнения всех заданий</returns>
        public int GetTime()
        {
            return timeOfLastScheduleExecution;
        }

        #region Неиспользуемые фукнции, которые страшно удалять

        /// <summary>
        /// Данная функция выполняет 
        /// </summary>
        /// <param name="position1"></param>
        /// <param name="position2"></param>
        // private void ChangeColum(int position1, int position2)
        // {
        //     var indd1 = 0;
        //     var indd2 = 0;
        //     for (var i = 0; i < matrixR[position1].Count; i++)
        //     {
        //         if (matrixR[position1][i] > 0)
        //         {
        //             indd1 = i;
        //         }
        //     }
        //     for (var i = 0; i < matrixR[position2].Count; i++)
        //     {
        //         if (matrixR[position2][i] > 0)
        //         {
        //             indd2 = i;
        //         }
        //     }
        //     var temp = matrixR[position1][indd1];
        //     matrixR[position1][indd1] = 0;
        //     matrixR[position1][indd2] = matrixR[position2][indd2];
        //     matrixR[position2][indd2] = 0;
        //     matrixR[position2][indd1] = temp;
        // }

        /// <summary>
        /// Выполняем построения расписания, что подразумевает поиск наилучшего времени выполнения всех пакетов заданий в разных позициях
        /// </summary>
        // public void ConstructSheduleCOPY()
        // {
        // 
        //     // Высчитываем временные критерии
        //     CalculateShedule();
        // 
        //     // Выполняем копирование матрицы R
        //     var matrixRCopy = ListUtils.MatrixIntDeepCopy(matrixR);
        // 
        //     // Сохраняем лучший результат времени выполнения последнего задания
        //     var tempTime = timeOfLastScheduleExecution;
        // 
        //     // Пробегаемся по всем позициям матриц R
        //     for (var batchIndex_i = 0; batchIndex_i < matrixR.Count - 1; batchIndex_i++)
        //     {
        // 
        //         // Пробегаемся по всем позициям матриц R
        //         for (var batchIndex_j = batchIndex_i + 1; batchIndex_j < matrixR.Count; batchIndex_j++)
        // 
        //             // Ранее использовалась функция "ChangeColum(i, j);", которая в результате выполняла перестановку строк
        //             // Выполяем перестановку местами строки i и j
        //             ListUtils.MatrixIntRowSwap(matrixR, batchIndex_i, batchIndex_j);
        // 
        //         // Для новой матрицы выполняем высчитывание временных критерией
        //         CalculateShedule();
        // 
        //         // Если сохранённый результат хуже, чем новый, пропускаем итерацию
        //         if (tempTime >= timeOfLastScheduleExecution)
        //             continue;
        // 
        //         // TODO: Данная строчка может являеться не корректной, так как выполняет поверхностое
        //         // копирования и после первого прохода будет ссылаться на одну и ту же матрицу
        //         matrixR = matrixRCopy;
        // 
        //         // Выполняем переопределение нового лучшего времени
        //         timeOfLastScheduleExecution = tempTime;
        //     }
        // }

        /// <summary>
        /// Данная функция выполняет построение расписания с буфером
        /// </summary>
        /// <param name="bufferSize">Размер буфера</param>
        /// <param name="dataTypesCount">Количество типов данных</param>
        /// <returns></returns>
        // public void ConstructSheduleWithBuffer1(int bufferSize, int dataTypesCount)
        // {
        // 
        //     // Высчитываем временные критерии
        //     CalculateSheduleWithBufer(bufferSize, dataTypesCount);
        // 
        //     // Выполняем копирование матрицы R
        //     var tempR = ListUtils.MatrixIntDeepCopy(matrixR);
        // 
        //     // Сохраняем лучший результат времени выполнения последнего задания
        //     var tempTime = timeOfLastScheduleExecution;
        // 
        //     // Выполяем все возможные перестановки позиций
        //     // Для всех позиций от 0 до matrixR.Count - 1 выполняем перебор
        //     for (var batchIndex_i = 0; batchIndex_i < matrixR.Count - 1; batchIndex_i++)
        //     {
        // 
        //         // Для всех позиций от pos1 + 1 до matrixR.Count выполняем все перестановку строк (представляющие из себя позицию)
        //         for (var batchIndex_j = batchIndex_i + 1; batchIndex_j < matrixR.Count; batchIndex_j++)
        // 
        //             // Ранее использовалась функция "ChangeColum(i, j);", которая в результате выполняла перестановку строку
        //             // Выполяем перестановку местами строки i и j
        //             ListUtils.MatrixIntRowSwap(matrixR, batchIndex_i, batchIndex_j);
        // 
        //         // Выполяем высчитывание расписания с буфером для всех возможных последовательностей
        //         CalculateSheduleWithBufer(bufferSize, dataTypesCount);
        // 
        //         // Если сохранённый результат хуже, чем новый, пропускаем итерацию
        //         if (tempTime >= timeOfLastScheduleExecution)
        //             continue;
        // 
        //         // TODO: Данная строчка может являеться не корректной, так как выполняет поверхностое
        //         // копирования и после первого прохода будет ссылаться на одну и ту же матрицу
        //         matrixR = tempR;
        // 
        //         // Выполняем переопределение нового лучшего времени
        //         timeOfLastScheduleExecution = tempTime;
        //     }
        // }

        #endregion
    }
}