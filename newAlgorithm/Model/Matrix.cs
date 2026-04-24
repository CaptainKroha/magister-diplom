using System;
using System.Collections.Generic;

namespace newAlgorithm.Model
{

    /// <summary>
    /// Данный класс описывает структуру данных в виде матрицы целочисленных элементов
    /// </summary>
    public class Matrix
    {

        private readonly int[,] _data;

        /// <summary>
        /// Данная переменная определяет размер первого измерения. Количество строк
        /// </summary>
        public int Rows { get;}

        /// <summary>
        /// Данная переменная определяет размер второго измерения. Количество столбцов
        /// </summary>
        public int Columns { get; }

        public Matrix(int rows, int columns)
        {
            if (rows <= 0) throw new ArgumentException("Rows must be positive", nameof(rows));
            if (columns <= 0) throw new ArgumentException("Columns must be positive", nameof(columns));

            Rows = rows;
            Columns = columns;
            
            _data = new int[rows, columns];

        }


        /// <summary>
        /// Данный конструктор выполняет создание матрицы по переданному двумерному списку
        /// </summary>
        /// <param name="matrix">Двумерный список</param>
        public Matrix(List<List<int>> matrix)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            if (matrix.Count == 0) throw new ArgumentException("Matrix cannot be empty", nameof(matrix));
            if (matrix[0] == null) throw new ArgumentException("First row cannot be null", nameof(matrix));

            Rows = matrix.Count;
            Columns = matrix[0].Count;

            // Проверка, что все строки одинаковой длины
            for (int i = 1; i < Rows; i++)
            {
                if (matrix[i] == null || matrix[i].Count != Columns)
                    throw new ArgumentException($"Row {i} has invalid length");
            }

            _data = new int[Rows, Columns];

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    _data[i, j] = matrix[i][j];
                }
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
            get => _data[row, col];
            set => _data[row, col] = value;
        }

        public static List<List<int>> ToListList(Matrix _matrix)
        {
            var result = new List<List<int>>(_matrix.Rows);
            for (int i = 0; i < _matrix.Rows; ++i)
            {
                result.Add(new List<int>(_matrix.Columns));
                for (int j = 0; j < _matrix.Columns; ++j)
                {
                    result[i].Add(_matrix[i, j]);
                }
            }
            return result;
        }

    }
}
