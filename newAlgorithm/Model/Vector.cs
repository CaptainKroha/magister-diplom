using magisterDiplom.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace magisterDiplom.Model
{

    /// <summary>
    /// Класс описывает вектор из целочисленных типов данных
    /// </summary>
    public class Vector
    {

        /// <summary>
        /// Инициализируем список из целочисленных типов данных
        /// </summary>
        private List<int> vector;

        /// <summary>
        /// Описываем количество элементов в векторе
        /// </summary>
        private int count;

        /// <summary>
        /// Данный конструктор инициализирует вектор длиной count заполненной 0
        /// </summary>
        /// <param name="count">Количество элементов в векторе</param>
        public Vector(int count) {
            this.count = count;
            this.vector = ListUtils.InitVectorInt(this.count, 0);
        }

        /// <summary>
        /// Данный конструктор инициализируем вектор с помощью переданного списка
        /// </summary>
        /// <param name="vector">Список по которому выполняется инициализация вектора</param>
        public Vector(List<int> vector)
        {
            this.count = vector.Count;
            this.vector = new List<int>(vector);
        }

        /// <summary>
        /// Выполняем проверку на заполненность вектора
        /// </summary>
        /// <returns>True, если вектор пустой, иначе False</returns>
        public bool isEmpty()
        {
            return vector.Count == 0;
        }

        /// <summary>
        /// Возвращает количество элементов в векторе
        /// </summary>
        /// <returns>Целочисленная переменная количество элементов в векторе</returns>
        public int getCount()
        {
            return count;
        }

        /// <summary>
        /// Данное переопределение оператора индексирования позволяет присвоить и получить элементы по индексу
        /// </summary>
        /// <param name="index">Индекс элемента вектора</param>
        /// <returns>Значение элемента по вектору</returns>
        /// <exception cref="IndexOutOfRangeException">Ошибка выхода за пределы размера вектора</exception>
        public int this[int index]
        {

            // Определяем получение элемента
            get
            {
                // Выполняем проверку на переполнение
                if (index > this.vector.Count)
                    throw new IndexOutOfRangeException();

                // Выполняем возвращат элемента вектора
                return this.vector[index];
            }

            // Определяем установку элемента
            set
            {

                // Выполняем проверку на переполнение
                if (index > this.vector.Count)
                    throw new IndexOutOfRangeException();

                // Выполняем переопределение элемента вектора
                this.vector[index] = value;
            }
        }
    }
}
