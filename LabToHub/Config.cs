using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabToHub
{
    static class Config
    {
        public const string GITLAB_ACCESS_TOKEN = "glpat-jVLNNxzR9yZsdv1KUs5i";
        public const string GITHUB_ACCESS_TOKEN = "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";

        // Get this using:
        //   var id = await github.Repository.Get("Keysight", "opentap").Id;  // keysight/opentap id = 436397521
        public const long GITHUB_REPO_ID = 436397521;
        public const int GITLAB_REPO_ID = 10858059;
        public const string GITLAB_REPO_URL = "https://gitlab.com/OpenTAP/opentap";  // no trailing slash

        public static Dictionary<string, string> LABEL_MAP = new Dictionary<string, string>
        {
            { "flow::To be Discussed", "To be Discussed" },
            { "flow::Merge Requested", "Merge Requested" },
            { "DOC", "documentation" }, // documentation is a default label in github
        };

        public static Dictionary<string, string> USER_MAP = new Dictionary<string, string>
        {
            { "asger", "AsgerIversen" },
            { "romadsen-ks", "rmadsen-ks" },
            { "StefanHolst", "StefanHolst" },
        };
    }
}
