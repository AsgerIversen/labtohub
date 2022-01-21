using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabToHub
{
    static class Config
    {
        public const string GITLAB_ACCESS_TOKEN = "";
        public const string GITHUB_ACCESS_TOKEN = "";

        // Get this using:
        //   var id = await github.Repository.Get("Keysight", "opentap").Id;  // keysight/opentap id = 436397521
        public const long GITHUB_REPO_ID = 436397521;
        public const int GITLAB_REPO_ID = 10858059;

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
