using GitLabApiClient;
using Octokit;
using System.Globalization;
using LabToHub;

// Get all issues from gitlab project
var gitlab = new GitLabClient("https://gitlab.com", Config.GITLAB_ACCESS_TOKEN);
var labIssues = await gitlab.Issues.GetAllAsync(Config.GITLAB_REPO_ID);
labIssues = labIssues.OrderBy(x => x.Iid).ToList(); // add issue to github in the same order as they were added in gitlab

var github = new GitHubClient(new ProductHeaderValue("LabToHub"));
github.Credentials = new Credentials(Config.GITHUB_ACCESS_TOKEN);
//var repo = await github.Repository.Get("Keysight", "opentap");  // id = 436397521

var hubIssues = await github.Issue.GetAllForRepository(Config.GITHUB_REPO_ID, new RepositoryIssueRequest());


var hubMilestones = await github.Issue.Milestone.GetAllForRepository(Config.GITHUB_REPO_ID);
Dictionary<string,int> hubMilestoneId = hubMilestones.ToDictionary(m => m.Title, m => m.Number);

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
foreach (var li in labIssues)
{
    string migratedDescription = $"Originally filed {li.CreatedAt.ToString("MMMM dd yyyy")} by {li.Author.Name} on [GitLab]({li.WebUrl})" + Environment.NewLine + Environment.NewLine;
    migratedDescription += li.Description;

    // is this issue already in GitHub?
    var hi = hubIssues.FirstOrDefault(i => i.Title == li.Title);
    if (hi is not null)
    {
        if(hi.Body != migratedDescription)
        {
            var update = new IssueUpdate();
            update.Body = migratedDescription;
            await github.Issue.Update(Config.GITHUB_REPO_ID, hi.Number,update);
            Console.WriteLine($"Updated issue decription for  {hi.Title}");
        }
        continue;
    }

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
    Console.WriteLine($"Created issue {createdI.Title}");
}
