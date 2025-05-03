namespace Panel.Domain.Common
{
	public class BaseResponse
	{
		public bool isSuccess = true;
		public string message;

		public BaseResponse(string message)
		{
			this.message = message;
		}

		public BaseResponse(bool isSuccess, string message)
		{
			this.isSuccess = isSuccess;
			this.message = message;
		}
	}
}
