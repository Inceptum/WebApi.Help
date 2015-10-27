using System.Collections.Generic;
using System.ComponentModel;

namespace Sandbox
{
    /// <summary>
    /// Contact DTO
    /// </summary>
    /// <summary lang="ru">
    /// ���������� � �������� �������
    /// </summary>
    [DisplayName("MyContract")]
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

        /// <summary>
        /// A collection of links associated with the contanct
        /// </summary>
        /// <summary lang="ru">
        /// ��������� ������, ��������������� � ���������
        /// </summary>
        public LinksCollection Links = new LinksCollection();
    }

    /// <summary>
    /// A collection of links
    /// </summary>
    /// <summary lang="ru">
    /// ��������� ������
    /// </summary>
    public class LinksCollection : Dictionary<string, string>
    {

    }

}