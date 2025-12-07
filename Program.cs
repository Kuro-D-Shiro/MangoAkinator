using System.Text.Json;

public class Node
{
    public int Id { get; set; }
    public string Text { get; set; }
    public Node? YesNode { get; set; }
    public Node? NoNode { get; set; }

    public Node(string text, int id, Node? yesNode, Node? noNode)
    {
        Text = text;
        Id = id;
        YesNode = yesNode;
        NoNode = noNode;
    }

    public bool IsLeaf => YesNode == null && NoNode == null;
}

public class MangoAkinator
{
    private const string DbPath = "Knowledge.json";
    private List<string> reasoningPath = new();

    public Node LoadOrCreateDefault()
    {
        if (File.Exists(DbPath))
        {
            string json = File.ReadAllText(DbPath);
            var root = JsonSerializer.Deserialize<Node>(json, new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true
            });
            if (root != null) return root;

            Console.WriteLine("Файл базы знаний повреждён. Создаю новую базу...");
        }

        var nodeNaruto = new Node("Наруто", 3, null, null);
        var nodeOnePiece = new Node("One Piece", 8, null, null);
        var nodeBerserk = new Node("Берсерк", 4, null, null);
        var nodeAttackOnTitan = new Node("Атака титанов", 9, null, null);
        var nodeSoloLeveling = new Node("Solo Leveling", 6, null, null);
        var nodeNotManga = new Node("Это не манга и её разновидности", 7, null, null);

        var nodeGreenHairCharacter = new Node(
            "Есть зеленоволосый персонаж, который постоянно теряется?",
            10, nodeOnePiece, nodeNaruto);

        var nodeShonen = new Node("Это сёнэн?", 2, nodeGreenHairCharacter, null);

        var nodeHeroBetrayed = new Node(
            "Главного героя предал лучший друг?",
            12, nodeBerserk, nodeAttackOnTitan);

        nodeShonen.NoNode = nodeHeroBetrayed;

        var nodeManhwaQuestion = new Node("Это манхва?", 5, nodeSoloLeveling, nodeNotManga);

        Node rootDefault = new Node("Это манга?", 1, nodeShonen, nodeManhwaQuestion);



        Save(rootDefault);
        return rootDefault;
    }

    private void Save(Node root)
    {
        var json = JsonSerializer.Serialize(root, new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true
        });

        File.WriteAllText(DbPath, json);
    }

    private void WaitEnter()
    {
        Console.WriteLine("\nНажмите Enter, чтобы вернуться в меню...");
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
    }

    private bool IsYes(string? input) => input != null && (input.Trim().ToLower() == "д" || input.Trim().ToLower() == "да");
    private bool IsNo(string? input) => input != null && (input.Trim().ToLower() == "н" || input.Trim().ToLower() == "нет");

    public void ChooseMode(Node root)
    {
        while (true)
        {
            Console.Clear();
            Console.Write("Выберите режим:" +
                "\n1. Играть" +
                "\n2. Показать всю базу знаний" +
                "\n3. Показать конкретный элемент базы знаний" +
                "\n4. Закончить игру" +
                "\nВаш выбор:\t");
            var input = Console.ReadLine();
            if (input == "1")
            {
                Console.Clear();
                StartGame(root);
            }
            else if (input == "2")
            {
                Console.Clear();
                PrintKnowledgeBase(root);
            }
            else if (input == "3")
            {
                Console.Clear();
                PrintSpecificElement(root);
            }
            else if (input == "4")
            {
                Console.Clear();
                Console.WriteLine("До свидания!");
                break;
            }
        }
    }

    public void StartGame(Node root)
    {
        Console.Write("Добро пожаловать в Акинатор по отгадыванию манги!\nВы готовы начать? д/н\t");
        while (true)
        {
            var input = Console.ReadLine();
            if (IsYes(input))
            {
                reasoningPath.Clear();
                var curNode = root;
                Console.WriteLine();

                while (!curNode.IsLeaf)
                {
                    Console.Write($"{curNode.Text} д/н:\t");
                    input = Console.ReadLine();

                    if (IsYes(input))
                    {
                        reasoningPath.Add($"{curNode.Text} → ДА");
                        curNode = curNode.YesNode;
                    }
                    else if (IsNo(input))
                    {
                        reasoningPath.Add($"{curNode.Text} → НЕТ");
                        curNode = curNode.NoNode;
                    }
                    else
                    {
                        ClearLastLine();
                        Console.Write($"Неправильный ввод! {curNode.Text} д/н:\t");
                    }
                }

                string finalQuestion = $"Отлично! Я отгадал! Вы загадали {curNode.Text}! Я прав? д/н:\t";
                Console.Write($"\n{finalQuestion}\t");

                while (true)
                {
                    input = Console.ReadLine();
                    if (IsYes(input))
                    {
                        Console.WriteLine("Вая, какой я молодец!");

                        Console.Write("\nХочешь узнать, почему я сделал такой вывод? д/н:\t");
                        var ans = Console.ReadLine();
                        if (IsYes(ans))
                        {
                            Console.WriteLine("\n=== Анализ моего решения ===");
                            foreach (var step in reasoningPath)
                                Console.WriteLine(step);
                            Console.WriteLine($"Следовательно: {curNode.Text}");
                        }

                        WaitEnter();
                        return;
                    }
                    else if (IsNo(input))
                    {
                        Console.Write("Какой правильный ответ?\t");
                        string newAnswer = Console.ReadLine()?.Trim();

                        Console.Write("Введите вопрос, который отличает ваш вариант от моего:\t");
                        string newQuestion = Console.ReadLine()?.Trim();

                        string branch;
                        string yesOrNoPrompt = $"Для ответа \"{newAnswer}\" правильный вариант — ДА или НЕТ? д/н:\t";
                        while (true)
                        {
                            Console.Write($"{yesOrNoPrompt}\t");
                            input = Console.ReadLine();
                            if (IsYes(input)) { branch = "yes"; break; }
                            else if (IsNo(input)) { branch = "no"; break; }
                        }

                        int newId = GenerateNewId(root);
                        var newNode = new Node(newAnswer, newId, null, null);

                        int oldId = curNode.Id;
                        string oldText = curNode.Text;

                        curNode.Text = newQuestion;
                        curNode.YesNode = branch == "yes" ? newNode : new Node(oldText, oldId, null, null);
                        curNode.NoNode = branch == "no" ? newNode : new Node(oldText, oldId, null, null);

                        Save(root);
                        Console.WriteLine("\nСпасибо! Я обучился новой манге!");
                        WaitEnter();
                        return;
                    }
                    else
                    {
                        ClearLastLine();
                        Console.Write($"Неправильный ввод! {finalQuestion}\t");
                    }
                }
            }
            else if (IsNo(input))
            {
                WaitEnter();
                return;
            }
            else
            {
                ClearLastLine();
                Console.Write("Неправильный ввод! Вы готовы начать? д/н:\t");
            }
        }
    }

    private int GenerateNewId(Node root)
    {
        int maxId = 0;
        void Traverse(Node n)
        {
            maxId = Math.Max(maxId, n.Id);
            if (n.YesNode != null) Traverse(n.YesNode);
            if (n.NoNode != null) Traverse(n.NoNode);
        }
        Traverse(root);
        return maxId + 1;
    }

    private void PrintKnowledgeBase(Node root)
    {
        Console.WriteLine("=== База знаний ===");
        PrintNodeRecursive(root, "", true, "");
        WaitEnter();
    }

    private void PrintNodeRecursive(Node node, string indent, bool isLast, string branchLabel)
    {
        bool isRoot = string.IsNullOrEmpty(indent);
        var connector = isRoot ? "" : (isLast ? "└─ " : "├─ ");
        var label = string.IsNullOrEmpty(branchLabel) ? "" : $"({branchLabel}) ";

        if (node.IsLeaf)
        {
            Console.WriteLine($"{indent}{connector}{label}[{node.Id}] Ответ: {node.Text}");
            return;
        }

        Console.WriteLine($"{indent}{connector}{label}[{node.Id}] Вопрос: {node.Text}");

        var childIndent = indent + (isLast ? "   " : "│  ");
        var children = new List<(Node node, string label)>();
        if (node.YesNode != null) children.Add((node.YesNode, "Да"));
        if (node.NoNode != null) children.Add((node.NoNode, "Нет"));

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            PrintNodeRecursive(child.node, childIndent, i == children.Count - 1, child.label);
        }
    }

    private void PrintSpecificElement(Node root)
    {
        while (true)
        {
            Console.Write("Введите id элемента для поиска (или пусто для выхода):\t");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input)) return;

            if (!int.TryParse(input, out var id))
            {
                ClearLastLine();
                Console.Write("Неправильный ввод! ");
                continue;
            }

            var found = FindNodeById(root, id);
            if (found == null)
            {
                Console.WriteLine($"\nЭлемент с id {id} не найден в базе знаний.");
            }
            else
            {
                Console.WriteLine("\nНайден элемент:");
                Console.WriteLine($"Id: {found.Id}");
                Console.WriteLine($"Тип: {(found.IsLeaf ? "Ответ" : "Вопрос")}");
                Console.WriteLine($"Текст: {found.Text}");
                if (!found.IsLeaf)
                {
                    Console.WriteLine($"Да: {(found.YesNode != null ? $"[{found.YesNode.Id}] {found.YesNode.Text}" : "(нет)")}");
                    Console.WriteLine($"Нет: {(found.NoNode != null ? $"[{found.NoNode.Id}] {found.NoNode.Text}" : "(нет)")}");
                }
            }
            WaitEnter();
            return;
        }
    }

    private Node? FindNodeById(Node node, int id)
    {
        if (node.Id == id) return node;
        Node? found = null;
        if (node.YesNode != null) found = FindNodeById(node.YesNode, id);
        if (found == null && node.NoNode != null) found = FindNodeById(node.NoNode, id);
        return found;
    }

    private static void ClearLastLine()
    {
        int currentLineCursor = Console.CursorTop - 1;
        Console.SetCursorPosition(0, currentLineCursor);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        MangoAkinator akinator = new MangoAkinator();
        Node root = akinator.LoadOrCreateDefault();
        akinator.ChooseMode(root);
    }
}
