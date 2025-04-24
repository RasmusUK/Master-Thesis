namespace EventSource.Core.Test;

public class BigObject : IEntity
{
    // 11 + (11 * 6) + (11 * 6 * 4) + (11 * 6 * 4 * 2) = 869 nodes
    public string Name { get; set; } = "Parent BigObject";
    public int Number { get; set; } = 42000;
    public double Price { get; set; } = 42000;
    public string Description { get; set; } = "This is a big object";
    public Guid Guid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Level1Node Node1 { get; set; } = new Level1Node();

    public List<Level1Node> Children { get; set; } =
        Enumerable.Range(1, 10).Select(i => new Level1Node($"Child Node {i}")).ToList();

    public Guid Id { get; } = Guid.NewGuid();
}

public class Level1Node
{
    public string Name { get; set; }
    public int Number { get; set; } = 42000;
    public double Price { get; set; } = 42000;
    public string Description { get; set; } = "This is a big object";
    public Guid Guid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Level2Node Node2 { get; set; }
    public List<Level2Node> Siblings { get; set; }

    public Level1Node(string name = "Level1 Node")
    {
        Name = name;
        Node2 = new Level2Node(name + " > Level2");
        Siblings = Enumerable
            .Range(1, 5)
            .Select(i => new Level2Node($"{name} > Sibling {i}"))
            .ToList();
    }
}

public class Level2Node
{
    public string Name { get; set; }
    public int Number { get; set; } = 42000;
    public double Price { get; set; } = 42000;
    public string Description { get; set; } = "This is a big object";
    public Guid Guid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Level3Node Node3 { get; set; }
    public List<Level3Node> Leaves { get; set; }

    public Level2Node(string name = "Level2 Node")
    {
        Name = name;
        Node3 = new Level3Node(name + " > Level3");
        Leaves = Enumerable.Range(1, 3).Select(i => new Level3Node($"{name} > Leaf {i}")).ToList();
    }
}

public class Level3Node
{
    public string Name { get; set; }
    public int Number { get; set; } = 42000;
    public double Price { get; set; } = 42000;
    public string Description { get; set; } = "This is a big object";
    public Guid Guid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Level4Node> Details { get; set; }

    public Level3Node(string name = "Level3 Node")
    {
        Name = name;
        Details = Enumerable
            .Range(1, 2)
            .Select(i => new Level4Node($"{name} > Detail {i}"))
            .ToList();
    }
}

public class Level4Node
{
    public string Info { get; set; }
    public int Number { get; set; } = 42000;
    public double Price { get; set; } = 42000;
    public string Description { get; set; } = "This is a big object";
    public Guid Guid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Level4Node(string info = "Level4 Info")
    {
        Info = info;
    }
}
