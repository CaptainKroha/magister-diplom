using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newAlgorithm
{

    /// <summary>
    /// Данный класс определяет комплект
    /// </summary>
    class Kit : IComparable
    {

        /// <summary>
        /// Данная переменная определяется, как список и представляет из себя состав комплекта
        /// </summary>
        private readonly List<int> _composition;

        /// <summary>
        /// Данная переменная определяется, как список и представялет из себя результирующий состав комплекта
        /// </summary>
        private List<int> _readyComposition;

        /// <summary>
        /// Данная переменная определяет время формирования комплекта
        /// </summary>
        private int _time;

        /// <summary>
        /// Данная переменная определяет критерий
        /// </summary>
        private double _criterion;

        /// <summary>
        /// TODO: Разобрать данную переменную
        /// Неизвестная временная переменная относящаяся к составу комплекта переданного аргументом в конструкторе
        /// </summary>
        private int _compositionTime;

        /// <summary>
        /// Данный конструктор создаёт экземпляр класса Kit 
        /// </summary>
        /// <param name="composition">Список состава на вход комплекта</param>
        /// <param name="compositionTime">TODO: Неизвестная временная переменная</param>
        public Kit(List<int> composition, int compositionTime)
        {
            // Инициализируем переменные данного класса
            _criterion = 0.0d;                  // Инициализируем критерий
            _composition = composition;         // Инициализируем состав комплекта
            _compositionTime = compositionTime; // TODO: Разобрать данную переменную

            // Инициализируем результирующий состав комплекта
            _readyComposition = new List<int>(_composition.Count);
            
            // Добавляем ноль в результирующий состав комплекта размерностью в переданный состав комплекта
            _readyComposition.AddRange(Enumerable.Repeat(0, _composition.Count));

            // Инициализируем переменные максимума и минимума из состава комплекта
            int min = _composition.Min();  // Минимум в составе комплекта
            int max = _composition.Max();  // Максимум в составе комплекта

            // Высчитываем критейрий как отношения разницы между максимум и минимум, на максимум
            _criterion = (max - min) / max;
        }

        /// <summary>
        /// Данная функция добавляет партию в ?????
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="type">Тип данных</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public SheduleElement AddBatch(int batch, int type, List<int> time)
        {

            // Если в текущем составе элемент типа type будет равен
            // элементу этого же типа в результирующем составе
            if (_composition[type] == _readyComposition[type])

                // Возвращаем экземпляр класса SheduleElement, представляющий элемент расписания
                return new SheduleElement(batch, type, time);
            
            var difference = 0;
            if (batch >= _composition[type] - _readyComposition[type])
            {
                difference = batch - _composition[type];
                _readyComposition[type] = _composition[type];
            }
            else
            {
                _readyComposition[type] += batch;

            }

            // Вычисляем новое время формирования комплекта
            _time = time[time.Count - 1];

            // Возвращаем экземпляр класса SheduleElement, представляющий элемент расписания
            return new SheduleElement(difference, type, time);
        }

        /// <summary>
        /// Данная функция выполняет проверку заполненности результирующего состава и говорит, что комплект заполнен
        /// </summary>
        /// <returns>Значение True, если результирующий состав заполнен и False иначе</returns>
        public bool IsSetAllComposition()
        {

            // Для всех элементов комплекта выполняем обработку
            for(int elem = 0; elem < _composition.Count; elem++)

                // Если хоть один элемент из результирующего состава не будет равен переданному составу,
                // значит комплект не заполнен
                if (_readyComposition[elem] != _composition[elem]) 
                    return false;
            
            // Данный участок кода выполнятся, только если каждый элемент
            // результирующего состава будет равен элементу переданного состава
            return true;
        }

        /// <summary>
        /// Данная функция возвращает временя формирования комплекта
        /// </summary>
        /// <returns>Целочисленное значение времени</returns>
        public int GetTime()
        {

            // Возвращаем время формирования комплекта
            return this._time;
        }

        /// <summary>
        /// TODO: Разобрать данную функцию
        /// Данная функция возвращает неизвестную до сих пор переменную
        /// </summary>
        /// <returns>TODO: Разобрать данную переменную</returns>
        public int GetCompositionTime()
        {
            return _compositionTime;
        }

        /// <summary>
        /// Данная функцию возвращает вычисленный критерий комплекта, который определяется, как (max - min)/max,
        /// где max - максимальный элемент в составе и min - минимальный элемент в составе
        /// </summary>
        /// <returns>Значение критерия типа double</returns>
        public double GetCriterion()
        {
            return _criterion;
        }

        /// <summary>
        /// Данная функция выполняет сравнение двух комплектов, данного и переданного. И является
        /// реализацией интерфейса IComparable. Сравнение выполняется по значению переменной критерия _criterion
        /// </summary>
        /// <param name="kit">Объект класса Kit для выполнения сравнения с экземпляром текущего класса</param>
        /// <returns>Значения в диапазоне [-1, 1], где 1 - больше, 0 - равно, -1 - меньше</returns>
        public int CompareTo(object kit)
        {

            // Выполняем считыванние переданного критерия
            double kit_criterion = ((Kit)kit)._criterion;

            // Возвращаем результат сравнения текущего объекта с переданным, где
            // 1 - текущий больше, 0 - они равны, -1 - текущий меньше
            return (_criterion > kit_criterion) ? 1 : 
                   ((_criterion < kit_criterion) ? -1 : 0);
        }
    }
}
