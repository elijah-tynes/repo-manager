// ENV Import
using dotenv.net;

// MCP Import
using ModelContextProtocol.Client;

// Semantic Kernel Imports
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;

// Load env variables
DotEnv.Load();

// Populate values from your OpenAI deployment
var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL");
var endPoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");
var apiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
var githubKey = Environment.GetEnvironmentVariable("GITHUB_KEY");
var useAzureOpenAI = Environment.GetEnvironmentVariable("USE_AZURE_OPENAI");

// Ensure environment variables have been properly set
if (string.IsNullOrWhiteSpace(modelId) || string.IsNullOrWhiteSpace(endPoint) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(githubKey))
{
    throw new NullReferenceException("Set your environment variables OPENAI_MODEL, OPENAI_ENDPOINT, OPENAI_KEY, and GITHUB_KEY in a .env file in this program's directory");
}

// Create an MCPClient for the GitHub server
var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "MCPServer",
    Command = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-github"],
    EnvironmentVariables = new Dictionary<string, string?>
    {
        ["GITHUB_PERSONAL_ACCESS_TOKEN"] = $"{githubKey}"
    } 
});
var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

// Retrieve the list of tools available on the GitHub server
var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

// Create a kernel
var builder = Kernel.CreateBuilder();

// Determine whether to use AzureAI or OpenAI based on environment variables
if (!string.IsNullOrWhiteSpace(useAzureOpenAI) && bool.Parse(useAzureOpenAI.ToLower()) == true)
{
    builder.AddAzureOpenAIChatCompletion(modelId, endPoint, apiKey);
}
else
{
    builder.AddOpenAIChatCompletion(modelId, apiKey);
}

// Use the code below to enable detailed logging (function calls, token usage, etc.)
// builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace)); 

// Build the kernel with GitHub and FileIo plugins
Kernel kernel = builder.Build();
kernel.Plugins.AddFromFunctions("GitHub", tools.Select(aiFunction => aiFunction.AsKernelFunction()));
kernel.Plugins.AddFromType<FilePlugin>("Files");

// Interactive loop to set working directory
string? workingDirectory = null;
while (workingDirectory == null)
{
    Console.Write("Enter the file path of your project: ");
    string? inputDirectory = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(inputDirectory) && Directory.Exists(inputDirectory))
    {
        workingDirectory = inputDirectory;
    }
    else
    {
        Console.WriteLine("Invalid directory. Please enter a valid path.");
    }
}

// Interactive loop to set GitHub repository
string? githubRepository = null;
while (githubRepository == null)
{
    Console.Write("Enter your GitHub repository link: ");
    string? inputRepository = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(inputRepository))
    {
        githubRepository = inputRepository;
    }
    else
    {
        Console.WriteLine("Invalid repository. Please enter a valid GitHub repository link");
    }
}

// Enable automatic function calling
OpenAIPromptExecutionSettings executionSettings = new()
{
    Temperature = 0,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }),
};

// Define the coding agent
ChatCompletionAgent codingAgent = new()
{
    Name = "CodingAgent",
    Instructions = @$"
    You are a coding assistant that manages a working file:
    - Your primary role is to help the user read, edit, and manage files in the project directory.
    - If the user mentions a filename (e.g., 'What are the contents of testing.java?'), automatically set it as the working file using 'set_working_file'.
    - If no filename is mentioned, operate on the current working file.
    - If no working file is set, ask the user to specify one.
    - When modifying a file, automatically use 'update_file' on the current working file unless the user specifies a different file.
    - Keep in mind your current directory is {workingDirectory} when finding a filepath.
    - Recognize that files may be inside subdirectories within the working directory. Always check subdirectories when searching for files.
    - After making a write to a file, always prompt a user with 'Type revert to undo changes' where 'revert' undoes the changes you made to the working file.

    GitHubAgent Handoff:
    - If the user's message expresses an intent to perform a GitHub-related action (e.g., committing code, pushing changes, creating a pull request, managing branches or repositories), immediately hand off the conversation to the GitHubAgent.
    - Do not attempt to perform GitHub operations yourself.",
    Description = @"An intelligent coding assistant that manages and edits files within a specified project directory. It can read, update,
    and navigate files, including those in subdirectories. It automatically sets and operates on a working file based on user input, and
    prompts for clarification when needed. Ideal for assisting with code review, file management, and development tasks.",
    Kernel = kernel,
    Arguments = new KernelArguments(executionSettings),
};

// - If you are lacking sufficient context to complete a request, view all files to obtain context on how to fulfill the request using list_files and read_all_files.

// Define the GitHub agent
ChatCompletionAgent githubAgent = new()
{
    Name = "GitHubAgent",
    Instructions = @$"You are a GitHub agent that assists users with interacting with GitHub repositories. 
    - You can commit changes, create branches, and submit pull requests to a specified GitHub repository, {githubRepository}.
    - Use only the kernel functions provided in 'GitHub' from {tools}.
    - Always confirm the repository URL, branch name, and commit message with the user before performing any GitHub operation.
    - If the user has not specified a GitHub repository, ask them to provide one.
    - If the user asks to review changes before committing, list the modified files and their diffs.
    - Ensure that all GitHub actions are traceable and confirm success or failure after execution.
    - If you are unsure about the user's intent, ask clarifying questions before proceeding.

    CodingAgent Handoff:
    - If the user's input is not related to GitHub, clearly inform them and switch control back to the CodingAgent.",
    Description = @"A GitHub operations agent that handles version control tasks such as committing changes,
    creating branches, and submitting pull requests. It activates when the user mentions GitHub-related
    actions and ensures all operations are confirmed and traceable. If the input is unrelated to GitHub,
    it hands control back to the CodingAgent.",
    Kernel = kernel,
    Arguments = new KernelArguments(executionSettings),
};

// Create a list to store chat history
ChatHistory chatHistory = [];

// Define orchestration handoffs and conditions
var handoffs = OrchestrationHandoffs
    .StartWith(codingAgent)
    .Add(codingAgent, githubAgent, @"If the user's message is about GitHub actions such as 
                                    committing code, pushing changes, creating or reviewing
                                    pull requests, managing branches, or anything related to repositories.")
    .Add(githubAgent, codingAgent, @"If the user's message is not related to GitHub.");

// Define agent orchestration
HandoffOrchestration orchestration = new HandoffOrchestration(
    handoffs,
    codingAgent,
    githubAgent);

// Start runtime to manage agent orchestration
InProcessRuntime runtime = new InProcessRuntime();

// Event handler for killing program
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("\nExiting RepoManager session.");
    e.Cancel = true;
    Environment.Exit(0);
};

// Welcome to RepoManager message
Console.WriteLine("\n\n==================================================================");
Console.WriteLine("                      Welcome to RepoManager                     ");
Console.WriteLine();
Console.WriteLine("A multi-agent AI tool to assist in development and version control");
Console.WriteLine();
Console.WriteLine("==================================================================");

// Obtain the SetWorkingDirectory function from the kernel plugin
var setDirectoryFunction = kernel.Plugins
    .GetFunction("Files", "set_working_directory");

// Invoke SetWorkingDirectory(workingDirectory) to set a working directory within the FileIo plugin
var directoryFunctionResult = await setDirectoryFunction.InvokeAsync(kernel, new() { ["directoryPath"] = workingDirectory });

// Interactive loop
while (true)
{
    // Start runtime
    await runtime.StartAsync();

    // Take user request
    Console.Write("\nRequest (type 'Done' to exit): ");
    string? request = Console.ReadLine();

    // Exit condition
    if (string.Equals(request?.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Exiting RepoManager session.");
        break;
    }

    // Skip empty inputs
    if (string.IsNullOrWhiteSpace(request)) continue;

    try
    {
        // Create a user message and add it to chat history
        var userMessage = new ChatMessageContent
        {
            Role = AuthorRole.User,
            Content = request
        };
        chatHistory.Add(userMessage);

        // Retrieve the chat history
        string currentConversation = UpdateAgentHistory();

        // Get response from agent using chat history and user request
        var result = await orchestration.InvokeAsync(currentConversation, runtime);
        string response = await result.GetValueAsync(TimeSpan.FromSeconds(300));
        Console.WriteLine($"\nAI Agent Response:\n{response}");

        // Add agent response to chat history
        chatHistory.AddMessage(AuthorRole.System, response);

        // End runtime, recover resources
        await runtime.RunUntilIdleAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

// Concatenates chat history as a string to pass to agents
string UpdateAgentHistory()
{
    return string.Join("\n", chatHistory.Select(msg => $"{msg.Role.Label}: {msg.Content?.Trim()}"));
}