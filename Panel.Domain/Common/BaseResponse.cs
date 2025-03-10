using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Shared
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
