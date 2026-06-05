namespace InrideFair.Utils;

/// <summary>
/// Сопоставление имён/текста с сигнатурами с учётом границ слов для коротких паттернов.
/// </summary>
public static class SignatureMatcher
{
    private const int ShortSignatureLength = 4;

    /// <summary>
    /// Проверить имя файла: точное совпадение или безопасное вхождение.
    /// </summary>
    public static bool MatchesFileName(string fileName, string signature)
    {
        var name = fileName.ToLowerInvariant();
        var sig = signature.ToLowerInvariant();

        if (name == sig)
            return true;

        if (sig.EndsWith(".exe", StringComparison.Ordinal) && name == sig)
            return true;

        if (sig.Contains('.') || sig.Length >= 8)
            return name == sig;

        return MatchesToken(name, sig);
    }

    /// <summary>
    /// Проверить произвольный текст (процесс, путь, значение реестра).
    /// </summary>
    public static bool MatchesText(string text, string signature)
    {
        var haystack = text.ToLowerInvariant();
        var needle = signature.ToLowerInvariant();

        if (haystack == needle)
            return true;

        if (needle.Length >= 8 || needle.Contains(' ') || needle.Contains('.'))
            return haystack.Contains(needle, StringComparison.Ordinal);

        return MatchesToken(haystack, needle);
    }

    private static bool MatchesToken(string haystack, string needle)
    {
        if (needle.Length == 0)
            return false;

        if (needle.Length >= ShortSignatureLength)
            return haystack.Contains(needle, StringComparison.Ordinal);

        var index = 0;
        while (index <= haystack.Length - needle.Length)
        {
            index = haystack.IndexOf(needle, index, StringComparison.Ordinal);
            if (index < 0)
                return false;

            var beforeOk = index == 0 || !char.IsLetterOrDigit(haystack[index - 1]);
            var afterIndex = index + needle.Length;
            var afterOk = afterIndex >= haystack.Length || !char.IsLetterOrDigit(haystack[afterIndex]);

            if (beforeOk && afterOk)
                return true;

            index++;
        }

        return false;
    }
}
