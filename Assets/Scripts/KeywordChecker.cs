using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nestor;

public class KeywordChecker
{
    private readonly NestorMorph _morph;

    /// <summary>
    /// Инициализирует экземпляр KeywordChecker.
    /// При создании загружает словари Nestor (может занять время).
    /// </summary>
    public KeywordChecker()
    {
        _morph = new NestorMorph();
    }

    /// <summary>
    /// Проверяет, содержит ли текст хотя бы N ключевых слов в любой форме.
    /// </summary>
    /// <param name="text">Анализируемый текст.</param>
    /// <param name="keywords">Список ключевых слов в именительном падеже (начальной форме).</param>
    /// <param name="n">Минимальное количество найденных ключевых слов для возврата true.</param>
    /// <returns>true, если в тексте встречаются минимум N ключевых слов (в любой форме), иначе false.</returns>
    public bool ContainsKeywords(string text, IEnumerable<string> keywords, int n = 1)
    {
        if (string.IsNullOrWhiteSpace(text) || keywords == null || n <= 0)
            return false;

        // Приводим ключевые слова к нижнему регистру (леммы в Nestor — с маленькой буквы)
        var keywordSet = new HashSet<string>(keywords.Select(k => k.ToLowerInvariant()));

        // Извлекаем слова из текста, убирая знаки препинания
        var words = Regex.Matches(text, @"\b\w+\b")
                         .Cast<Match>()
                         .Select(m => m.Value);

        int found = 0;
        var alreadyFound = new HashSet<string>();

        foreach (string word in words)
        {
            // Пытаемся получить нормальную форму (лемму) для каждого слова
            var info = _morph.WordInfo(word);
            if (info != null && info.Length > 0)
            {
                string lemma = info[0].Lemma?.Word?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(lemma) && keywordSet.Contains(lemma))
                {
                    if (alreadyFound.Add(lemma)) // считаем каждое ключевое слово только один раз
                    {
                        found++;
                        if (found >= n)
                            return true;
                    }
                }
            }
        }

        return false;
    }
}