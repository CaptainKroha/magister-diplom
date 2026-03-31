using System.Collections.Generic;
using System.Linq;

namespace newAlgorithm.Model
{
    /// <summary>
    /// Данный класс описывает множество заданий, как контейнер
    /// </summary>
    public class JobContainer
    {

        /// <summary>
        /// Данный список представляет из себя список заданий, открытый на чтения и скрытый на запись извне. Известный как контейнер
        /// </summary>
        public List<Job> JobList { get; private set; }

        /// <summary>
        /// Данная перменная описывает позицию последнего задания в списке, известного как контейнер
        /// </summary>
        public int LastPosition { get; private set; }

        /// <summary>
        /// Данная пустой конструктор создает экземпляет, и инициализирует переменные, данного класса
        /// </summary>
        public JobContainer()
        {
            JobList = new List<Job>();
            LastPosition = 0;
        }

        /// <summary>
        /// Данная функция добавляет переданное задание в контейнер и инициализирует его, как последнее
        /// </summary>
        /// <param name="job">Задание, которое добавляется в контейнер</param>
        public void add(Job job)
        {
            JobList.Add(job);
            LastPosition++;
        }

        /// <summary>
        /// Данная функция выполняет поиск задания, по внтренней позиции задания и возвращает задание из контейнера
        /// </summary>
        /// <param name="position">Внутренняя позиция задания, по которому необходимо найти задание в контейнере</param>
        /// <returns>Задание найденное в списке или null</returns>
        public Job find(int position)
        {

            // Используем итераторы для увеличения производительности
            foreach (Job job in JobList)
            {
                if (job.Position == position)
                    return job;
            }

            // В случае, если ничего не нашли, возвращаем null
            return null;
        }

        /// <summary>
        /// Данная функция возвращает первое задание в контейнере
        /// </summary>
        /// <returns></returns>
        public Job popFirstJob()
        {
            Job first = JobList.First();
            JobList.RemoveAt(0);

            // Возвращаем первое задание
            return first;
        }
    }
}
