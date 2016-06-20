using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace EchoServer.Controllers
{
	internal class MainController : IController
	{
		private IEnumerable<Task> _tasks;

		public MainController()
		{
			_tasks = CreateMainTasks();
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Task> CreateMainTasks()
		{
			return Enumerable.Empty<Task>();
		}
	}
}
