using System;
using System.Collections.Generic;
using System.Linq;

namespace magisterDiplom.Utils
{

    /// <summary>
    /// Данный класс используется для вспомогательных преобразований
    /// </summary>
    public class ListUtils
    {

        /// <summary>
        /// Данная статическая функция выполняет инициализацию вектора длиной size заполненного элементами value
        /// </summary>
        /// <param name="size">Длина векора</param>
        /// <param name="value">Значение для заполнения вектора, по умолчанию 0</param>
        /// <returns>Список из целочисленных значений длиной size</returns>
        public static List<int> InitVectorInt(int size, int value = 0)
        {

            // Выполяем проверку входных параметров
            if (size <= 0)
                return null;

            // Выполяем инициализацию вектора
            List<int> list = new List<int>(size);
            list.AddRange(Enumerable.Repeat(value, size));

            // Возвращаем вектор
            return list;
        }

        /// <summary>
        /// Данная статическая функция выполняет инициализацию вектора высотой rowCount и шириной colCount заполненного элементами value
        /// </summary>
        /// <param name="rowCount">Количество строк</param>
        /// <param name="colCount">Количество столбцов</param>
        /// <param name="value">Значение для заполнения матрицы, по умолчанию 0</param>
        /// <returns>Массив из целочисленных значений размера [rowCount x colCount]</returns>
        public static List<List<int>> InitMatrixInt(int rowCount, int colCount, int value = 0)
        {

            // Выполяем проверку входных параметров
            if (rowCount <= 0 || colCount <= 0)
                return null;

            // Выполяем инициализацию матрицы
            List<List<int>> matrix = new List<List<int>>(rowCount);
            for (int i = 0; i < rowCount; i++)
                matrix.Add(InitVectorInt(colCount, value));

            // Возвращаем матрицы
            return matrix;
        }

        /// <summary>
        /// Данная статическая функция выполняет конкатинацию вектора в строку
        /// </summary>
        /// <param name="vector">Целочисленный список (вектор) для преобразования в строку</param>
        /// <param name="separator">Раздилитель между элементами списка, по умолчанию пробел</param>
        /// <param name="prefix">Префикс в начале всей строки, по умолчанию пустая строка</param>
        /// <param name="postfix">Постфикс в конце всей строки, по умолчанию пустая строка</param>
        /// <returns>Строка со всеми элементами разделёнными separator</returns>
        public static string VectorIntToString(List<int> vector, string separator=" ", string prefix="", string postfix="")
        {
            string result = prefix;
            int index = 1;
            foreach (var element in vector)
                result += element + ((index++ == vector.Count) ? "" : separator);
            return result + postfix;
        }

        /// <summary>
        /// Данная статическая функция выполняет конкатинацию матрицы в строки раздлённые переводами строки
        /// </summary>
        /// <param name="matrix">Вложенный список - матрица</param>
        /// <param name="separator">Разделитель между элементами крайнего списка, по умолчанию пробел</param>
        /// <param name="prefix">Префикс перед каждой строкой, по умолчанию пустая строка</param>
        /// <param name="postfix">Постфикс после каждой строки, по умолчанию пустая строка</param>
        /// <returns>Строка со всеми элементами вложенного списка разделёнными separator и переводами строк</returns>
        public static string MatrixIntToString(List<List<int>> matrix, string separator = " ", string prefix = "", string postfix = "")
        {
            string result = "";
            foreach (var list in matrix)
                result += VectorIntToString(list, separator, prefix, postfix) + Environment.NewLine;
            return result;
        }

        /// <summary>
        /// Данная статическая фукнция выполняет глубокое копирования целочисленного вектора
        /// </summary>
        /// <param name="vector">Вектор который необходимо скопировать в новый вектор</param>
        /// <returns>Скопированный вектор</returns>
        public static List<int> VectorIntDeepCopy(List<int> vector)
        {
            return vector.ToList();
        }

        /// <summary>
        /// Данная статическая фукнция выполняет глубокое копирования целочисленной матрицы
        /// </summary>
        /// <param name="matrix">Матрица которую необходимо скопировать в новую</param>
        /// <returns>Скопированная матрица</returns>
        public static List<List<int>> MatrixIntDeepCopy(List<List<int>> matrix)
        {
            return matrix.Select(VectorIntDeepCopy).ToList();
        }

        /// <summary>
        /// Данная статическая функция выполняет перестановку двух строк в матрице
        /// </summary>
        /// <param name="matrix">Матрица в которой необходимо выполнить перестановку двух строку</param>
        /// <param name="rowIndex1">Индекс первой строки для перестановки</param>
        /// <param name="rowIndex2">Индекс второй строки для перестановки</param>
        public static void MatrixIntRowSwap(List<List<int>> matrix, int rowIndex1, int rowIndex2)
        {

            // Выполняем проверку на инициализацию матрицы
            if (matrix == null)
                return;

            // Выполяем проверку на крайние случаи
            if (matrix.Count <= rowIndex1 || matrix.Count <= rowIndex2 || rowIndex1 < 0 || rowIndex2 < 0 || rowIndex1 == rowIndex2 || matrix[rowIndex1].Count != matrix[rowIndex2].Count)
                return;

            // Выполняем перестановку двух строк
            List<int> row1 = matrix[rowIndex1];
            List<int> row2 = matrix[rowIndex2];
            matrix[rowIndex1] = row2;
            matrix[rowIndex2] = row1;
        }

        /// <summary>
        /// Данная статическая функция выполняет перестановку двух колонок в матрице
        /// </summary>
        /// <param name="matrix">Матрица в которой необходимо выполнить перестановку двух колонок</param>
        /// <param name="colIndex1">Индекс первой колонки для перестановки</param>
        /// <param name="colIndex2">Индекс второй колонки для перестановки</param>
        public static void MatrixIntColumnSwap(List<List<int>> matrix, int colIndex1, int colIndex2)
        {

            // Выполняем проверку на инициализацию матрицы
            if (matrix == null)
                return;

            // Выполяем проверку на крайние случаи
            if (matrix[0].Count <= colIndex1 || matrix[0].Count <= colIndex2 || colIndex1 < 0 || colIndex2 < 0 || colIndex1 == colIndex2)
                return;

            // Выполняем перестановку двух колонок
            List<int> col = new List<int>(matrix.Count);
            for (int row = 0; row < matrix.Count; row++) { 
                col.Add(matrix[row][colIndex1]);
                matrix[row][colIndex1] = matrix[row][colIndex2];
            }
            for (int row = 0; row < matrix.Count; row++)
                matrix[row][colIndex2] = col[row];
        }

        /// <summary>
        /// Данная статическая функция выполняем сравнение двух матриц и возвращаем логический результат
        /// </summary>
        /// <param name="matrix1">Целочисленная матрица 1</param>
        /// <param name="matrix2">Целочисленная матрица 2</param>
        /// <returns>True, если матрицы идентичны, иначе False</returns>
        public static bool IsMatrixIntEqual(List<List<int>> matrix1, List<List<int>> matrix2)
        {

            // Выполняем проверку на инициализацию матрицы
            if (matrix1 == null || matrix2 == null)
                return false;

            // Выполяем проверку на размеры матриц
            if (matrix1.Count != matrix2.Count)
                return false;
            
            // Для каждой строки выполняем сравнение
            for (int row = 0; row < matrix1.Count; row++)
            {

                // Выполяем проверку на размеры матриц
                if (matrix1[row].Count != matrix2[row].Count)
                    return false;

                // Для каждой колонки выполняем сравнение
                for(int col = 0; col < matrix1[row].Count; col++)

                    // Если значения не равны возврщаем false
                    if (matrix1[row][col] != matrix2[row][col])
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Данная статическая функция выполняем поворот матрицы и возвращает новую матрицу
        /// </summary>
        /// <param name="matrix">Целочисленная матрица для поворота</param>
        /// <returns>Новая целочисленная матрица после поворота</returns>
        public static List<List<int>> MatrixIntFlip(List<List<int>> matrix)
        {

            // Выполняем проверку на null
            if (matrix == null || matrix.Count == 0)
                return null;

            // Создаём новую матрицу и выполняем её инициализацию
            List<List<int>> newMatrix = InitMatrixInt(matrix[0].Count, matrix.Count);
            for (int i = 0; i < matrix.Count; i++)
                for (int j = 0; j < matrix[0].Count; j++) {

                    // Выполняем проверку на однородность матрицы
                    if (matrix[i].Count != matrix[0].Count)
                        return null;

                    newMatrix[j][i] = matrix[i][j];
                }

            // Возвращаем результат
            return newMatrix;
        }
    }
}
