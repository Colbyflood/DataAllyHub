using DataAllyEngine.Common;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;
using FacebookLoader.Loader.AdImage;

namespace DataAllyEngine.LoaderTask;

public class FacebookAdImagesService : FacebookServiceBase
{
    public FacebookAdImagesService(Channel channel, ILoaderProxy loaderProxy, FacebookParameters facebookParameters, ILogging logging)
        : base(channel, loaderProxy, facebookParameters, logging)
    {
    }

    public async Task<FbRunLog> InitiateAdImagesLoad(string scopeType, int? backfillDays)
    {
        logging.LogInformation($"Requesting loading of ad images for channel {channel.Id} in scope {scopeType}");

        var runlog = new FbRunLog();
        runlog.ChannelId = channel.Id;
        runlog.FeedType = Names.FEED_TYPE_AD_IMAGE;
        runlog.ScopeType = scopeType;
        runlog.StartedUtc = DateTime.UtcNow;
        runlog.LastStartedUtc = runlog.StartedUtc;
        runlog.BackfillDays = backfillDays;
        loaderProxy.WriteFbRunLog(runlog);

        var success = await StartAdImagesLoad(runlog);
        if (success)
        {
            runlog.FinishedUtc = DateTime.UtcNow;
            loaderProxy.WriteFbRunLog(runlog);
        }

        return runlog;
    }

    public async Task<bool> StartAdImagesLoad(FbRunLog runlog)
    {
        logging.LogInformation($"Requesting and processing ad images for scope {runlog.ScopeType} in runlog {runlog.Id}");

        runlog.LastStartedUtc = DateTime.UtcNow;
        loaderProxy.WriteFbRunLog(runlog);

        var loader = new AdImagesLoader(facebookParameters, logging);
        var response = await loader.StartLoadAsync();

        if (response == null)
        {
            logging.LogError($"Failed to load ad images and response is null for {runlog.Id}");
            LogProblem(runlog.Id, Names.FB_PROBLEM_INTERNAL_PROBLEM, DateTime.UtcNow, null, null);
            return false;
        }

        if (response.Content.Count > 0)
        {
            var content = response.ToJson();

            var runStaging = new FbRunStaging();
            runStaging.FbRunlogId = runlog.Id;
            runStaging.Sequence = GetNextSequence(runlog);
            runStaging.Content = content;

            loaderProxy.WriteFbRunStaging(runStaging);
        }

        if (response.IsSuccessful)
        {
            runlog.FinishedUtc = DateTime.UtcNow;
            loaderProxy.WriteFbRunLog(runlog);
        }
        else
        {
            var reason = Names.FB_PROBLEM_THROTTLED;
            if (response.NotPermitted)
                reason = Names.FB_PROBLEM_NOT_PERMITTED;
            else if (response.TokenExpired)
                reason = Names.FB_PROBLEM_BAD_TOKEN;

            LogProblem(runlog.Id, reason, DateTime.UtcNow, response.RestartUrl, response.ExceptionBody);
        }

        return response.IsSuccessful;
    }

    public async Task<bool> ResumeAdImagesLoad(FbRunLog runlog, string url)
    {
        logging.LogInformation($"Resuming and processing ad images for scope {runlog.ScopeType} in runlog {runlog.Id}");

        runlog.LastStartedUtc = DateTime.UtcNow;
        loaderProxy.WriteFbRunLog(runlog);

        var loader = new AdImagesLoader(facebookParameters, logging);
        var response = await loader.LoadAsync(url);

        if (response.Content.Count > 0)
        {
            var content = response.ToJson();

            var runStaging = new FbRunStaging();
            runStaging.FbRunlogId = runlog.Id;
            runStaging.Sequence = GetNextSequence(runlog);
            runStaging.Content = content;

            loaderProxy.WriteFbRunStaging(runStaging);
        }

        if (response.IsSuccessful)
        {
            runlog.FinishedUtc = DateTime.UtcNow;
            loaderProxy.WriteFbRunLog(runlog);
        }
        else
        {
            var reason = Names.FB_PROBLEM_THROTTLED;
            if (response.NotPermitted)
                reason = Names.FB_PROBLEM_NOT_PERMITTED;
            else if (response.TokenExpired)
                reason = Names.FB_PROBLEM_BAD_TOKEN;

            LogProblem(runlog.Id, reason, DateTime.UtcNow, response.RestartUrl, response.ExceptionBody);
        }

        return response.IsSuccessful;
    }
}