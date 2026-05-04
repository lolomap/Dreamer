using System;
using System.Collections.Generic;
using System.Linq;
using Nestor.Poetry;
using UnityEngine;

public class PoetryLogic
{
    private static PoetryLogic _instance;
    public static PoetryLogic Instance => _instance ??= new();

    private readonly RhymeAnalyzer _rhymeAnalyzer = new();
    private readonly FootAnalyser _footAnalyzer = new();

    public float ScoreRhyme(string line1, string line2)
    {
        RhymingPair pair = _rhymeAnalyzer.ScoreRhyme(line1.Split(' ').Last(), line2.Split(' ').Last());
        return (float)pair.Score;
    }

    public int ScorePoem(string poem)
    {
        // --- Шаг 1: Разбиение на строки, очистка и извлечение последних слов ---
        string[] allLines = poem.Split(new[] { '\n' }, StringSplitOptions.None);
        var lastWords = new List<string>();
        var stanzaBreaks = new List<int>(); // индексы последних строк строф

        for (int i = 0; i < allLines.Length; i++)
        {
            string line = allLines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                // Пустая строка — завершает предыдущую строфу
                if (lastWords.Count > 0 && !stanzaBreaks.Contains(lastWords.Count - 1))
                    stanzaBreaks.Add(lastWords.Count - 1);
                continue;
            }

            // Удаляем концевые знаки препинания
            string cleaned = line.TrimEnd('.', ',', ';', '!', '?', '-', '…');
            string[] words = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            lastWords.Add(words.Length > 0 ? words.Last() : string.Empty);
        }

        // Если после последней строки нет пустой, закрываем строфу
        if (lastWords.Count > 0 && (stanzaBreaks.Count == 0 || stanzaBreaks.Last() != lastWords.Count - 1))
            stanzaBreaks.Add(lastWords.Count - 1);

        // --- Шаг 2: Матрица рифменных оценок ---
        var rhymeAnalyzer = new RhymeAnalyzer();
        int n = lastWords.Count;
        double[,] scores = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (!string.IsNullOrEmpty(lastWords[i]) && !string.IsNullOrEmpty(lastWords[j]))
                {
                    RhymingPair pair = rhymeAnalyzer.ScoreRhyme(lastWords[i], lastWords[j]);
                    scores[i, j] = pair.Score;
                    scores[j, i] = pair.Score;
                }
            }
        }

        // --- Шаг 3: Поиск лучших рифмованных пар внутри строф ---
        double totalRhymeScore = 0;
        int pairsCount = 0;
        int stanzaStart = 0;

        foreach (int stanzaEnd in stanzaBreaks)
        {
            int stanzaLength = stanzaEnd - stanzaStart + 1;
            int offset = stanzaStart;
            double maxSum = 0;
            int chosenPairs = 0;

            if (stanzaLength == 4)
            {
                var schemes = new Dictionary<string, (int, int)[]>
                {
                    { "AABB", new[] { (0, 1), (2, 3) } },
                    { "ABAB", new[] { (0, 2), (1, 3) } },
                    { "ABBA", new[] { (0, 3), (1, 2) } }
                };

                foreach (var scheme in schemes.Values)
                {
                    double sum = 0;
                    foreach (var (i, j) in scheme)
                        sum += scores[offset + i, offset + j];

                    if (sum > maxSum)
                    {
                        maxSum = sum;
                        chosenPairs = scheme.Length;
                    }
                }
            }
            else if (stanzaLength == 2)
            {
                maxSum = scores[offset, offset + 1];
                chosenPairs = 1;
            }
            else if (stanzaLength == 3)
            {
                // Варианты AAB и ABA
                double sumAAB = scores[offset, offset + 1];
                double sumABA = scores[offset, offset + 2];

                if (sumAAB >= sumABA)
                {
                    maxSum = sumAAB;
                    chosenPairs = 1; // одна пара, третья строка без рифмы
                }
                else
                {
                    maxSum = sumABA;
                    chosenPairs = 1;
                }
            }
            else
            {
                // Для строф другой длины (например, 5+) можно использовать пороговый метод
                // или считать все пары с score > 0.7, но здесь для простоты пропускаем
            }

            totalRhymeScore += maxSum;
            pairsCount += chosenPairs;
            stanzaStart = stanzaEnd + 1;
        }

        // --- Шаг 4: Перевод средней оценки в рейтинг 1–3 ---
        double avgRhyme = pairsCount > 0 ? totalRhymeScore / pairsCount : 0;
        int rhymeQuality;
        if (avgRhyme < 0.4)
            rhymeQuality = 1;
        else if (avgRhyme < 0.7)
            rhymeQuality = 2;
        else
            rhymeQuality = 3;

        // --- Шаг 5 (опционально): определение размера для информации ---
        Foot foot = _footAnalyzer.FindBestFootByPoem(poem);
        Debug.Log($"Размер стихотворения: {foot.Type}");

        // --- Вывод результатов ---
        Debug.Log($"Количество рифмованных пар: {pairsCount}");
        Debug.Log($"Сумма оценок рифмы: {totalRhymeScore:F2}");
        Debug.Log($"Средняя оценка рифмы: {avgRhyme:F2}");
        Debug.Log($"Итоговый рейтинг рифмы (1-3): {rhymeQuality}");

        return rhymeQuality;
    }
}