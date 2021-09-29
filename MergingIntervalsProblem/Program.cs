using MergingIntervalsProblem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MergingIntervalsProblem
{
    class Program
    {
        const string ADDED = "ADDED";
        const string REMOVED = "REMOVED";
        const string DELETED = "DELETED";
        static void Main(string[] args)
        {
            TimeSpan currentDateTime = DateTime.Now.TimeOfDay;
            List<MergeItem> items = ReadIntervalsFromFile();
            if(items == null || !items.Any())
            {
                Console.WriteLine("No data found");
                return;
            }
            items = items.OrderBy(c => c.ArrivalTime).ToList();
            List<MergeItem> mergedItems = new List<MergeItem>();
            foreach (MergeItem originalItem in items)
            {
                mergedItems.Add(originalItem);
                if (items.IndexOf(originalItem) == 0)
                    continue;

                foreach (MergeItem mergedItem in mergedItems)
                {
                    if (mergedItems.IndexOf(mergedItem) == mergedItems.Count - 1)
                        continue;
                    bool rangesOverlap = CheckRangesOverlap(mergedItem, originalItem);

                    if (originalItem.Action == ADDED)
                    {
                        if (rangesOverlap)
                        {
                            int lowestStart = mergedItem.Start < originalItem.Start ? mergedItem.Start : originalItem.Start;
                            int highestEnd = mergedItem.End > originalItem.End ? mergedItem.End : originalItem.End;
                            mergedItems.Remove(mergedItem);
                            mergedItems.Remove(originalItem);
                            mergedItems.Add(new MergeItem { ArrivalTime = DateTime.Now.TimeOfDay, Start = lowestStart, End = highestEnd, Action = ADDED });
                            break;
                        }
                    }
                    else if (originalItem.Action == REMOVED)
                    {
                        if (rangesOverlap)
                        {
                            MergeItem lastItem = mergedItems.LastOrDefault();
                            mergedItems.Remove(lastItem);
                            mergedItems.Remove(mergedItem);

                            List<int> rangeNumbers = Enumerable.Range(mergedItem.Start, mergedItem.End).ToList();

                            // Getting items withing overlapped range
                            List<MergeItem> filteredItems = items.Where(c => c.Action == ADDED && c.Start != lastItem.Start && c.End != lastItem.End && (rangeNumbers.Contains(c.Start) || rangeNumbers.Contains(c.End))).ToList();

                            if (lastItem.Start >= mergedItem.Start)
                            {
                                mergedItem.Start = filteredItems.OrderBy(c => c.Start).Select(c => c.Start).FirstOrDefault();
                            }
                            if (lastItem.End >= mergedItem.End)
                            {
                                mergedItem.End = filteredItems.OrderByDescending(c => c.End).Select(c => c.End).FirstOrDefault();
                            }
                            mergedItem.ArrivalTime = DateTime.Now.TimeOfDay;
                            mergedItems.Add(mergedItem);
                            break;
                        }
                    }
                    else if (originalItem.Action == DELETED)
                    {
                        if (rangesOverlap)
                        {
                            MergeItem lastItem = mergedItems.LastOrDefault();
                            mergedItems.Remove(lastItem);
                            mergedItems.Remove(mergedItem);

                            MergeItem firstMergeItem = new MergeItem
                            {
                                ArrivalTime = DateTime.Now.TimeOfDay,
                                Start = mergedItem.Start,
                                End = lastItem.Start,
                                Action = ADDED
                            };

                            MergeItem secondMergeItem = new MergeItem
                            {
                                ArrivalTime = DateTime.Now.TimeOfDay,
                                Start = lastItem.End,
                                End = mergedItem.End,
                                Action = ADDED
                            };

                            mergedItems.Add(firstMergeItem);
                            mergedItems.Add(secondMergeItem);
                            break;
                        }
                    }
                }
                PrintMergedIntervals();
            }

            void PrintMergedIntervals()
            {
                StringBuilder builder = new StringBuilder();
                mergedItems = mergedItems.OrderBy(c => c.End).ToList();
                foreach (var item in mergedItems)
                {
                    builder.Append($"[{item.Start},{item.End}]");
                    if (mergedItems.IndexOf(item) != mergedItems.Count - 1)
                        builder.Append(",");
                }
                Console.WriteLine(builder.ToString());
            }
        }

        static bool CheckRangesOverlap(MergeItem firstMergeItem, MergeItem secondMergeItem)
        {
            List<int> firstItemRange = Enumerable.Range(firstMergeItem.Start, firstMergeItem.End - firstMergeItem.Start).ToList();
            List<int> secondItemRange = Enumerable.Range(secondMergeItem.Start, secondMergeItem.End - secondMergeItem.Start).ToList();

            return firstItemRange.Any(c => secondItemRange.Any(d => c == d));
        }

        static List<MergeItem> ReadIntervalsFromFile()
        {
            List<MergeItem> mergedItems = new List<MergeItem>();
            bool fileExists = File.Exists(@"intervals.csv");
            if (!fileExists)
                return mergedItems;

            List<string> actionsList = new List<string>
            {
                ADDED,
                REMOVED,
                DELETED
            };
            using (StreamReader reader = new StreamReader(@"intervals.csv"))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');

                    if (values.Length > 0 && int.TryParse(values[1], out _))
                    {
                        bool validData = TimeSpan.TryParse(values[0], out _) && int.TryParse(values[1], out _) && int.Parse(values[2], out _) && actionsList.Contains(values[3]);
                        if (!validData)
                        {
                            mergedItems = new List<MergeItem>();
                            Console.WriteLine("Invalid data format");
                            break;
                        }
                        mergedItems.Add(new MergeItem
                        {
                            ArrivalTime = TimeSpan.Parse(values[0]),
                            Start = int.Parse(values[1]),
                            End = int.Parse(values[2]),
                            Action = values[3]
                        });
                    }
                }
            }
            return mergedItems;
        }
    }
}
