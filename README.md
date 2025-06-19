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
Clone the repository:
```bash
git clone https://github.com/elijah-tynes/RepoManager.git
cd RepoManager
```

Configure your `.env` file with the required environment variables:
```env
OPENAI_MODEL=your-model-id
OPENAI_KEY=your-openai-api-key
OPENAI_ENDPOINT=your-openai-endpoint
GITHUB_KEY=your-github-personal-access-token

# Optional: use Azure OpenAI instead of OpenAI
USE_AZURE_OPENAI=true
```

Run the program:
```bash
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

---

This README was generated and pushed to the repository by RepoManager. 😉 
