using newAlgorithm.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace newAlgorithm.Service
{
    public class CalculationService
    {

        /// <summary>
        /// Матрица P представляет из себя матрицу последовательностей состоящая из 0 и 1. Структура матрица следующая [dataTypesCount x maxBatchesPositions]
        /// </summary>
        /// <param name="pMatrix">Двойной список P</param>
        /// <param name="batchIndex">Индекс партии в последовательности</param>
        /// <returns>Тип данных</returns>
        public static int CalculationDataTypeFromBatch(List<List<int>> pMatrix, int batchIndex)
        {

            // Инициализируем тип данных
            int dataType = 0;

            // До тех пор, пока не нашли
            while (true)
                if (pMatrix[dataType][batchIndex] == 1)
                    return dataType;
                else
                    dataType++;
        }

        /// <summary>
        /// Матрица P представляет из себя матрицу последовательностей состоящая из 0 и 1. Структура матрица следующая [dataTypesCount x maxBatchesPositions]
        /// </summary>
        /// <param name="pMatrix">Матрица P</param>
        /// <param name="batchIndex">Индекс партии в последовательности</param>
        /// <returns>Тип данных</returns>
        public static int CalculationDataTypeFromBatch(Matrix pMatrix, int batchIndex)
        {

            // Инициализируем тип данных
            int dataType = 0;

            // До тех пор, пока не нашли
            while (true)
                if (pMatrix[dataType, batchIndex] == 1)
                    return dataType;
                else 
                    dataType++;
        }

        /// <summary>
        /// Ультралютая функция на 500 строк непотно чего. Данная фукнция помогает высчитать время конца текущего состава пакетов
        /// </summary>
        /// <param name="rMatrix">Матрица R</param>
        /// <param name="pMatrix">Матрица P - матрица порядка обработка партий [dataTypesCount x maxBatchesPositions] = [n x n_p]</param>
        /// <param name="timeProcessing">Матрица времени выполнения</param>
        /// <param name="timeChangeover">Трёхмерная матрица времени переналадки</param>
        /// <param name="bufferSize">Целочисленный размер буфера</param>
        /// <returns></returns>
        public static void CalculateTnMatrix(
            List<List<int>> rMatrix,
            List<List<int>> pMatrix,
            List<List<int>> timeProcessing,
            Dictionary<int, List<List<int>>> timeChangeover,
            int bufferSize,
            ref Dictionary<int, List<List<int>>> tnMatrix
            )
        {
            // Количество приборов в конвейерной системе
            int deviceCount = timeProcessing.Count;

            // TreeDimMatrix tnMatrix = new TreeDimMatrix(deviceCount);

            // Количество пакетов для всех типов данных, так же известное как n_p
            int maxBatchesPositions = pMatrix[0].Count;

            // Предыдущий тип
            int previousDataType = 0;
            int previousJobCount = 0;

            // Для всех пакетов выполняем обработку. batchIndex так же известен, как h
            for (int batchIndex = 0; batchIndex < maxBatchesPositions; batchIndex++)
            {

                // Получаем узел матрицы R в позиции batchIndex
                int currentJobCount = 0; // Переменная учитыается только в условиях
                int currentDataType = 0;

                // Из узла матрицы R вытаскиваем тип данных и количество заданий данного типа
                for (int dataType = 0; dataType < rMatrix.Count; dataType++)
                    
                    // Если для текущей позиции найден ПЗ
                    if (rMatrix[dataType][batchIndex] != 0) { 
                        currentJobCount = rMatrix[dataType][batchIndex];
                        currentDataType = dataType;
                        break;
                    }
                

                // Для всех заданий выполняем обработку. job так же известен, как q
                for (int job = 0; job < currentJobCount; job++)
                {

                    // Данная функция высчитываем время
                    int getProccessingTimeOnDeviceInBatch(int _device, int _batchIndex)
                    {

                        // Время выполнения
                        int _procTime = 0;
                        int _dataType = 0;

                        // До тех пор, пока не нашли нужное время выполнения
                        while (_procTime == 0)
                        {
                            // Подсчёт времени выполнения типа данных dataType на устройстве _device
                            int _position = pMatrix[_dataType][_batchIndex];
                            
                            // Получаем время выполнения типа _dataType на приборе _device
                            int _proccessingTime = timeProcessing[_device][_dataType++];

                            // Выполяем подсчёт времени выполнения на позицию
                            _procTime = _proccessingTime * _position;
                        }

                        return _procTime;
                    }

                    // Для всех приборов выполняем обработку
                    for (int device = 0; device < deviceCount; device++)
                    {

                        // Для первого прибора выполняем обработку
                        if (device == 0)
                        {

                            // Для первого пакета в последовательности выполняем обработку
                            // 4.1 (4)
                            if (batchIndex == 0)
                            {

                                // Если данное задание является первым в пакете, добавляем его в матрицу
                                if (job == 0)

                                    // TODO: Релазиовать наладку приборов
                                    tnMatrix[device][batchIndex][job] = 0;

                                // Если данное задание не первое и не превышает размер буфера, выполняем обработку
                                if (job > 0 && job <= bufferSize)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device][batchIndex][job - 1];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTime = startTime + procTime;

                                    // Добавляем время конца выполнения задания матрицу
                                    tnMatrix[device][batchIndex][job] = stopTime;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }

                                // Если данное задание превышает размер буфера, выполняем обработку
                                // 45
                                if (job > bufferSize && job <= currentJobCount - 1)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    // int startTime = tnMatrix[device + 1, batchIndex + 1, job + 1 - 1];
                                    int startTime = tnMatrix[device][batchIndex][job - 1];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTime = startTime + procTime;

                                    // Время начала задания на следующем приборе 
                                    // int startBufferTime = tnMatrix[device + 1 + 1, batchIndex + 1, job + 1 - bufferSize];
                                    int startBufferTime = tnMatrix[device + 1][batchIndex][job - bufferSize];

                                    // Выбираем между время между концом выполнения текущего задания и началом выполнения задания в буфере на следующем приборе
                                    int resultTime = Math.Max(stopTime, startBufferTime);

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][batchIndex][job] = resultTime;
                                }

                                // Продолжаем вычисления для следующего прибора
                                continue;
                            }

                            // Для любого не первого пакета в последовательности выполняем обработку
                            if (batchIndex >= 1 && batchIndex <= maxBatchesPositions - 1)
                            {

                                // Для первого задания в пакете выполняем обработку
                                if (job == 0)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device][batchIndex - 1].Last();
                                    int procTime = getProccessingTimeOnDeviceInBatch(device, batchIndex - 1);

                                    // Высчитываем время переналадки с предыдущего типа на текущий
                                    // int changeTime = timeChangeover[device + 1, previousType, currentDataType];
                                    int changeTime = timeChangeover[device][previousDataType][currentDataType];

                                    // Высчитываем время конца выполнения задания
                                    int stopTime = changeTime + startTime + procTime;

                                    // Время начала задания на следующем приборе предыдущего пакета предыдущего задания
                                    int startBufferTime = 0;
                                    if (previousJobCount - bufferSize >= 0 && previousJobCount - bufferSize < tnMatrix[device + 1][batchIndex - 1].Count())
                                        startBufferTime = tnMatrix[device + 1][batchIndex - 1][previousJobCount - bufferSize];

                                    // Выбираем между время между концом выполнения текущего задания и началом выполнения задания в буфере на следующем приборе
                                    int result = Math.Max(stopTime, startBufferTime);

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][batchIndex][job] = result;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }

                                // Если данное задание не первое и не превышает размер буфера, выполняем обработку
                                if (job > 0 && job <= bufferSize - 1)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device][batchIndex][job - 1];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTime = startTime + procTime;

                                    // Время начала задания на следующем приборе предыдущего пакета предыдущего задания
                                    int startBufferTime = 0;
                                    if (previousJobCount - bufferSize + job >= 0 &&  previousJobCount - bufferSize + job < tnMatrix[device + 1][batchIndex - 1].Count())
                                        startBufferTime = tnMatrix[device + 1][batchIndex - 1][previousJobCount - bufferSize + job];

                                    // Выбираем между время между концом выполнения текущего задания и началом выполнения задания в буфере на следующем приборе
                                    stopTime = Math.Max(stopTime, startBufferTime);

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][batchIndex][job] = stopTime;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }

                                // Если данное задание превышает размер буфера, выполняем обработку
                                // 45
                                if (job >= bufferSize && job <= currentJobCount - 1)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device][batchIndex][job - 1];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTime = startTime + procTime;

                                    // Время начала задания на следующем приборе предыдущего пакета предыдущего задания
                                    int startBufferTime = tnMatrix[device + 1][batchIndex][job - bufferSize];

                                    // Выбираем между концом выполнения текущего задания и началом выполнения задания в буфере на следующем приборе
                                    int result = Math.Max(stopTime, startBufferTime);

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][batchIndex][job] = result;
                                }

                                // Продолжаем вычисления для следующего прибора
                                continue;
                            }
                        }

                        // Для любого не первого и не последнего прибора выполняем обработку
                        //4.3 (6)
                        if (device >= 1 && device <= deviceCount - 2)
                        {

                            // Для первого пакета в последовательности выполняем обработку
                            if(batchIndex == 0)
                            {

                                // Для первого задания в пакете выполняем обработку
                                // 49
                                if (job == 0)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device - 1][0][0];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device - 1, 0);

                                    // Высчитываем время конца выполнения задания
                                    int stopTime = startTime + procTime;

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][0][0] = stopTime;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }

                                // Если данное задание не первое и не превышает размер буфера, выполняем обработку
                                //50
                                if (job > 0 && job <= bufferSize)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device - 1][0][job];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device - 1, 0);

                                    // Высчитываем время конца выполнения задания
                                    int stopTimeCurrentJob = startTime + procTime;

                                    // Высчитываем время начала и выполнения предыдущего задания
                                    startTime = tnMatrix[device][0][job - 1];
                                    procTime = getProccessingTimeOnDeviceInBatch(device, 0);

                                    // Высчитываем время конца выполнения предыдущего задания
                                    int stopTimePreviousJob = startTime + procTime;

                                    // Выбираем между концом выполнения текущего задания и концом выполнения предыдущего задания
                                    int result = Math.Max(stopTimeCurrentJob, stopTimePreviousJob);

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][0][job] = result;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }

                                // Если данное задание превышает размер буфера, выполняем обработку
                                // 45
                                if (job > bufferSize && job <= currentJobCount - 1)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device - 1][0][job];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device - 1, 0);

                                    // Высчитываем время конца выполнения задания
                                    int stopTimeCurrentJob = startTime + procTime;

                                    // Высчитываем время начала и выполнения предыдущего задания
                                    startTime = tnMatrix[device][0][job];
                                    procTime = getProccessingTimeOnDeviceInBatch(device, 0);

                                    // Высчитываем время конца выполнения предыдущего задания
                                    int stopTimePreviousJob = startTime + procTime;

                                    // Выбираем между концом выполнения текущего задания и концом выполнения предыдущего задания
                                    int stopTime = Math.Max(stopTimeCurrentJob, stopTimePreviousJob);

                                    // Время начала задания на следующем приборе предыдущего задания через буфер
                                    int startBufferTime = tnMatrix[device + 1][0][job - bufferSize];

                                    // Выбираем между концом выполнения задания и концом выполнения задания через буфер
                                    int result = Math.Max(stopTime, startBufferTime);

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][0][job] = result;
                                }

                                // Продолжаем вычисления для следующего прибора
                                continue;
                            }

                            // Для любого не первого пакета в последовательности выполняем обработку
                            //4.4 (7)
                            if (batchIndex >= 1 && batchIndex <= maxBatchesPositions - 1)
                            {
                                
                                // Для первого задания в пакете выполняем обработку
                                if (job == 0)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device - 1][batchIndex][0];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device - 1, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTimeCurrentJob = startTime + procTime;

                                    // Высчитываем время начала и выполнения предыдущего задания
                                    startTime = tnMatrix[device][batchIndex - 1].Last();
                                    procTime = getProccessingTimeOnDeviceInBatch(device, batchIndex - 1);

                                    // Время переналадки прибора с предыдущего типа на текущий
                                    // int changeTime = timeChangeover[device + 1, previousType, currentDataType];
                                    int changeTime = timeChangeover[device][previousDataType][currentDataType];

                                    // Высчитываем время конца выполнения предыдущего задания
                                    int stopTimePreviousJob = startTime + procTime + changeTime;

                                    // Выбираем между концом выполнения текущего задания и концом выполнения предыдущего задания
                                    int stopTime = Math.Max(stopTimeCurrentJob, stopTimePreviousJob);

                                    // Время начала задания на следующем приборе предыдущего задания через буфер

                                    // Время начала задания на следующем приборе предыдущего пакета предыдущего задания
                                    int startBufferTime = 0;
                                    if (previousJobCount - bufferSize >= 0 && previousJobCount - bufferSize < tnMatrix[device + 1][batchIndex - 1].Count())
                                        startBufferTime = tnMatrix[device + 1][batchIndex - 1][previousJobCount - bufferSize];

                                    // Выбираем между концом выполнения задания и концом выполнения задания через буфер
                                    int result = Math.Max(stopTime, startBufferTime);

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][batchIndex][0] = result;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }

                                // Если данное задание не первое и не превышает размер буфера, выполняем обработку
                                //53
                                if (job > 0 && job <= bufferSize - 1)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device - 1][batchIndex][job];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device - 1, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTimeCurrentJob = startTime + procTime;

                                    // Высчитываем время начала и выполнения предыдущего задания
                                    startTime = tnMatrix[device][batchIndex][job - 1];
                                    procTime = getProccessingTimeOnDeviceInBatch(device, batchIndex);

                                    // Высчитываем время конца выполнения предыдущего задания
                                    int stopTimePreviousJob = startTime + procTime;

                                    // Выбираем между концом выполнения текущего задания и концом выполнения предыдущего задания
                                    int stopTime = Math.Max(stopTimeCurrentJob, stopTimePreviousJob);

                                    // Время начала задания на следующем приборе предыдущего задания через буфер
                                    int startBufferTime = 0;
                                    if (previousJobCount - bufferSize + job >= 0 && previousJobCount - bufferSize + job < tnMatrix[device + 1][batchIndex - 1].Count())
                                        startBufferTime = tnMatrix[device + 1][batchIndex - 1][previousJobCount - bufferSize + job];

                                    // Выбираем между концом выполнения задания и концом выполнения задания через буфер
                                    int result = Math.Max(stopTime, startBufferTime);

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][batchIndex][job] = result;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }

                                // Если данное задание превышает размер буфера, выполняем обработку
                                // 54
                                if (job >= bufferSize && job <= currentJobCount - 1)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device - 1][batchIndex][job];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device - 1, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTimeCurrentJob = startTime + procTime;

                                    // Высчитываем время начала и выполнения предыдущего задания
                                    startTime = tnMatrix[device][batchIndex][job - 1];
                                    procTime = getProccessingTimeOnDeviceInBatch(device, batchIndex);

                                    // Высчитываем время конца выполнения предыдущего задания
                                    int stopTimePreviousJob = startTime + procTime;

                                    // Выбираем между концом выполнения текущего задания и концом выполнения предыдущего задания
                                    int stopTime = Math.Max(stopTimeCurrentJob, stopTimePreviousJob);

                                    // Время начала задания на следующем приборе задания через буфер
                                    int startBufferTime = tnMatrix[device + 1][batchIndex][job - bufferSize];

                                    // Выбираем между концом выполнения задания и концом выполнения задания через буфер
                                    int result = Math.Max(stopTime, startBufferTime);

                                    // Добавляем результат в матрицу
                                    tnMatrix[device][batchIndex][job] = result;
                                }

                                // Продолжаем вычисления для следующего прибора
                                continue;
                            }
                        }

                        // Для последнего прибора выполняем обработку
                        if (device == deviceCount - 1)
                        {

                            // Для первого пакета в последовательности выполняем обработку
                            if (batchIndex == 0)
                            {

                                // Для первого задания в пакете выполняем обработку
                                if (job == 0)
                                {

                                    // Подсчитываем время выполнения для всех пакетов для предыдущих приборов
                                    int result = 0;
                                    for (int li = 0; li <= deviceCount - 2; li++)
                                        result += getProccessingTimeOnDeviceInBatch(li, 0);

                                    // Добавляем результат в матрицу
                                    tnMatrix[deviceCount - 1][0][0] = result;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }

                                // Если данное задание превышает размер буфера, выполняем обработку
                                // 45
                                // TODO: нет необходимости обрабатывать случай (job + 1 <= currentJobCount), если значение job + 1 не имзеняется динамично, так как условие прописано в 
                                if (job > 0 && job <= currentJobCount - 1)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device - 1][batchIndex][job];
                                    int procTime = getProccessingTimeOnDeviceInBatch(device - 1, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTimeCurrentJob = startTime + procTime;

                                    // Высчитываем время начала и выполнения предыдущего задания
                                    startTime = tnMatrix[device][batchIndex][job - 1];
                                    procTime = getProccessingTimeOnDeviceInBatch(deviceCount - 1, batchIndex);

                                    // Высчитываем время конца выполнения предыдущего задания
                                    int stopTimePreviousJob = startTime + procTime;

                                    // Выбираем между концом выполнения текущего и предыдущего задания
                                    int result = Math.Max(stopTimeCurrentJob, stopTimePreviousJob);

                                    // Добавляем результат в матрицу
                                    tnMatrix[deviceCount - 1][batchIndex][job] = result;
                                }

                                // Продолжаем вычисления для следующего прибора
                                continue;
                            }

                            // Дл
                            // 4.5 (9)
                            if (batchIndex >= 1 && batchIndex <= maxBatchesPositions - 1)
                            {

                                // Для первого задания в пакете выполняем 
                                if (job == 0)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[device - 1][batchIndex][job];
                                    int procTime = getProccessingTimeOnDeviceInBatch(deviceCount - 2, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTimeCurrentJob = startTime + procTime;

                                    // Высчитываем время начала и выполнения предыдущего задания
                                    startTime = tnMatrix[deviceCount - 1][batchIndex - 1].Last();
                                    procTime = getProccessingTimeOnDeviceInBatch(deviceCount - 1, batchIndex - 1);

                                    // Время переналадки с предыдущего типа на текущей
                                    // int changeTime = timeChangeover[deviceCount, previousType, currentDataType];
                                    int changeTime = timeChangeover[deviceCount - 1][previousDataType][currentDataType];

                                    // Высчитываем время конца выполнения предыдущего задания
                                    int stopTimePreviousJob = changeTime + startTime + procTime;

                                    // Выбираем между концом выполнения текущего и предыдущего задания
                                    int result = Math.Max(stopTimeCurrentJob, stopTimePreviousJob);

                                    // Добавляем результат в матрицу
                                    tnMatrix[deviceCount - 1][batchIndex][job] = result;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }


                                // Если данное задание превышает размер буфера, выполняем обработку
                                // 48
                                // TODO: нет необходимости обрабатывать случай (job + 1 <= currentJobCount), если значение job + 1 не имзеняется динамично, так как условие прописано в 
                                if (job > 0 && job <= currentJobCount - 1)
                                {

                                    // Высчитываем время начала и выполнения задания
                                    int startTime = tnMatrix[deviceCount - 2][batchIndex][job];
                                    int procTime = getProccessingTimeOnDeviceInBatch(deviceCount - 2, batchIndex);

                                    // Высчитываем время конца выполнения задания
                                    int stopTimeCurrentJob = startTime + procTime;

                                    // Высчитываем время начала и выполнения предыдущего задания
                                    startTime = tnMatrix[deviceCount - 1][batchIndex][job- 1];
                                    procTime = getProccessingTimeOnDeviceInBatch(deviceCount - 1, batchIndex);

                                    // Высчитываем время конца выполнения предыдущего задания
                                    int stopTimePreviousJob = startTime + procTime;

                                    // Выбираем между концом выполнения текущего и предыдущего задания
                                    int result = Math.Max(stopTimeCurrentJob, stopTimePreviousJob);

                                    // Добавляем результат в матрицу
                                    tnMatrix[deviceCount - 1][batchIndex][job] = result;

                                    // Продолжаем вычисления для следующего прибора
                                    continue;
                                }

                            }
                        }
                    }
                }

                // Переопределяем предыдущий тип и задание
                previousDataType = currentDataType;
                previousJobCount = currentJobCount;
            }
        }
    }
}
