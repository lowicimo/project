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
        // функция шифроваия
        static Func<int, int, int> encrypting = (x, y) =>
        {
            var result = (x + y) % 32;
            return result;
        };
        // функция дешифровки
        public static Func<int, int, int> decrypting = (x, y) =>
        {
            var result = (x - y) % 32;
            if (result < 0) result += 32;
            return result;
        };

        // выполняет шифрование или дешифровку в зависимости от того какая функция передана в качестве Func
        // pathIn- путь к исходному тексту, pathOut - путь для записи измененного текста, keyWord - ключ
        // работает только с текстами без разделителей
        static void ChangeText(string pathIn, string pathOut, string keyWord, Func<int, int, int> n)
        {
            var sr = new StreamReader(pathIn, Encoding.UTF8);// входной поток
            var sw = new StreamWriter(pathOut, false, Encoding.UTF8);//выходной поток
            var alphabet = GetAlphabet();
            while (!sr.EndOfStream) // условие конца потока (файла)
            {
                foreach (var changeLetter in keyWord) // получаем по очереди каждую букву из keyWord
                {
                    var symbol = (char)sr.Read();//считываем символ
                    if (alphabet.ContainsKey(symbol))
                        foreach (var e in alphabet.Keys)// alphabet.Keys возвращает множество всех ключей словаря
                                                        // конструкция позволяет получить букву из alphabet, полученную после сдвига 
                            if (alphabet[e] == n(alphabet[symbol], alphabet[changeLetter])) // n функция сдвига
                            {
                                sw.Write(e); //записываем измененнную букву в файл 
                                break;
                            }
                }
            }
            sr.Close();
            sw.Close();
        }

        // возвращает список ключей словаря
        public static List<T> ToList<T>(Dictionary<T, int> dic)
        {
            var list = new List<T>();
            foreach (var k in dic.Keys)
                list.Add(k);
            return list;
        }

        // pathIn путь до файла, по которому считается статистика биграмм 
        // pathOut - путь до файла, в котором записывается результат
        // возвращает countBigrams наиболее часто встречающихся биграмм в порядке убывания частоты  
        public static List<string> StatisticsBigrams(string pathIn, string pathOut, int countBigrams)
        {
            var alphabet = GetAlphabet();
            var Bigrams = new Dictionary<string, int>(); // ключ словаря - биграмма, значение - колличество данной биграммы в тексте
            var letters = new Queue<char>(); // объявление очереди
            var sr = new StreamReader(pathIn, Encoding.UTF8);// входной поток
            while (!sr.EndOfStream)
            {
                var symbol = (char)sr.Read();
                Text.Add(symbol); // добавляем букву в список
                letters.Enqueue(symbol); // добавляем букву в очередь
                if (letters.Count == 2) // если очередь содержит 2 символа (т.е. биграмму)
                    SetCount(letters.Dequeue().ToString() + letters.Peek().ToString(), Bigrams); // Dequeue возвращает первый объект из очереди и удаляеет его, Peek возвращает первый объект из очереди, но не удаляет его
            }
            WriteStatistic(Bigrams, pathOut);//(см. WriteStatistic)

            // сортирует словарь по убыванию колличества бигграм и выбирает countBigrams наиболее часто встречающихся биграмм
            Bigrams = Bigrams.OrderByDescending(item => item.Value).Take(countBigrams).ToDictionary(e => e.Key, e => e.Value);

            return ToList(Bigrams); // (см. ToList) 
        }

        // записывает для биграммы в словарь Distance countDistance наиболее часто встречающихся расстояний
        public static void DistanceBetweeneBigrams(string bigram, Dictionary<int, int> Distance)
        {
            var letters = new Queue<char>();
            var distance = 0;
            foreach (var symbol in Text)
            {
                distance++;
                letters.Enqueue(symbol);
                if (letters.Count == 2)
                {
                    if (bigram == letters.Dequeue().ToString() + letters.Peek().ToString()) // такой же способ нахождения биграмм, как и для статистики 
                    {
                        SetCount(distance, Distance);
                        distance = 0;
                    }
                }
            }
        }

        // возврщает наиболее вероятную длину ключа
        // bigrams - список наиболее распространенных биграмм шифрованного текста
        // countDistance - выборка для наиболее частых расстояний между биграммамм
        // countLengths - колличестов наиболее вероятных длин ключа, записываются в файл, определяемый потоком sw
        // если возвращаемая длина ключа неверна  стоит заглянуть в файл и посмотреть какие еще есть варианты 
        public static int LengthKeyWord(List<string> bigrams, int countLengths, string pathOut)
        {
            var sw = new StreamWriter(pathOut, false);
            var Distance = new Dictionary<int, int>();// ключ - расстояние между биграммами, значение - количество таких расстояний
                                                      // var CountingBigrams = new List<Dictionary<int, int>>();// список, в котором для каждой из биграмм, установленнос соответствие: расстояние между биграммами - их колличество 
            foreach (var bigram in bigrams)
                DistanceBetweeneBigrams(bigram, Distance); //(см. CountingBigrams)

            // сортирует и выбирает наиболее вероятные
            Distance = Distance.OrderByDescending(item => item.Value).Take(countLengths).ToDictionary(e => e.Key, e => e.Value);

            foreach (var k in Distance.Keys)
                sw.WriteLine("{0} {1}", k, Distance[k]); // записывает в файл "расстояние количество"  
            sw.Close();
            return Distance.Min(e => e.Key); // возвращаем минимальное расстояние из выборки
        }


        // 
        static List<List<char>> ListLetters(int lengthKeyWord, int countLetters)
        {
            var statisticsLetters = new List<Dictionary<char, int>>(lengthKeyWord);
            int number = 0;
            foreach (var symbol in Text)
            {
                number++;
                var i = number % lengthKeyWord;
                if (i == 0) i = lengthKeyWord;
                if (statisticsLetters.Count == i - 1)
                    statisticsLetters.Add(new Dictionary<char, int>());
                SetCount(symbol, statisticsLetters[i - 1]);
            }
            var listLetters = new List<List<char>>(lengthKeyWord);
            for (int j = 0; j < lengthKeyWord; j++)
            {
                statisticsLetters[j] = statisticsLetters[j].OrderByDescending(item => item.Value).Take(countLetters).ToDictionary(e => e.Key, e => e.Value);
                listLetters.Add(ToList(statisticsLetters[j]));
            }
            return listLetters;
        }
        public static List<Dictionary<char, int>> GetKeyWords(string pathIn, int lengthKeyWord, int countReplace, int countShift)
        {
            var sr = new StreamReader(pathIn, Encoding.UTF8);
            var alphabet = GetAlphabet();
            var listLetters = ListLetters(lengthKeyWord, countReplace);
            char letter;
            var KeyWords = new List<Dictionary<char, int>>(lengthKeyWord);
            for (int j = 0; j < countShift; j++)
            {
                letter = (char)sr.Read();
                sr.ReadLine();

                for (int i = 0; i < countReplace; i++)
                {
                    for (int position = 0; position < listLetters.Count; position++)
                    {
                        if (i == 0 && j == 0)
                            KeyWords.Add(new Dictionary<char, int>());
                        foreach (var e in alphabet.Keys)
                            if (alphabet[e] == decrypting(alphabet[listLetters[position][i]], alphabet[letter]))
                                SetCount(e, KeyWords[position]);
                    }
                }
            }
            sr.Close();
            return KeyWords;
        }
        public static string GetKeyWord(List<Dictionary<char, int>> KeyWords, int countLetters, int CountShift, string pathOut)
        {
            var sw = new StreamWriter(pathOut, false, Encoding.UTF8);
            var listLetters = new List<List<char>>();
            for (int i = 0; i < KeyWords.Count; i++)
            {
                KeyWords[i] = KeyWords[i].OrderByDescending(item => item.Value).Take(countLetters).ToDictionary(e => e.Key, e => e.Value);
                listLetters.Add(new List<char>());
                foreach (var k in KeyWords[i].Keys)
                {
                    sw.Write("{0} {1}  ", k, KeyWords[i][k] / (float)CountShift);
                    listLetters[i].Add(k);
                }
                sw.WriteLine();
            }
            sw.Close();
            var keyWord = new char[listLetters.Count];
            for (int i = 0; i < listLetters.Count; i++)
                keyWord[i] = listLetters[i][0];
            return string.Join("", keyWord);
        }

        static void Main(string[] args)
        {

            WriteTextOnlyLetters("text.txt", "TextOnlyLetters.txt");
            StatisticsLetters("Statistics letters.txt"); //статистика


            Console.Write("Inpute keyword: ");
            string keyWord = Console.ReadLine();
            ChangeText("TextOnlyLetters.txt", "encrypted text.txt", keyWord, encrypting);// шифрование
            // расшифровка текста без ключа
            var bigrams = StatisticsBigrams("encrypted text.txt", "encrypted bigrams statistics.txt", 50);//(см. StatisticsBigrams)
            var lengthKeyWord = LengthKeyWord(bigrams, 10, "lenghtKeyWord.txt");// возвращает наиболее вероятную длину ключа
            var KeyWords = GetKeyWords("Statistics letters.txt", lengthKeyWord, 5, 4);//промежуточное значение 
            keyWord = GetKeyWord(KeyWords, 3, 4, "Key words.txt"); // наиболее вероятный ключ            
            ChangeText("encrypted text.txt", "decrypted text.txt", keyWord, decrypting);// расшифровка текста
        }
    }
}
//всем привки в этом чатике