using DataAllyEngine.Models;

namespace DataAllyEngine.ContentProcessingTask;

public interface IContentProcessor
{
	void ProcessContentFor(Channel channel, FbRunLog runlog, FbSaveContent fbSaveContent);
}