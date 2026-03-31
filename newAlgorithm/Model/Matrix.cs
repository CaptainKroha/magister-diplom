using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace newAlgorithm.Model
{

    /// <summary>
    /// Данный класс описывает структуру данных в виде матрицы целочисленных элементов
    /// </summary>
    public class Matrix
    {

        /// <summary>
        /// Данная переменная определяет размер первого измерения. Количество строк
        /// </summary>
        public int rowCount { get; private set; }

        /// <summary>
        /// Данная переменная определяет размер второго измерения. Количество столбцов
        /// </summary>
        public int columnCount { get; private set; }

        /// <summary>
        /// Данная переменная определяет матрицу
        /// </summary>
        private List<List<int>> matrix = new List<List<int>>();

        /// <summary>
        /// Данный конструктор выполняет создание матрицы по переданному двумерному списку
        /// </summary>
        /// <param name="matrix">Двумерный список</param>
        public Matrix(List<List<int>> matrix)
        {
            this.matrix = matrix;
            this.rowCount = Convert.ToInt32(matrix.Count);
            this.columnCount = Convert.ToInt32(matrix[0].Count);

            // Выполняем проверку на одинакувую размерность элементов в двумерном списке
            // int max = Convert.ToInt32(matrix[0].Count); 
            // foreach (List<int> row in matrix)
            //     if (max != row.Count)
            //         throw new Exception();
        }

        /// <summary>
        /// Данная функция выполняем размер вектора по индексу
        /// </summary>
        /// <param name="index">Индекс получения строки элементов</param>
        /// <returns>Размер вектора по индексу</returns>
        public int GetVectorSize(int index)
        {
            return matrix[index].Count;
        }

        public int GetItem(int i, int j)
        {
            try
            {
                return matrix[i - 1][j - 1];
            }
            catch (System.Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// Данное переопределение оператора индексирования позволяет присвоить и получить элементы по индексу
        /// </summary>
        /// <param name="row">Индекс строки</param>
        /// <param name="col">Индекс колонки</param>
        /// <returns>Целочисленное значение по индексам row и col</returns>
        public int this[int row, int col]
        {

            // Определяем код для получения значения
            get
            {

                // Обрабатываем случай выхода за границы матрицы
                if (row >= this.rowCount || col >= this.columnCount)
                    return 0;

                // Возвращаем значение элемента в матрице по индексам row и col
                return this.matrix[row][col];
            }

            // Определяем код для установки значения
            set
            {

                // Обрабатываем случай выхода за границы матрицы
                if (row >= this.rowCount || col >= this.columnCount)
                    throw new IndexOutOfRangeException();

                // Обабатываем случай, когда переданный тип не соответсвует int
                if (value.GetType() != typeof(int))
                    throw new InvalidCastException();

                // Устанавливаем значение элемента в матрице по индексам row и col
                this.matrix[row][col] = value;
            }
        }

        #region Неиспользованные функции

        /// <summary>
        /// Данная статическая функция выполняет созданию матрицы из 0 [row, col]
        /// </summary>
        /// <param name="row">Количество строк</param>
        /// <param name="col">Количество колонок</param>
        /// <returns></returns>
        public static Matrix zeros(int row, int col)
        {

            // Инициализируем двумерный список из 0
            List<List<int>> array = new List<List<int>>();
            for (int i = 0; i < row; i++) {
                array.Add(new List<int>());
                for (int j = 0; j < col; j++)
                    array[i].Add(0);
            }

            // Возвращаем экземпляр класса Matrix состоящий из 0
            return new Matrix(array);
        }

        #endregion
    }
}
