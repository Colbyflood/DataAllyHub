using DataAllyEngine.Models;
using FacebookLoader.Common;

namespace DataAllyEngine.LoaderTask;

public interface ILoaderRunner
{
	void StartAdCreativesLoad(FacebookParameters facebookParameters, Channel channel, string scopeType);

	void ResumeAdCreativesLoad(FacebookParameters facebookParameters, FbRunLog runlog, string url);

	void StartAdImagesLoad(FacebookParameters facebookParameters, Channel channel, string scopeType);

	void ResumeAdImagesLoad(FacebookParameters facebookParameters, FbRunLog runlog, string url);

	void StartAdInsightsLoad(FacebookParameters facebookParameters, Channel channel, string scopeType, DateTime startDate, DateTime endDate);

	void ResumeAdInsightsLoad(FacebookParameters facebookParameters, FbRunLog runlog, string url);
}