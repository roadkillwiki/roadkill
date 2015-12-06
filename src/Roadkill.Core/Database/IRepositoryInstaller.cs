using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mindscape.LightSpeed.Caching;
using Roadkill.Core.Configuration;
using Roadkill.Core.DependencyResolution;
using StructureMap.Web;

namespace Roadkill.Core.Database
{
	public interface IRepositoryInstaller
	{
		void Install();
		void TestConnection();
	}
}
