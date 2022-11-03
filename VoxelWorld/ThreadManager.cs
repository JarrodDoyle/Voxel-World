namespace VoxelWorld;

public enum TaskType
{
    ChunkMeshingThread,
    ChunkGenerationThread,
}

public static class ThreadManager
{
    public static int MaxWorkerThreads => GetMaxThreads().Item1;
    public static int MaxIoThreads => GetMaxThreads().Item2;

    // We want to have a storage of tasks waiting to run AND a storage of currently running tasks
    private static readonly List<Task>[] TaskPool;
    private static readonly int[] NumRunningTasks;
    private static readonly int[] MaxNumRunningTasks;
    private static readonly int NumThreadTypes;

    static ThreadManager()
    {
        // TODO: MaxThreads is so fucking high. 2^15 (32k)
        // TODO: Work out some good defaults for number of threads for each task type
        NumThreadTypes = Enum.GetNames(typeof(TaskType)).Length;
        MaxNumRunningTasks = new int[NumThreadTypes];
        TaskPool = new List<Task>[NumThreadTypes];
        NumRunningTasks = new int[NumThreadTypes];

        for (var i = 0; i < NumThreadTypes; i++)
        {
            MaxNumRunningTasks[i] = 64 * (i + 1);
            TaskPool[i] = new List<Task>();
        }
    }

    public static int GetMaxRunningTasks(TaskType taskType)
    {
        return MaxNumRunningTasks[(int) taskType];
    }

    public static void SetMaxRunningTasks(TaskType taskType, int maxRunningTasks)
    {
        MaxNumRunningTasks[(int) taskType] = maxRunningTasks;
    }

    public static void AddTask(TaskType taskType, Task task)
    {
        TaskPool[(int) taskType].Add(task);
    }

    public static void Update()
    {
        for (var i = 0; i < NumThreadTypes; i++)
        {
            // Remove completed tasks and update the number of currently running tasks
            // TODO: Tasks waiting to be scheduled aren't considered running
            TaskPool[i].RemoveAll(item => item.IsCompleted);
            NumRunningTasks[i] = TaskPool[i].Count(task => task.Status == TaskStatus.Running);

            // Run any new tasks if possible
            var numTasksToRun = MaxNumRunningTasks[i] - NumRunningTasks[i];
            if (numTasksToRun <= 0) continue;

            var pool = TaskPool[i];
            foreach (var task in pool)
            {
                if (task.Status == TaskStatus.Created)
                {
                    task.Start();
                    numTasksToRun--;
                }

                if (numTasksToRun == 0) break;
            }
        }
    }

    private static (int, int) GetMaxThreads()
    {
        ThreadPool.GetMaxThreads(out var worker, out var io);
        return (worker, io);
    }
}