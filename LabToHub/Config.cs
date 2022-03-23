using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabToHub
{
    static class Config
    {
        ///////////////////////////////////////////////////
        // GitLab project to migrate from:
        ///////////////////////////////////////////////////
        public const string GITLAB_REPO_NAME = "Campaign Plugin";
        public const string GITLAB_ACCESS_TOKEN = "ZrSa5ksxWq1AUcx2KTGc";
        // name of the main branch in gitlab.
        // When this name appears as a target for a MR, the target will be switched to "main"
        public const string GITLAB_MAIN_BRANCH_NAME = "integration";

        ///////////////////////////////////////////////////
        // GitHub repository to migrate to:
        ///////////////////////////////////////////////////
        public const string GITHUB_ACCESS_TOKEN = "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";
        public const string GITHUB_REPO_OWNER = "opentap"; // this is either your user name or the name of the organization that the repo is in
        public const string GITHUB_REPO_NAME = "campaign-plugin";


        ///////////////////////////////////////////////////
        // General:
        ///////////////////////////////////////////////////

        // This should be a local directory containing a clone of the gitlab project to be migrated.
        // The clone should have a two remotes, one for gitlab and one for github.
        // The github remote must be named "origin"
        public const string LOCAL_CLONE_PATH = @"C:\git\PackageRepositoryServer";

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
            { "sebastian.vlaic", "sebastian-pop" }
        };  
    }
}
