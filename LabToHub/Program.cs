using GitLabApiClient;
using Octokit;
using System.Globalization;
using LabToHub;
using System.Text.RegularExpressions;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// Get all issues from gitlab project
var gitlab = new GitLabClient("https://gitlab.com", Config.GITLAB_ACCESS_TOKEN);
var labIssues = await gitlab.Issues.GetAllAsync(Config.GITLAB_REPO_ID);
labIssues = labIssues.OrderBy(x => x.Iid).ToList(); // add issue to github in the same order as they were added in gitlab

var github = new GitHubClient(new ProductHeaderValue("LabToHub"));
github.Credentials = new Credentials(Config.GITHUB_ACCESS_TOKEN);
//var repo = await github.Repository.Get("Keysight", "opentap");  // id = 436397521

var hubIssues = await github.Issue.GetAllForRepository(Config.GITHUB_REPO_ID, new RepositoryIssueRequest{ State = ItemStateFilter.All });


var hubMilestones = await github.Issue.Milestone.GetAllForRepository(Config.GITHUB_REPO_ID);
Dictionary<string,int> hubMilestoneId = hubMilestones.ToDictionary(m => m.Title, m => m.Number);

Dictionary<int, int> issueIdMap = new Dictionary<int, int>();
foreach (var li in labIssues)
{
    // is this issue already in GitHub (based on description)
    var hi = hubIssues.FirstOrDefault(i => i.Body is not null && i.Body.Contains("[GitLab](" + li.WebUrl + ")"));
    if (hi is not null)
    {
        issueIdMap.Add(li.Iid, hi.Number);
    }
}

foreach (var li in labIssues)
{
    string migratedDescription = $"Originally filed {li.CreatedAt.ToString("MMMM dd yyyy")} by {li.Author.Name} on [GitLab]({li.WebUrl})" + Environment.NewLine + Environment.NewLine;
    migratedDescription += TransformMarkdown(li.Description);

    // is this issue already in GitHub (based on description)
    var hi = hubIssues.FirstOrDefault(i => i.Body is not null && i.Body.Contains("[GitLab](" + li.WebUrl + ")"));
    if (hi is not null)
    {
        // update issue already found
        if (hi.Body != migratedDescription || hi.Title != li.Title)
        {
            var update = new IssueUpdate();
            update.Body = migratedDescription;
            update.Title = li.Title;
            await github.Issue.Update(Config.GITHUB_REPO_ID, hi.Number, update);

            Console.WriteLine($"Updated issue '{hi.Title}'");
        }
        continue;
    }

    // is this issue already in GitHub? (based on title)
    //hi = hubIssues.FirstOrDefault(i => i.Title == li.Title);
    //if (hi is not null)
    //{
    //    if(hi.Body != migratedDescription)
    //    {
    //        var update = new IssueUpdate();
    //        update.Body = migratedDescription;
    //        await github.Issue.Update(Config.GITHUB_REPO_ID, hi.Number,update);
    //        Console.WriteLine($"Updated issue decription for  {hi.Title}");
    //    }
    //    continue;
    //}

    NewIssue newi = new NewIssue(li.Title);
    newi.Body = migratedDescription;
    li.Labels.ForEach(l => newi.Labels.Add(Config.LABEL_MAP.ContainsKey(l) ? Config.LABEL_MAP[l] : l ));
    //i.Assignees.ForEach(a => newi.Assignees.Add(i.))
    if (li.Milestone is not null)
    {
        if (!hubMilestoneId.ContainsKey(li.Milestone.Title))
        {
            NewMilestone newm = new NewMilestone(li.Milestone.Title);
            newm.Description = li.Milestone.Description;
            if(li.Milestone.DueDate is not null)
                newm.DueOn = DateTimeOffset.Parse(li.Milestone.DueDate);
            newm.State = li.Milestone.State == GitLabApiClient.Models.Milestones.Responses.MilestoneState.Closed ? ItemState.Closed : ItemState.Open;
            var createdM = await github.Issue.Milestone.Create(Config.GITHUB_REPO_ID, newm);
            Console.WriteLine($"Created milestone {createdM.Title}");
            hubMilestoneId.Add(li.Milestone.Title, createdM.Number);
        }
        newi.Milestone = hubMilestoneId[li.Milestone.Title];
    }


    var createdI = await github.Issue.Create(Config.GITHUB_REPO_ID, newi);
    Console.WriteLine($"Created issue '{createdI.Title}'");
    issueIdMap.Add(li.Iid, createdI.Number);
    Thread.Sleep(1000); // wait one second between create requests as per GitHub API docs here: https://docs.github.com/en/rest/guides/best-practices-for-integrators#dealing-with-secondary-rate-limits
}


// --- Merge requests ---
// Get all MRs from gitlab project
var labmrs = await gitlab.MergeRequests.GetAsync(Config.GITLAB_REPO_ID);
labmrs = labmrs.OrderBy(x => x.Iid).ToList(); // add issue to github in the same order as they were added in gitlab

var hubprs = await github.PullRequest.GetAllForRepository(Config.GITHUB_REPO_ID, new PullRequestRequest { State = ItemStateFilter.All });

//var prj = await gitlab.Projects.GetAsync(Config.GITLAB_REPO_ID);
//var labRepoUrl = prj.HttpUrlToRepo;
//var gitDir = Path.GetFullPath("gitTree");
//System.Diagnostics.Process.Start("git", "clone ");

foreach (var mr in labmrs)
{
    if (mr.SourceProjectId != Config.GITLAB_REPO_ID)
    {
        Console.WriteLine($"Skipping !{mr.Iid} because it is from a fork.");
        continue;
    }

    string target = mr.TargetBranch;
    if (target == "master") target = "main";
    var pr = new NewPullRequest(mr.Title, mr.SourceBranch, target);
    string migrationHeader = $"Originally filed {mr.CreatedAt.ToString("MMMM dd yyyy")} by {mr.Author.Name} on [GitLab]({mr.WebUrl})" + Environment.NewLine + Environment.NewLine;
    pr.Body = migrationHeader + TransformMarkdown(mr.Description);

    // is this already in GitHub (based on description)
    var existing = hubprs.FirstOrDefault(i => i.Body is not null && i.Body.Contains("[GitLab](" + mr.WebUrl + ")"));
    if (existing is not null)
    {
        // update PR already found
        if (existing.Body != pr.Body || existing.Title != mr.Title)
        {
            var update = new PullRequestUpdate();
            update.Body = pr.Body;
            update.Title = pr.Title;
            await github.PullRequest.Update(Config.GITHUB_REPO_ID, existing.Number, update);

            Console.WriteLine($"Updated pull request '{mr.Title}'");
        }
        continue;
    }

    // Checkout the branch from gitlab and push it to github
    // (hack that assumes a bunch of things about an already clone tree in git\opentap)
    Directory.SetCurrentDirectory("c:\\git\\opentap");
    System.Diagnostics.Process.Start("git", $"checkout {mr.SourceBranch}").WaitForExit(10000);
    System.Diagnostics.Process.Start("git", $"push khub").WaitForExit(10000);
    Thread.Sleep(500);

    // Create the pull request
    var createdPr = await github.PullRequest.Create(Config.GITHUB_REPO_ID, pr);
    Console.WriteLine($"Created pull request '{mr.Title}'");
}

/// <summary>
/// Helper method to replace issue links and user names
/// </summary>
string TransformMarkdown(string markdown)
{
    string result = markdown;
    
    // replace user names
    foreach (var user in Config.USER_MAP)
    {
        result = result.Replace("@" + user.Key, "@" + user.Value);
    }

    // Replace issue numbers with ones known to github
    string labIssueBaseUrl = labIssues.First().WebUrl.Substring(0, labIssues.First().WebUrl.LastIndexOf('/'));
    result = Regex.Replace(result, @"#(\d+)", m => {
        int iid = int.Parse(m.Groups[1].Value);
        if (issueIdMap.ContainsKey(iid))
            return "#" + issueIdMap[iid];
        else
            return $"[#{iid}]({labIssueBaseUrl}/{iid})";
        });

    return result;
}