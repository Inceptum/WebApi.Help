using System.Collections.Generic;

namespace Sandbox
{
    /// <summary>
    /// User DTO
    /// </summary>
    /// <summary lang="ru">
    /// Информация о персоне
    /// </summary>
    public class User
    {
        /// <summary>
        /// Age of the person
        /// </summary>
        /// <summary lang="ru">
        /// Возраст персоны
        /// </summary>
        public int Age;

        /// <summary>
        /// Name of the person
        /// </summary>
        /// <summary lang="ru">
        /// Имя персоны
        /// </summary>
        public string Name;

        /// <summary>
        /// A gender of the person
        /// </summary>
        /// <summary lang="ru">
        /// Пол персоны
        /// </summary>
        public Gender Gender;

        /// <summary>
        /// Extra data, associated with the person
        /// </summary>
        /// <summary lang="ru">
        /// Дополнительные данные, ассоциированные с пользователем
        /// </summary>
        public ExtraData Extra = new ExtraData();

        /// <summary>
        /// List of document extra fields
        /// </summary>
        /// <summary lang="ru">
        /// Список дополнительных полей документа
        /// </summary>
        public ExtraValuesList ExtraValuesList;
    }

    /// <summary>
    /// Some extra information
    /// </summary>
    /// <summary lang="ru">
    /// Некоторая дополнительная информация
    /// </summary>
    public class ExtraData
    {
        /// <summary>
        /// The data object
        /// </summary>
        /// <summary lang="ru">
        /// Данные
        /// </summary>
        public object Data;
    }

    /// <summary>
    /// List of document extra fields
    /// </summary>
    /// <summary lang="ru">
    /// Список дополнительных полей документа
    /// </summary>
    public class ExtraValuesList : List<object>
    {
    }
}