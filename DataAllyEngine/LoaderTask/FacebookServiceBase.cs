using Amazon.Runtime.Internal.Util;
using DataAllyEngine.Models;
using DataAllyEngine.Proxy;
using FacebookLoader.Common;

namespace DataAllyEngine.LoaderTask;

public abstract class FacebookServiceBase
{
    protected readonly Channel channel;
    protected readonly ILoaderProxy loaderProxy;
    protected readonly FacebookParameters facebookParameters;
    protected readonly ILogging logging;

    // ReSharper disable once InconsistentNaming
    protected const int RESTART_TIME_MINUTES = 60;
    protected const int MAX_FB_STAGING_RECORDS_IN_ROWS = 250;

    protected FacebookServiceBase(Channel channel, ILoaderProxy loaderProxy, FacebookParameters facebookParameters, ILogging logging)
    {
        this.channel = channel;
        this.loaderProxy = loaderProxy;
        this.facebookParameters = facebookParameters;
        this.logging = logging;
    }

    protected void LogProblem(int runLogId, string reason, DateTime now, string? restartUrl, string? fbErrorResponse)
    {
        var problem = new FbRunProblem();
        problem.FbRunlogId = runLogId;
        problem.Reason = reason;
        problem.CreatedUtc = now;
        if (!string.IsNullOrEmpty(restartUrl))
        {
            problem.RestartUrl = restartUrl;
            problem.RestartAfterUtc = DateTime.UtcNow.AddMinutes(FacebookServiceBase.RESTART_TIME_MINUTES);
        }

        problem.FbErrorResponse = fbErrorResponse;

        loaderProxy.WriteFbRunProblem(problem);
    }


    protected int GetNextSequence(FbRunLog runlog)
    {
        return loaderProxy.GetNextSequenceByRunlogId(runlog.Id);
    }

}