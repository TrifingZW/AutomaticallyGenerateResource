namespace AutomaticallyGenerateResource.TrifingZW;

public class Create
{
    public static readonly List<string> List = new();
    public static readonly List<string> Files = new();

    public static void Main() => CreateR();

    static void CreateR()
    {
        string? fullBaseDirectoryName = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.FullName;
        if (fullBaseDirectoryName == null) return;

        string? programFolderPath = Directory.GetParent(fullBaseDirectoryName)?.FullName;
        if (programFolderPath == null) return;

        bool thereAreProjectFiles = false;

        string[] files = Directory.GetFiles(programFolderPath);
        foreach (var file in files)
            if (Path.GetExtension(file).Equals(".csproj"))
                thereAreProjectFiles = true;

        if (!thereAreProjectFiles)
        {
            Console.WriteLine("没有检测到项目文件");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
            return;
        }

        TraverseDirectory(programFolderPath); // 获取所有文件夹路径

        string outfile = "R.xml";
        StreamWriter stringWriter = File.CreateText(outfile);
        foreach (var file in List)
        {
            if (!Path.GetExtension(file).Equals(".png") && !Path.GetExtension(file).Equals(".mp3") && !Path.GetExtension(file).Equals(".fx")) continue;

            var relativeTo = Directory.GetParent(programFolderPath)?.FullName;

            if (relativeTo == null) continue;

            stringWriter.WriteLine(Path.GetRelativePath(relativeTo, file));

            Files.Add(Path.GetRelativePath(relativeTo, file));
        }

        stringWriter.Flush();

        OutputR(programFolderPath);
    }

    static void TraverseDirectory(string path)
    {
        if (Path.GetFileName(path).Equals("bin") || Path.GetFileName(path).Equals(".idea") || Path.GetFileName(path).Equals("obj") || Path.GetFileName(path).Equals(".Properties"))
            return;

        try
        {
            // 获取当前文件夹下的所有文件路径
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                List.Add(file);
            }

            // 获取当前文件夹下的所有子文件夹路径
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                // 递归遍历子文件夹
                TraverseDirectory(directory);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
            Console.WriteLine("An error occurred: " + e.Message);
        }
    }

    static void OutputR(string programFolderPath)
    {
        Directory.CreateDirectory($"{programFolderPath}\\Resource");
        string RPath = $"{programFolderPath}\\Resource\\R.cs";

        Dictionary<string, object> pathDictionary = new Dictionary<string, object>();

        foreach (string line in Files)
        {
            string[] parts = line.Split('\\');

            AddToDictionary(pathDictionary, line, parts);
        }

        StreamWriter stringWriter = File.CreateText(RPath);

        stringWriter.WriteLine($"namespace {Path.GetFileName(programFolderPath)}.Resource;");
        stringWriter.WriteLine("public class R \n{");
        PrintDictionary(pathDictionary, stringWriter);
        stringWriter.WriteLine("}");
        stringWriter.Flush();
    }

    static void AddToDictionary(Dictionary<string, object> dict, string line, string[] parts)
    {
        if (parts.Length == 0) return;

        string key = parts[0]; // 第一个元素作为键

        if (parts.Length == 1)
        {
            dict[key] = line.Replace('\\', '/'); 
            return;
        }

        if (!dict.ContainsKey(key))
        {
            dict[key] = new Dictionary<string, object>(); // 初始化子节点的字典
        }

        AddToDictionary((Dictionary<string, object>)dict[key], line, parts[1..]); // 递归处理子节点
    }

    static void PrintDictionary(Dictionary<string, object> dict, StreamWriter stringWriter)
    {
        foreach (var pair in dict)
        {
            if (pair.Value.GetType() == typeof(Dictionary<string, object>))
            {
                stringWriter.WriteLine($"public class {pair.Key} \n{{");
                PrintDictionary((pair.Value as Dictionary<string, object>)!, stringWriter);
                stringWriter.WriteLine("}");
            }

            if (pair.Value is string)
            {
                string str=(pair.Value as string)!;
                stringWriter.WriteLine($"public static string {Path.GetFileNameWithoutExtension(pair.Key)}=\"{str[..str.LastIndexOf(".", StringComparison.Ordinal)]}\";");
            }
        }
    }
}