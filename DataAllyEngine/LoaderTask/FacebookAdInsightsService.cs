using DataAllyEngine.Common;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;
using FacebookLoader.Content;
using FacebookLoader.Loader.AdInsight;

namespace DataAllyEngine.LoaderTask;

public class FacebookAdInsightsService : FacebookServiceBase
{
    public FacebookAdInsightsService(Channel channel, ILoaderProxy loaderProxy, FacebookParameters facebookParameters, ILogging logging)
        : base(channel, loaderProxy, facebookParameters, logging)
    {
    }

    public async Task<FbRunLog> InitiateAdInsightsLoad(string scopeType, DateTime startDate, DateTime endDate, int? backfillDays, int fbSaveContentId)
    {
        logging.LogInformation($"Requesting loading of ad insights for channel {channel.Id} in scope {scopeType} between {startDate} and {endDate}");

        var runlog = new FbRunLog();
        runlog.ChannelId = channel.Id;
        runlog.FeedType = Names.FEED_TYPE_AD_INSIGHT;
        runlog.ScopeType = scopeType;
        runlog.StartedUtc = DateTime.UtcNow;
        runlog.LastStartedUtc = runlog.StartedUtc;
        runlog.BackfillDays = backfillDays;
        loaderProxy.WriteFbRunLog(runlog);


        var fbSaveContent = loaderProxy.GetFbSaveContentById(fbSaveContentId);
        if (fbSaveContent != null)
        {
            fbSaveContent.AdInsightRunlogId = runlog.Id;
            loaderProxy.WriteFbSaveContent(fbSaveContent);
        }

        var success = await StartAdInsightsLoad(runlog, startDate, endDate);
        if (success)
        {
            runlog.FinishedUtc = DateTime.UtcNow;
            loaderProxy.WriteFbRunLog(runlog);
        }

        return runlog;
    }

    public async Task<bool> StartAdInsightsLoad(FbRunLog runlog, DateTime startDate, DateTime endDate)
    {
        logging.LogInformation($"Requesting and processing ad insights for scope {runlog.ScopeType} in runlog {runlog.Id}");

        runlog.LastStartedUtc = DateTime.UtcNow;
        loaderProxy.WriteFbRunLog(runlog);

        var loader = new AdInsightsLoader(facebookParameters, logging);
        var response = await loader.StartLoadAsync(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        if (response == null)
        {
            logging.LogError($"Failed to load ad insights and response is null for {runlog.Id}");
            LogProblem(runlog.Id, Names.FB_PROBLEM_INTERNAL_PROBLEM, DateTime.UtcNow, null, null);
            return false;
        }

        if (response.Content.Count > 0)
        {
            int contentCount = response.Content.Count;
            int batchSize = 250;

            int totalBatches = (contentCount + batchSize - 1) / batchSize;

            for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                var batch = response.Content.Skip(batchIndex * batchSize).Take(batchSize).ToList<FacebookAdInsight>();

                var content = new FacebookAdInsightsResponse(batch);

                var runStaging = new FbRunStaging();
                runStaging.FbRunlogId = runlog.Id;
                runStaging.Sequence = GetNextSequence(runlog);
                runStaging.Content = content.ToJson();

                loaderProxy.WriteFbRunStaging(runStaging);
            }
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
            else if (response.TemporaryDowntime)
                reason = Names.FB_PROBLEM_TEMPORARY_DOWNTIME;

            LogProblem(runlog.Id, reason, DateTime.UtcNow, response.RestartUrl, response.ExceptionBody);
        }

        return response.IsSuccessful;
    }

    public async Task<bool> ResumeAdInsightsLoad(FbRunLog runlog, string url)
    {
        logging.LogInformation($"Resuming and processing ad insights for scope {runlog.ScopeType} in runlog {runlog.Id}");

        runlog.LastStartedUtc = DateTime.UtcNow;
        loaderProxy.WriteFbRunLog(runlog);

        var loader = new AdInsightsLoader(facebookParameters, logging);
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