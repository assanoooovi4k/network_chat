using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using static System.Console;

namespace Server
{
    public class Authorization
    {
        /// <summary>
        /// Путь к файлу с базой данных
        /// </summary>
        private static string path = Environment.CurrentDirectory + "\\DataBase.json";
        private struct User
        {
            public string UserName;
            public string Password;
        }
        private static List<User> Users = new List<User>();
        /// <summary>
        /// Загрузить базу данных из файла
        /// </summary>
        public static void GetData()
        {
            try
            {
                string json = String.Empty;
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        json = sr.ReadToEnd();
                    }
                }
                if (!String.IsNullOrWhiteSpace(json))
                    Users = JsonConvert.DeserializeObject<List<User>>(json);
            }
            catch (Exception ex)
            {
                WriteLine("Error while getting data: " + ex.Message);
            }
        }
        /// <summary>
        /// Добавить пользователя в базу данных
        /// </summary>
        /// <param name="Login">Логин</param>
        /// <param name="Password">Пароль</param>
        public static bool Add(string Login, string Password)
        {
            try
            {
                bool exists = false;
                foreach(var usr in Users)
                {
                    if (usr.UserName.ToLower().Equals(Login.ToLower()) && usr.Password.Equals(Password))
                    {
                        exists = true;
                        break;
                    }
                }
                if(!exists)
                {
                    Users.Add(new User()
                    {
                        UserName = Login,
                        Password = Password
                    });
                    UpdateBase();
                    WriteLine($"New user {Login} added to DataBase");
                    return true;
                }
                else
                {
                    WriteLine($"User {Login} already Exists");
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteLine("Error while adding a new user: " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Провести попытку авторизации пользователя по указанным данным
        /// </summary>
        /// <param name="Login">Логин</param>
        /// <param name="Password">Пароль</param>
        /// <returns>Успешно ли прошла авторизация</returns>
        public static bool Authorize(string Login, string Password)
        {
            try
            {
                if (Users == null)
                {
                    WriteLine("DataBase is empty");
                    return false;
                }
                foreach (var usr in Users)
                {
                    if (usr.UserName.Equals(Login) && (usr.Password.Equals(Password)))
                    {
                        WriteLine($"User {Login} successfully authorized");
                        return true;
                    }
                }
                WriteLine($"User {Login} failed to login");
                return false;
            }
            catch(Exception ex)
            {
                WriteLine("Error while authorization: " + ex.Message);
                return false;
            }
        }
        /// <summary>
        ///Обновить файл с базой данных
        /// </summary>
        private static void UpdateBase()
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(JsonConvert.SerializeObject(Users, Formatting.Indented));
                    }
                }
                WriteLine("DataBase was updated");
                GetData();
            }
            catch (Exception ex)
            {
                WriteLine("Error while updating DataBase: " + ex.Message);
            }
        }
    }
}
