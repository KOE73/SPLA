namespace SPLA.Demo.Summarizer;

/// <summary>The command line, parsed. Everything here overrides the <c>summarize:</c> section for one
/// invocation — the section is the standing decision, the switches are today's experiment.</summary>
public sealed class Options
{
    /// <summary>Folder to work in (the source is picked out of it, the results go back into it), or a
    /// file — in which case that file is the source and its folder is the target.</summary>
    public string? Target { get; private set; }

    /// <summary>Explicit project file. Otherwise found by walking up from <see cref="Target"/>.</summary>
    public string? SplaFile { get; private set; }

    /// <summary>Explicit source document, overriding the pick.</summary>
    public string? Source { get; private set; }

    /// <summary>Where results go. Default: the source's own folder.</summary>
    public string? OutDir { get; private set; }

    /// <summary>Prompt variants by name (file in prompts_dir, extension optional) or by path.</summary>
    public List<string> Prompts { get; } = new();

    /// <summary>Prompts given inline. Named <c>text1</c>, <c>text2</c>, … in output file names.</summary>
    public List<string> PromptTexts { get; } = new();

    /// <summary>Model entry ids from <c>connections:</c>. <c>all</c> = every entry in the project.</summary>
    public List<string> Models { get; } = new();

    /// <summary>LM Studio model keys to load/unload in turn. <c>all</c> = every downloaded LLM.</summary>
    public List<string> LmModels { get; } = new();

    public bool ModelsAll { get; private set; }
    public bool LmAll { get; private set; }

    /// <summary>Print what was found (folders, prompts, models) and stop.</summary>
    public bool List { get; private set; }

    /// <summary>Plan the matrix, print the cells and the output names, run nothing.</summary>
    public bool DryRun { get; private set; }

    public bool Quiet { get; private set; }
    public bool Help { get; private set; }
    public string? ReasoningLevel { get; private set; }
    public double? Temperature { get; private set; }
    public int? TimeoutSeconds { get; private set; }
    public string? PromptPlace { get; private set; }

    /// <summary>Parse failure message, or null when the line was understood.</summary>
    public string? Error { get; private set; }

    public static Options Parse(string[] args)
    {
        var o = new Options();
        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];

            string? Next(string flag)
            {
                if (i + 1 < args.Length) return args[++i];
                o.Error = $"ключ {flag} требует значение";
                return null;
            }

            switch (a.ToLowerInvariant())
            {
                case "-h" or "--help" or "/?": o.Help = true; break;
                case "--list": o.List = true; break;
                case "--dry-run": o.DryRun = true; break;
                case "--quiet": o.Quiet = true; break;

                case "--project": o.SplaFile = Next(a); break;
                case "--source": o.Source = Next(a); break;
                case "--out": o.OutDir = Next(a); break;
                case "--prompt-place": o.PromptPlace = Next(a); break;
                case "--reasoning": o.ReasoningLevel = Next(a); break;

                case "--prompt":
                    if (Next(a) is { } p) o.Prompts.Add(p);
                    break;
                case "--prompt-text":
                    if (Next(a) is { } pt) o.PromptTexts.Add(pt);
                    break;
                case "--prompt-file":
                    // A prompt kept outside prompts_dir: read it here so the runner sees plain text.
                    if (Next(a) is { } pf)
                    {
                        if (!File.Exists(pf)) o.Error = $"файл промпта не найден: {pf}";
                        else o.Prompts.Add(pf);
                    }
                    break;

                case "--model":
                    if (Next(a) is { } m)
                    {
                        if (m.Equals("all", StringComparison.OrdinalIgnoreCase)) o.ModelsAll = true;
                        else o.Models.Add(m);
                    }
                    break;
                case "--lm":
                    if (Next(a) is { } lm)
                    {
                        if (lm.Equals("all", StringComparison.OrdinalIgnoreCase)) o.LmAll = true;
                        else o.LmModels.Add(lm);
                    }
                    break;

                case "--temp":
                    if (Next(a) is { } t)
                    {
                        if (double.TryParse(t, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out var tv))
                            o.Temperature = tv;
                        else o.Error = $"--temp: не число: {t}";
                    }
                    break;
                case "--timeout":
                    if (Next(a) is { } ts)
                    {
                        if (int.TryParse(ts, out var tsv)) o.TimeoutSeconds = tsv;
                        else o.Error = $"--timeout: не число: {ts}";
                    }
                    break;

                default:
                    if (a.StartsWith('-')) { o.Error = $"неизвестный ключ: {a}"; break; }
                    if (a.EndsWith(".spla", StringComparison.OrdinalIgnoreCase)) o.SplaFile = a;
                    else if (o.Target == null) o.Target = a;
                    else o.Error = $"лишний позиционный аргумент: {a} (папка задаётся один раз)";
                    break;
            }

            if (o.Error != null) break;
        }
        return o;
    }

    public const string Usage = """
        Использование:
          Summarizer.exe <папка|файл> [ключи]

        Берёт из папки один исходный документ, прогоняет по нему матрицу
        «каждый вариант промпта × каждая модель» и кладёт результаты обратно в ту же папку
        под именем «<исходник> - резюме <дата-время> <промпт> <модель>.md».

        Что где искать:
          <папка|файл>            папка с исходником, или сам файл-исходник
          --project <файл.spla>   файл проекта (иначе ищется вверх от папки)
          --source <файл>         исходник явно, вместо автоподбора
          --out <папка>           куда писать результаты (по умолчанию — папка исходника)

        Промпты (можно повторять; если ни одного не задано — все из prompts_dir):
          --prompt <имя|путь>     вариант из prompts_dir по имени, или путь к любому .md
          --prompt-file <путь>    то же, но без догадок: строго файл
          --prompt-text "<текст>" промпт прямо в командной строке
          --prompt-place <где>    system | user | both — куда кладётся промпт

        Модели (можно повторять; если ни одной — из summarize.models, иначе первая из проекта):
          --model <id|all>        запись модели из connections: проекта
          --lm <ключ|all>         локальная модель LM Studio: загрузить её и прогнать
                                  (all = все скачанные LLM; между прогонами выгружается)

        Прочее:
          --reasoning <off|on|low|medium|high>   режим размышлений на этот прогон
          --temp <число>          температура на этот прогон
          --timeout <сек>         предел на один прогон
          --list                  показать, что нашлось (исходник, промпты, модели), и выйти
          --dry-run               показать матрицу и имена выходных файлов, ничего не запускать
          --quiet                 не транслировать поток модели в консоль

        Примеры:
          Summarizer.exe "C:\Director\20260810 110209 Совещание по ЭТН"
          Summarizer.exe "C:\Director\...\ЭТН" --prompt short --lm all
          Summarizer.exe "C:\Director\...\ЭТН" --prompt-text "Только список поручений" --model localai
          Summarizer.exe "C:\Director\...\ЭТН" --list
        """;
}
