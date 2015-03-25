namespace Sandbox
{
    /// <summary>
    /// Contact DTO
    /// </summary>
    /// <summary lang="ru">
    /// Информация о контакте персоны
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// Person's phone number
        /// </summary>
        /// <summary lang="ru">
        /// Номер телефона
        /// </summary>
        public string Phone;

        /// <summary>
        /// Person's email
        /// </summary>
        /// <summary lang="ru">
        /// Адрес электронной почты
        /// </summary>
        public string Email;

        /// <summary>
        /// Extra data, associated with the contact
        /// </summary>
        /// <summary>
        /// Дополнительные данные, ассоциированные с контактом
        /// </summary>
        public ExtraData Extra = new ExtraData();
    }
}