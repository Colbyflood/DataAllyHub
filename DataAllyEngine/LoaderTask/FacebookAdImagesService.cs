using DataAllyEngine.Common;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;
using FacebookLoader.Content;
using FacebookLoader.Loader.AdImage;

namespace DataAllyEngine.LoaderTask;

public class FacebookAdImagesService : FacebookServiceBase
{
    public FacebookAdImagesService(Channel channel, ILoaderProxy loaderProxy, FacebookParameters facebookParameters, ILogging logging)
        : base(channel, loaderProxy, facebookParameters, logging)
    {
    }

    public async Task<FbRunLog> InitiateAdImagesLoad(string scopeType, int? backfillDays, int fbSaveContentId)
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

        var fbSaveContent = loaderProxy.GetFbSaveContentById(fbSaveContentId);
        if (fbSaveContent != null)
        {
            fbSaveContent.AdImageRunlogId = runlog.Id;
            loaderProxy.WriteFbSaveContent(fbSaveContent);
        }

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
            int contentCount = response.Content.Count;
            int batchSize = MAX_FB_STAGING_RECORDS_IN_ROWS;

            int totalBatches = (contentCount + batchSize - 1) / batchSize;

            for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                var batch = response.Content.Skip(batchIndex * batchSize).Take(batchSize).ToList<FacebookAdImage>();

                var content = new FacebookAdImagesResponse(batch);

                var runStaging = new FbRunStaging();
                runStaging.FbRunlogId = runlog.Id;
                runStaging.Sequence = GetNextSequence(runlog);
                runStaging.Content = content.ToJson();
                runStaging.RowsCount = batch.Count;

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

    public async Task<bool> ResumeAdImagesLoad(FbRunLog runlog, string url)
    {
        logging.LogInformation($"Resuming and processing ad images for scope {runlog.ScopeType} in runlog {runlog.Id}");

        runlog.LastStartedUtc = DateTime.UtcNow;
        loaderProxy.WriteFbRunLog(runlog);

        var loader = new AdImagesLoader(facebookParameters, logging);
        var response = await loader.LoadAsync(url);

        if (response == null)
        {
            logging.LogError($"Failed to load ad images on resume and response is null for {runlog.Id}");
            LogProblem(runlog.Id, Names.FB_PROBLEM_INTERNAL_PROBLEM, DateTime.UtcNow, null, null);
            return false;
        }

        if (response.Content.Count > 0)
        {
            int contentCount = response.Content.Count;
            int batchSize = MAX_FB_STAGING_RECORDS_IN_ROWS;

            int totalBatches = (contentCount + batchSize - 1) / batchSize;

            for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                var batch = response.Content.Skip(batchIndex * batchSize).Take(batchSize).ToList<FacebookAdImage>();

                var content = new FacebookAdImagesResponse(batch);

                var runStaging = new FbRunStaging();
                runStaging.FbRunlogId = runlog.Id;
                runStaging.Sequence = GetNextSequence(runlog);
                runStaging.Content = content.ToJson();
                runStaging.RowsCount = batch.Count;

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

            LogProblem(runlog.Id, reason, DateTime.UtcNow, response.RestartUrl, response.ExceptionBody);
        }

        return response.IsSuccessful;
    }
}