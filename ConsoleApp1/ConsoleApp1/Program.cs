using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vigener
{
    class Program
    {
        static List<char> Text = new List<char>(); // список, который будет содержать все буквы текста  

        // возвращает словарь, где в качестве ключа - буква алфавита, значение - её порядковый номер
        // без учета ё        
        static Dictionary<char, int> GetAlphabet()
        {
            // var означает, что тип переменной определяется компилятором
            var alphabet = new Dictionary<char, int>();//объявление словаря 
            var num = 0;
            for (char letter = 'а'; letter <= 'я'; letter++)
                alphabet.Add(letter, num++);  //добавляем новый элемент {буква, номер} 
            return alphabet;
        }

        // добавляет к значению словаря 1        
        // T- означает, что тип преременной может быть любым 
        static void SetCount<T>(T item, Dictionary<T, int> dic)
        {
            if (dic.ContainsKey(item)) // если в словаре присутствует ключ item
                dic[item]++;           // то к его значению добавляется 1
            else dic.Add(item, 1);     // иначе создается новый объект словаря с ключом item и значением 1 
        }

        // записывает результаты подсчета статистики в файл
        // dic - содержит статистические данные
        // pathOut - путь, где будут записаны результаты 
        static void WriteStatistic<T>(Dictionary<T, int> dic, string pathOut)
        {
            // OrderByDescending(item => item.Value) сортирует элементы словаря по убыванию значений 
            //ToDictionary(e => e.Key, e => e.Value) необходим, чтобы вновь плучить словарь
            // т.к OrderByDescending возвращает, какой-то другой тип  
            dic = dic.OrderByDescending(item => item.Value).ToDictionary(e => e.Key, e => e.Value);
            var sum = dic.Sum(item => item.Value);
            var sw = new StreamWriter(pathOut);// открыват файл для записи
            foreach (var key in dic.Keys) // dic.Keys возвращает список ключей словаря 
                sw.WriteLine(key + " " + (double)dic[key] / sum);
            sw.Close();
        }

        //записывает исходный текст без пhобелов,знаков припинания
        // все буквы приводятся к нижниму регистру
        // pathIn путь к исходному файлу
        // путь к преобразованному тексту
        static void WriteTextOnlyLetters(string pathIn, string pathOut)
        {
            var alphabet = GetAlphabet(); //(см. GetAlphabet)            
            char symbol;
            var sr = new StreamReader(pathIn, Encoding.UTF8);// входной поток
            var sw = new StreamWriter(pathOut, false, Encoding.UTF8);//выходной поток
            while (!sr.EndOfStream)//условие конца потока
            {
                symbol = char.ToLower((char)sr.Read());//считывает символ и приводит к нижниму регистру
                if (alphabet.ContainsKey(symbol))// проверяет является ли символ буквой русского алфавита
                {
                    Text.Add(symbol);// добавляет символ в список
                    sw.Write(symbol); // записывает в файл
                }
            }
            sr.Close();
            sw.Close();

        }

        // считает статистику букв по файлам, находящимся в катологе DirectoryIn   
        static void StatisticsLetters(string pathOut)
        {
            var Letters = new Dictionary<char, int>(); // ключь - буква, значение - колличество в тексте
            foreach (var symbol in Text)
                SetCount(symbol, Letters);// (см. SetCount)                        
            WriteStatistic(Letters, pathOut);// (см. WriteStatistic)
            Text.Clear();
        }