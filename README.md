# labtohub (GitLab to GitHub)
Simple script/tool to migrate issues, milestones and merge requests from a GitLab project to a GitHub repository.

## Usage:

1. Create new project in GitHub
2. Push git main branch to new project (from an already cloned copy of the GitLab project)
    ```
    cd LOCAL_CLONE_PATH
    git reset --hard
    git checkout GITLAB_MAIN_BRANCH_NAME
    git pull
    git remote rename origin gitlab
    git remote add origin git@github.com:GITHUB_REPO_OWNER/GITHUB_REPO_NAME.git
    git branch -M main
    git push -u origin main
    ```
4. Clone this repo
    ```
    cd ..
    git clone git@github.com:AsgerIversen/labtohub.git
    ```
6. Open project in you favorite C# IDE
8. Change values in Config.cs
9. Compile
10. Run (maybe with debugger attached ;))
