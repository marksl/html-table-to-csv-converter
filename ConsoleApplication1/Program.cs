using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string fileName = args[0];

            IEnumerable<List<string>> strings = GetResults(fileName);

            WriteResults(strings, fileName);
        }

        private static void WriteResults(IEnumerable<List<string>> strings, string fileName)
        {
            string outputFileName = Path.GetFileNameWithoutExtension(fileName) + ".csv";
            string outputFileWithPath = Path.Combine(Path.GetDirectoryName(fileName), outputFileName);
            using (var streamWriter = new StreamWriter(outputFileWithPath))
            {
                foreach (var list in strings)
                {
                    foreach (string s in list)
                    {
                        streamWriter.Write(s.Replace("&nbsp;", string.Empty));
                        streamWriter.Write(", ");
                    }

                    streamWriter.WriteLine();
                }
            }
        }

        private static IEnumerable<List<string>> GetResults(string fileName)
        {
            var strings = new List<List<string>>();

            State current = State.Questions;

            var doc = new HtmlDocument();
            doc.Load(fileName);
            foreach (HtmlNode tr in doc.DocumentNode.SelectNodes("//table[@id='studentTable']").Descendants("tr"))
            {
                var nextLine = new List<string>();
                strings.Add(nextLine);

                if (current == State.Questions)
                {
                    bool first = true;

                    foreach(var node in tr.ChildNodes)
                    {
                        if (node.Name != "td")
                            continue;

                        if (first)
                        {
                            nextLine.Add(node.InnerText);
                            first = false;
                        }
                        else
                        {
                            nextLine.Add(node.InnerText + " - Score");
                            nextLine.Add(node.InnerText + " - Attempts");
                            nextLine.Add(node.InnerText + " - Viewed Solution");
                        }
                    }

                    current = State.ItemsJunkLine;
                }
                else if (current == State.ItemsJunkLine)
                {
                    current = State.Items;
                }
                else if (current == State.Items)
                {
                    bool first = true;
                    foreach (var node in tr.ChildNodes)
                    {
                        if (node.Name != "td")
                            continue;

                        if (first)
                        {
                            nextLine.Add(node.InnerText);
                            first = false;
                        }
                        else
                        {
                            nextLine.Add(node.InnerText);
                            nextLine.Add(node.InnerText);
                            nextLine.Add(node.InnerText);
                        }
                    }

                    current = State.StudentsJunkLine;
                }
                else if (current == State.StudentsJunkLine)
                {
                    current = State.Students;
                }
                else if (current == State.Students)
                {
                    bool first = true;
                    foreach (var node in tr.ChildNodes)
                    {
                        if (node.Name != "td")
                            continue;

                        if (first)
                        {
                            nextLine.Add(node.InnerText);

                            first = false;
                        }
                        else
                        {
                            string[] parts = node.Attributes["title"].Value
                                .Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

                            nextLine.Add(parts[0].Split(':')[1]);
                            nextLine.Add(parts[1].Split(':')[1]);
                            
                            if (ViewedSolution(parts))
                            {
                                nextLine.Add("true");
                            }
                            else
                            {
                                nextLine.Add(string.Empty);
                            }
                        }
                    }
                }
            }

            return strings;
        }

        private static bool ViewedSolution(string[] parts)
        {
            return parts.Count() == 3;
        }

        #region Nested type: State

        private enum State
        {
            Questions,
            ItemsJunkLine,
            Items,
            StudentsJunkLine,
            Students
        }

        #endregion
    }
}