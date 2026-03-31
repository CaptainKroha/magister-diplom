using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace newAlgorithm.Model
{
    public class TreeDimMatrix
    {
        public List<TreeDimMatrixNode> treeDimMatrix = new List<TreeDimMatrixNode>();
        public int deviceCount;
        public Dictionary<int, int> lastPosition = new Dictionary<int, int>();

        /// <summary>
        /// Данный конструктор принимает в себя количество приборов и возвращает экземпляр данного класса
        /// </summary>
        /// <param name="deviceCount">Количество приборов</param>
        public TreeDimMatrix(int deviceCount)
        {

            // Выполняем инициализацию
            this.deviceCount = deviceCount;
            for (int i = 1; i <= deviceCount; i++)
                lastPosition.Add(i, 0);
        }

        public TreeDimMatrix(List<List<List<int>>> changeoverTime)
        {

            // Выполняем инициализацию
            this.deviceCount = changeoverTime.Count;
            for (int i = 1; i <= deviceCount; i++)
                lastPosition.Add(i, 0);

            // Для каждого устройства в матрице переналадки
            for (int device = 0; device < changeoverTime.Count; device++)
            {

                // Для каждого типа с которого происходит переналадка
                for (int fromDataType = 0; fromDataType < changeoverTime[device].Count; fromDataType++)
                {

                    // Для каждого типа на который происходит переналадки
                    for (int toDataType = 0; toDataType < changeoverTime[device][fromDataType].Count; toDataType++)
                    {

                        // Выполняем заполнение новой матрицы переналадки
                        var time = changeoverTime[device][fromDataType][toDataType];
                        if (time != 0)
                            AddNode(device + 1, fromDataType + 1, toDataType + 1, changeoverTime[device][fromDataType][toDataType]);
                        
                    }
                }
            }
        }

        /// <summary>
        /// Данная фукнция выполняет добавление нового узла в трёхмерную матрицу
        /// </summary>
        /// <param name="deviceIndex">Индекс устройства</param>
        /// <param name="fromDataTypeIndex">Индекс типа с которого выполняется переналадка</param>
        /// <param name="toDataTypeIndex">Индекс типа на который выполняется переналадка</param>
        /// <param name="timeValue">Значение времени переналадки прибора</param>
        /// <returns>Экзмепляр данного класса с добавленным узлом</returns>
        public TreeDimMatrix AddNode(int deviceIndex, int fromDataTypeIndex, int toDataTypeIndex, int timeValue)
        {

            // Инициализируем новый трёхмерный узел
            TreeDimMatrixNode node = new TreeDimMatrixNode(deviceIndex, fromDataTypeIndex, toDataTypeIndex, timeValue);

            // Добавляем созданный ранее трёхмерный узел в список
            treeDimMatrix.Add(node);

            // Выполняем высчитывание нового значения последнего элемента для прибора deviceIndex
            ++lastPosition[deviceIndex];

            // Возвращаем экзмепляр данного класса
            return this;
        }

        public int GetValue(int deviceNumber, int type, int position)
        {
            foreach (TreeDimMatrixNode node in treeDimMatrix)
            {
                if (node.device == deviceNumber && node.fromDataType == type && node.toDataType == position)
                    return node.time;
            }

            return 0;
        }
        public int this[int deviceIndex, int fromDataTypeIndex, int toDataTypeIndex]
        {

            // Определяем код для получения значения
            get
            {

                // Для каждого трёхмерного узла в списке выполяем поиск
                foreach (TreeDimMatrixNode node in treeDimMatrix)
                    if (node.device == deviceIndex && node.fromDataType == fromDataTypeIndex && node.toDataType == toDataTypeIndex)
                        return node.time;
                    

                // В случае, если не нашли значение, возвращаем 0
                return 0;
            }

            // Определяем код для установки значения
            set
            {
                // Для каждого трёхмерного узла в списке выполяем поиск
                foreach (TreeDimMatrixNode node in treeDimMatrix)
                    if (node.device == deviceIndex && node.fromDataType == fromDataTypeIndex && node.toDataType == toDataTypeIndex) {
                        node.SetTime(value);
                        break;
                    }
            }
        }

        #region Неиспользуемые функции

        /// <summary>
        /// Данная функция выполняем поиск узла трёхмерной матрицы
        /// </summary>
        /// <param name="deviceIndex">Индекс устройства</param>
        /// <param name="fromDataTypeIndex">Индекс типа с которого выполняется переналадка</param>
        /// <param name="toDataTypeIndex">Индекс типа на который выполняется переналадка</param>
        /// <returns>Экзмепляр узла трёхмерной матрицы (TreeDimMatrixNode) или null</returns>
        public TreeDimMatrixNode Find(int deviceIndex, int fromDataTypeIndex, int toDataTypeIndex)
        {

            // Для всех узлов в списке выполняем поиск по значениям
            foreach (TreeDimMatrixNode node in treeDimMatrix)
            {

                // Если значения найдены возвращаем экземпляр узла трёхмерной матрицы
                if (node.device == deviceIndex && node.fromDataType == fromDataTypeIndex && node.toDataType == toDataTypeIndex)
                    return node;
            }

            // В случае, если ничего не найдено, возвращаем null
            return null;
        }

        #endregion

    }
}
