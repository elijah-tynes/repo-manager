# RepoManager

## Overview
RepoManager is an AI-powered application designed to streamline project and repository management. Leveraging advanced AI agents, it provides a conversational interface that accepts user requests to perform a wide range of code, file, and GitHub repository operations. Whether you need to manage files locally, read or update project code, or maintain GitHub repositories (committing, pushing, pull requests, and more), RepoManager simplifies these tasks by coordinating intelligent agents on your behalf.

### Key Features
- Conversational interface to interact with Coding and GitHub agents.
- Automated project directory navigation and file management
- Seamless integration for GitHub operations (repo maintenance, PRs, commits, etc.)
- Intelligent hand-off between file/code operations and version control actions
- User-centric feedback and robust error handling throughout all actions

## Usage
- Configure `.env` with all the required variables (`OPENAI_MODEL`, `OPENAI_ENDPOINT`, `OPENAI_KEY`, `GITHUB_KEY`). Optionally, include the env variable `USE_AZURE_OPENAI=true` if you want to use Azure OpenAI instead of OpenAI endpoints.
- On startup, provide paths for your project directory and your desired GitHub repository link.
- Interactively enter requests, which will be managed by the appropriate AI agent.

---

This README was generated and pushed to the repository by RepoManager. 😉 
