using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabToHub
{
    static class Config
    {
        public static bool Verify()
        {
            bool success = true;

            if (GITLAB_FULL_NAMESPACE == "YOUR_GITLAB_FULL_NAMESPACE_HERE")
            {
                success = false;
                Console.Error.WriteLine($"Pleasure configure 'Config.{nameof(GITLAB_FULL_NAMESPACE)}'");
            }

            if (GITLAB_PROJECT_NAME == "YOUR_GITLAB_PROJECT_NAME_HERE")
            {
                success = false;
                Console.Error.WriteLine($"Pleasure configure 'Config.{nameof(GITLAB_PROJECT_NAME)}'");
            }

            if (GITLAB_ACCESS_TOKEN == "YOUR_GITLAB_TOKEN_HERE")
            {
                success = false;
                Console.Error.WriteLine($"Pleasure configure 'Config.{nameof(GITLAB_ACCESS_TOKEN)}'");
            }

            if (LOCAL_CLONE_PATH == @"PATH_TO_LOCAL_REPO_HERE")
            {
                success = false;
                Console.Error.WriteLine($"Pleasure configure 'Config.{nameof(LOCAL_CLONE_PATH)}'");
            }

            if (GITHUB_ACCESS_TOKEN == "YOUR_GITHUB_TOKEN_HERE")
            {
                success = false;
                Console.Error.WriteLine($"Pleasure configure 'Config.{nameof(GITHUB_ACCESS_TOKEN)}'");
            }

            if (GITHUB_REPO_NAME == "YOUR_GITHUB_REPO_NAME_HERE")
            {
                success = false;
                Console.Error.WriteLine($"Pleasure configure 'Config.{nameof(GITHUB_REPO_NAME)}'");
            }

            return success;
        }
        ///////////////////////////////////////////////////
        // GitLab project to migrate from:
        ///////////////////////////////////////////////////
        public const string GITLAB_PROJECT_NAME = "YOUR_GITLAB_PROJECT_NAME_HERE";
        public const string GITLAB_FULL_NAMESPACE = "YOUR_GITLAB_FULL_NAMESPACE_HERE";
        public const string GITLAB_ACCESS_TOKEN = "YOUR_GITLAB_TOKEN_HERE";
        // name of the main branch in gitlab.
        // When this name appears as a target for a MR, the target will be switched to "main"
        public const string GITLAB_MAIN_BRANCH_NAME = "integration";

        ///////////////////////////////////////////////////
        // GitHub repository to migrate to:
        ///////////////////////////////////////////////////
        public const string GITHUB_ACCESS_TOKEN = "YOUR_GITHUB_TOKEN_HERE";
        public const string GITHUB_REPO_OWNER = "opentap"; // this is either your user name or the name of the organization that the repo is in
        public const string GITHUB_REPO_NAME = "YOUR_GITHUB_REPO_NAME_HERE";


        ///////////////////////////////////////////////////
        // General:
        ///////////////////////////////////////////////////

        // This should be a local directory containing a clone of the gitlab project to be migrated.
        // The clone should have a two remotes, one for gitlab and one for github.
        // The github remote must be named "origin"
        public const string LOCAL_CLONE_PATH = @"PATH_TO_LOCAL_REPO_HERE";

        // To rename any labels as part of the migration process, add them here 
        public static Dictionary<string, string> LABEL_MAP = new Dictionary<string, string>
        {
            { "flow::To be Discussed", "To be Discussed" },
            { "flow::Merge Requested", "Merge Requested" },
            { "DOC", "documentation" }, // documentation is a default label in github
        };

        // This map is used to migrate @ mentions in issue/MR descriptions
        public static Dictionary<string, string> USER_MAP = new Dictionary<string, string>
        {
            { "asger", "AsgerIversen" },
            { "romadsen-ks", "rmadsen-ks" },
            { "StefanHolst", "StefanHolst" },
            { "alexnlarsen", "alnlarsen" },
            { "enesozturk", "enes-ozturk" },
            { "sebastian.vlaic", "sebastian-pop" },
            { "dragos", "db-ks" }
        };  
    }
}
