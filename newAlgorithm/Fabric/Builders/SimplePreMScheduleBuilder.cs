using System;

namespace magisterDiplom.Fabric.Builders
{

    /// <summary>
    /// Класс наследник от абстрактного класса фабрики
    /// </summary>
    /// <typeparam name="T">Класс по которому выполняется создание экземпляра</typeparam>
    public class SimplePreMScheduleBuilder<T> : ScheduleBuilder<T>
    {

        /// <summary>
        /// Конструктор для создание экземпляра данного класса
        /// </summary>
        public SimplePreMScheduleBuilder()
        {
            // TODO: 
        }

        public override T Build()
        {
            throw new NotImplementedException();
        }
    }
}