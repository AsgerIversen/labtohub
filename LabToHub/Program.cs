using GitLabApiClient;
using Octokit;
using System.Globalization;
using LabToHub;
using System.Text.RegularExpressions;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// Get all issues from gitlab project
var gitlab = new GitLabClient("http://gitlab.it.keysight.com", Config.GITLAB_ACCESS_TOKEN);
var labIssues = await gitlab.Issues.GetAllAsync(Config.GITLAB_REPO_ID);
labIssues = labIssues.OrderBy(x => x.Iid).ToList(); // add issue to github in the same order as they were added in gitlab

var github = new GitHubClient(new ProductHeaderValue("LabToHub"));
github.Credentials = new Credentials(Config.GITHUB_ACCESS_TOKEN);
var hubRepo = await github.Repository.Get(Config.GITHUB_REPO_OWNER, Config.GITHUB_REPO_NAME);  // id = 436397521

var hubIssues = await github.Issue.GetAllForRepository(hubRepo.Id, new RepositoryIssueRequest{ State = ItemStateFilter.All });


var hubMilestones = await github.Issue.Milestone.GetAllForRepository(hubRepo.Id);
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
        if (hi.Body != migratedDescription ||
            hi.Title != li.Title
            // || (hi.State == ItemState.Open) != (li.State == GitLabApiClient.Models.Issues.Responses.IssueState.Opened)
            )
        {
            var update = new IssueUpdate();
            update.Body = migratedDescription;
            update.Title = li.Title;
            //update.State = li.State == GitLabApiClient.Models.Issues.Responses.IssueState.Opened ? ItemState.Open : ItemState.Closed;
            await github.Issue.Update(hubRepo.Id, hi.Number, update);

            Console.WriteLine($"Updated issue '{hi.Title}'");
        }
    }
    else
    {

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
        li.Labels.ForEach(l => newi.Labels.Add(Config.LABEL_MAP.ContainsKey(l) ? Config.LABEL_MAP[l] : l));
        //i.Assignees.ForEach(a => newi.Assignees.Add(i.))
        if (li.Milestone is not null)
        {
            if (!hubMilestoneId.ContainsKey(li.Milestone.Title))
            {
                NewMilestone newm = new NewMilestone(li.Milestone.Title);
                newm.Description = li.Milestone.Description;
                if (li.Milestone.DueDate is not null)
                    newm.DueOn = DateTimeOffset.Parse(li.Milestone.DueDate);
                newm.State = li.Milestone.State == GitLabApiClient.Models.Milestones.Responses.MilestoneState.Closed ? ItemState.Closed : ItemState.Open;
                var createdM = await github.Issue.Milestone.Create(hubRepo.Id, newm);
                Console.WriteLine($"Created milestone {createdM.Title}");
                hubMilestoneId.Add(li.Milestone.Title, createdM.Number);
            }
            newi.Milestone = hubMilestoneId[li.Milestone.Title];
        }


        hi = await github.Issue.Create(hubRepo.Id, newi);
        Console.WriteLine($"Created issue '{hi.Title}'");
        issueIdMap.Add(li.Iid, hi.Number);
        Thread.Sleep(1000); // wait one second between create requests as per GitHub API docs here: https://docs.github.com/en/rest/guides/best-practices-for-integrators#dealing-with-secondary-rate-limits
    }

    if (!li.Labels.Contains("MigratedToGitHub"))
    {
        var newComment = new GitLabApiClient.Models.Notes.Requests.CreateIssueNoteRequest($"This issue has moved to Github [here]({hi.HtmlUrl}).");
        await gitlab.Issues.CreateNoteAsync(li.ProjectId, li.Iid, newComment);

        var liUpdate = new GitLabApiClient.Models.Issues.Requests.UpdateIssueRequest
        {
            //State = GitLabApiClient.Models.Issues.Requests.UpdatedIssueState.Close
            Labels = li.Labels.Concat(new[] { "MigratedToGitHub" }).ToList()
        };
        await gitlab.Issues.UpdateAsync(li.ProjectId, li.Iid, liUpdate);
        Console.WriteLine($"Added migrated label to #{li.Iid} ({hi.Title}).");
    }
}


// --- Merge requests ---
// Get all MRs from gitlab project
var labmrs = await gitlab.MergeRequests.GetAsync(Config.GITLAB_REPO_ID);
labmrs = labmrs.OrderBy(x => x.Iid).ToList(); // add issue to github in the same order as they were added in gitlab

var hubprs = await github.PullRequest.GetAllForRepository(hubRepo.Id, new PullRequestRequest { State = ItemStateFilter.All });

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
    if (target == Config.GITLAB_MAIN_BRANCH_NAME) target = "main";
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
            await github.PullRequest.Update(hubRepo.Id, existing.Number, update);

            Console.WriteLine($"Updated pull request '{mr.Title}'");
        }
        continue;
    }

    // Checkout the branch from gitlab and push it to github
    // (hack that assumes a bunch of things about an already clone tree in git\opentap)
    Directory.SetCurrentDirectory(Config.LOCAL_CLONE_PATH);
    System.Diagnostics.Process.Start("git", $"checkout {mr.SourceBranch}").WaitForExit(10000);
    System.Diagnostics.Process.Start("git", $"push origin").WaitForExit(10000);
    Thread.Sleep(2000);

    // Create the pull request
    var createdPr = await github.PullRequest.Create(hubRepo.Id, pr);
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

    // fix image links:
    result = result.Replace("](/uploads", $"]({Config.GITLAB_REPO_URL}/uploads");

    return result;
}