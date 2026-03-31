using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newAlgorithm
{
    class Sets
    {
        /// <summary>
        /// Данная переменная определяем типы данных
        /// </summary>
        private readonly int dataTypesCount;
        private List<List<Kit>> _readySets;

        /// <summary>
        /// Данный конструктур создаёт и возвращает экземпляр класса 
        /// </summary>
        /// <param name="composition"></param>
        /// <param name="time"></param>
        public Sets(List<List<int>> composition, List<List<int>> time)
        {
            dataTypesCount = composition.Count;
            _readySets = new List<List<Kit>>();

            // Для каджого из типов данных
            for (int dataType = 0; dataType < dataTypesCount; dataType++)
            {

                // Создаём набор и добавляем их в переменную _readySets
                _readySets.Add(new List<Kit>());

                // Для каждого из списка 
                for (int j = 0; j < time[dataType].Count; j++)
                {

                    _readySets[dataType].Add(new Kit(composition[dataType], time[dataType][j]));
                }
            }
        }

        /// <summary>
        /// Данная функция формирует новый критерий на основе переданного в аргументе значения диррективных строков
        /// </summary>
        /// <returns></returns>
        public int GetNewCriterion(bool deadline)
        {
            
            // Инициализируем результирующую переменную
            int res = 0;

            // Если диррективные сроки были установлены
            if (deadline)
            {

                // Для всех строк в готовом множестве
                foreach (var row in _readySets)

                    // Для всех столбцов в строке готового множества
                    foreach (var kit in row)

                        // Выполяем обработку
                        if (kit.GetTime() > kit.GetCompositionTime())
                            res += kit.GetTime() - kit.GetCompositionTime();
                    
                // Возвращаем результат
                return res;
            }

            var count = 0;

            foreach (var row in _readySets)
            {
                count += row.Count;
                foreach (var elem in row)
                {
                    res += elem.GetTime();
                    if (res < elem.GetTime())
                        res = elem.GetTime();
                }
            }

            // Возвращаем результат
            return res / count;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="type"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        protected void AddBatches(SheduleElement sheduleElement)
        {
            var sets = new List<Kit>();
            foreach (var row in _readySets)
            {
                foreach (var elem in row)
                {
                    sets.Add(elem);
                }
            }

            sets.Sort(
                (Kit kit1, Kit kit2) => kit1.CompareTo(kit2)
                
            );

            foreach (var kit in sets)
            {

                // Если комплект не заполнен переопределяем элемент расписания
                if (!kit.IsSetAllComposition())
                    sheduleElement = kit.AddBatch(sheduleElement.getJobCount(), sheduleElement.getType(), sheduleElement.getTime());

                // Если количество заданий меньше или равно 0, прекращаем обрабтку
                if (sheduleElement.getJobCount() <= 0)
                    return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrixRWithTime"></param>
        public void GetSolution(List<SheduleElement> matrixRWithTime)
        {
            foreach (var scheduleElement in matrixRWithTime)
            {
                AddBatches(scheduleElement);
            }
        }

        #region Неиспользуемые функции

        /// <summary>
        /// Старый критерий
        /// </summary>
        /// <returns></returns>
        public int GetCriterion()
        {
            int res = 0;
            foreach (var row in _readySets)
                foreach (var elem in row)
                    if (res < elem.GetTime())
                        res = elem.GetTime();

            // Возвращаем результат
            return res;
        }

        #endregion
    }
}
