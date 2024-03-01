using Dadata.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DadataApi
{
    /// <summary>
    /// Класс для хранения списка людей.
    /// </summary>
    public class List
    {
        /// <summary>
        /// Список людей.
        /// </summary
        public List<Person> People { get; set; }
        /// <summary>
        /// Конструктор класса List.
        /// </summary>
        public List()
        {
            People = new List<Person>();
        }
        /// <summary>
        /// Метод для добавления человека в список.
        /// </summary>
        /// <param name="person">Объект класса Person.</param>
        public void AddPerson(Person person)
        {
            People.Add(person);
        }
        /// <summary>
        /// Метод для отображения информации о людях из списка.
        /// </summary>
        public void Display()
        {
            foreach (var person in People)
            {
                Console.WriteLine(person);
                Console.WriteLine();
            }
        }
        /// <summary>
        /// Метод для сохранения списка людей в txt файл.
        /// </summary>
        public void saveFile()
        {
            using (StreamWriter writer = new StreamWriter("zxc.txt", append: true))
            {
                foreach (var person in People)
                {
                    writer.WriteLine(person);
                    writer.WriteLine();
                }
            }
        }
    }

    /// <summary>
    /// Класс для представления информации о человеке.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Номер телефона человека.
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Мобильный оператор телефона.
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// ФИО человека.
        /// </summary>
        public string Initials { get; set; }
        /// <summary>
        /// ИНН человека.
        /// </summary>
        public string INN { get; set; }
        /// <summary>
        /// Страна проживания.
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Пол человека.
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Переопределение метода ToString для вывода информации о человеке.
        /// </summary>
        /// <returns>Строка с информацией о человеке.</returns>
        public override string ToString()
        {
            return $" {Initials}:\n Номер телефона:  {PhoneNumber}\n Мобильный оператор:  {Operator}\n ИНН: {INN}\n Страна:  {Country}\n Пол:  {Gender}";
        }
        /// <summary>
        /// Метод для сохранения информации о человеке в файл.
        /// </summary>
        public void saveFile()
        {
            using (StreamWriter writer = new StreamWriter("zxc.txt", append: true))
            {
                writer.WriteLine(this);
                writer.WriteLine();
            }
        }
    }
    public class Request
    {
        public string Query { get; set; }
    }
    public class PrettyName
    {
        public string Result { get; set; }
        public string Gender { get; set; }
    }
    public class NumberInfo
    {
        public string Phone { get; set; }
        public string Provider { get; set; }
        public string Country { get; set; }
    }
    public class InnInfo
    {
        public List<Suggestions> Suggestions { get; set; }
    }
    public class Suggestions
    {
        public Data Data { get; set; }
    }
    public class Data
    {
        public Management Management { get; set; }
    }
    public class Management
    {
        public string Name { get; set; }
    }

    public class Program
    {
        static HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Token f5f7522b536b8f49aa10549643e8ae1caea096cf");
                httpClient.DefaultRequestHeaders.Add("X-Secret", "2265ce775e8f6b9f0b780b39388ca6d5bf7eacdc");

                List peopleList = new List();

                while (true)
                {
                    Console.WriteLine("Информации о человеке");
                    Console.WriteLine("1)добавить человека");
                    Console.WriteLine("2)отобразить данные о человеке");
                    Console.WriteLine("3)сохранить в файл");
                    Console.WriteLine("4)выход");
                    Console.Write("Ваш выбор: ");
                    int choice = Convert.ToInt32(Console.ReadLine());

                    switch (choice)
                    {
                        case 1:
                            Person personAdd = new Person();

                            Console.WriteLine("Введите номер телефона:");
                            string phoneNumber = Console.ReadLine();
                            var numberResponse = await httpClient.PostAsJsonAsync("https://cleaner.dadata.ru/api/v1/clean/phone", new[] { phoneNumber });
                            var numberResult = await numberResponse.Content.ReadFromJsonAsync<List<NumberInfo>>();

                            foreach (var numberInfo in numberResult)
                            {
                                personAdd.PhoneNumber = numberInfo.Phone;
                                personAdd.Country = numberInfo.Country;
                                personAdd.Operator = numberInfo.Provider;
                            }

                            Console.WriteLine("Введите ИНН:");
                            string inn = Console.ReadLine();
                            var innResponse = await httpClient.PostAsJsonAsync("http://suggestions.dadata.ru/suggestions/api/4_1/rs/findById/party", new Request { Query = inn });
                            var innInfo = await innResponse.Content.ReadFromJsonAsync<InnInfo>();

                            personAdd.INN = inn;
                            personAdd.Initials = innInfo.Suggestions[0].Data.Management.Name;

                            string[] names = { personAdd.Initials };
                            var nameResponse = await httpClient.PostAsJsonAsync("https://cleaner.dadata.ru/api/v1/clean/name", names);
                            var prettyNames = await nameResponse.Content.ReadFromJsonAsync<List<PrettyName>>();

                            foreach (var prettyName in prettyNames)
                            {
                                personAdd.Initials = prettyName.Result;
                                personAdd.Gender = prettyName.Gender;
                            }

                            peopleList.AddPerson(personAdd);
                            break;

                        case 2:
                            peopleList.Display();
                            break;

                        case 3:
                            peopleList.saveFile();
                            Console.WriteLine("Список людей сохранен в txt файл.");
                            break;

                        case 4:
                            Console.WriteLine("Работа звершена.");
                            return;

                        default:
                            Console.WriteLine("Неверный выбор.Попробуйте ещё раз.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
