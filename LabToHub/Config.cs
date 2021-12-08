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
        public const long HUB_REPO_ID = 436397521;

        public static Dictionary<string, string> LABEL_MAP = new Dictionary<string, string>
        {
            { "flow::To be Discussed", "To be Discussed" },
            { "flow::Merge Requested", "Merge Requested" },
            { "DOC", "documentation" }, // documentation is a default label in github
        };

    }
}
