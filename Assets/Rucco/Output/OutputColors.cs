public static class OutputColors
{
    private static string Color(string text, char color)
    => $"${color}{text}$W";

    public static string Red(string text)
        => Color(text, 'R');

    public static string Green(string text)
        => Color(text, 'G');

    public static string Blue(string text)
        => Color(text, 'B');

    public static string Yellow(string text)
        => Color(text, 'Y');

    public static string Cyan(string text)
        => Color(text, 'C');

    public static string Magenta(string text)
        => Color(text, 'M');
}