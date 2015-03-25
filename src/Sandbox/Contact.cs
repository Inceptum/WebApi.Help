namespace Sandbox
{
    /// <summary>
    /// Contact DTO
    /// </summary>
    /// <summary lang="ru">
    /// ���������� � �������� �������
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// Person's phone number
        /// </summary>
        /// <summary lang="ru">
        /// ����� ��������
        /// </summary>
        public string Phone;

        /// <summary>
        /// Person's email
        /// </summary>
        /// <summary lang="ru">
        /// ����� ����������� �����
        /// </summary>
        public string Email;

        /// <summary>
        /// Extra data, associated with the contact
        /// </summary>
        /// <summary>
        /// �������������� ������, ��������������� � ���������
        /// </summary>
        public ExtraData Extra = new ExtraData();
    }
}