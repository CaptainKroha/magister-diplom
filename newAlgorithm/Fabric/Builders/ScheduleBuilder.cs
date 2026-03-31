namespace magisterDiplom
{
    /// <summary>
    /// Класс фабрика содержащий функцию для создание необходимого экземпляра класса T
    /// </summary>
    public abstract class ScheduleBuilder<T>
    {

        /// <summary>
        /// Выполняет создание экземпляра класса T в наследном классе
        /// </summary>
        public abstract T Build();
    }
}