# RepoManager
RepoManager is an AI-powered application designed to streamline project and repository management. Leveraging advanced AI agents, it provides a conversational interface that accepts user requests to perform a wide range of code, file, and GitHub repository operations. Whether you need to manage files locally, read or update project code, or maintain GitHub repositories (committing, pushing, pull requests, and more), RepoManager simplifies these tasks by coordinating intelligent agents on your behalf.

### Key Features
- Conversational interface to interact with Coding and GitHub agents  
- Automated project directory navigation and file management  
- Seamless integration for GitHub operations (repo maintenance, PRs, commits, etc.)  
- Intelligent hand-off between file/code operations and version control actions  
- User-centric feedback and robust error handling throughout all actions  

## Prerequisites

- [Install .NET SDK](https://dotnet.microsoft.com/en-us/download)
- [Install Node.js](https://nodejs.org/)
- [Create an OpenAI Key](https://platform.openai.com/account/api-keys) or [Create an Azure OpenAI Key](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/quickstart?pivots=rest-api)
- [Create a GitHub Personal Access Token](https://github.com/settings/tokens)

## Getting Started

Configure your `.env` file with the required environment variables:
```env
OPENAI_MODEL=your-model-id
OPENAI_KEY=your-openai-api-key
OPENAI_ENDPOINT=your-openai-endpoint
GITHUB_KEY=your-github-personal-access-token

# Optional: use Azure OpenAI instead of OpenAI
USE_AZURE_OPENAI=true
```

Clone the repository and run the program:
```bash
git clone https://github.com/elijah-tynes/repo-manager.git
cd repo-manager/RepoManager
dotnet run
```

On startup, the application will prompt for:

**Local project directory path**  
Example (Windows):  
```bash
C:\Users\yourname\Projects\MyApp
```

**GitHub repository link**  
Example:  
```bash
https://github.com/yourusername/MyApp
```

You will then be able to interactively enter natural language requests, which are automatically routed to the appropriate agent (Coding or GitHub) to take action.

## Samples
### Using the Coding Agent to Update Files
Modify a file:
![image](https://github.com/user-attachments/assets/a21a8252-ebce-4c8b-859d-b133332b97ed)
![image](https://github.com/user-attachments/assets/24b29e6c-4c22-4a87-8a97-eaa192e460ad)

List files in a directory (Used implicitly by agent for context gathering):
![image](https://github.com/user-attachments/assets/dbba7ce8-91ae-4623-9a08-d766ef4e66ee)

Cross-context file modifications:
![image](https://github.com/user-attachments/assets/135163a6-cb25-439f-81a9-641811a5630a)
![image](https://github.com/user-attachments/assets/c17640c5-8427-4d5f-9c8f-6759aae8d0d0)

> [!NOTE]
> Any file modifications provide the ability to revert changes, adding a layer of observability. 
>
> The agent will not read any .env files to adhere to user privacy.

### Using the GitHub Agent to Update a GitHub Repository
Enter GitHub request:
![image](https://github.com/user-attachments/assets/b31eee04-f124-4fc7-a865-ab6b58f7ea6d)

Provide confirmation of intent before any changes are made:
![image](https://github.com/user-attachments/assets/53210157-e9b3-4f1a-8a8d-66bb16a4879c)

---

This README was generated and pushed to the repository by RepoManager. 😉 
