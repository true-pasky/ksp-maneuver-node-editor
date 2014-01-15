
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amne
{

#if DEBUG
	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class _EditorAny : AmneBehaviour
	{
		protected override string StartupName { get { return "EditorAny"; } }
		protected override KSPAddon.Startup Startup { get { return KSPAddon.Startup.EditorAny; } }
		protected override bool InFlight { get { return false; } }
	}
#endif

#if DEBUG
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class _Instantly : AmneBehaviour
	{
		protected override string StartupName { get { return "Instantly"; } }
		protected override KSPAddon.Startup Startup { get { return KSPAddon.Startup.Instantly; } }
		protected override bool InFlight { get { return false; } }
	}
#endif

#if DEBUG
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class _MainMenu : AmneBehaviour
	{
		protected override string StartupName { get { return "MainMenu"; } }
		protected override KSPAddon.Startup Startup { get { return KSPAddon.Startup.MainMenu; } }
		protected override bool InFlight { get { return false; } }
	}
#endif


	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class _Flight : AmneBehaviour
	{
		protected override string StartupName { get { return "Flight"; } }
		protected override KSPAddon.Startup Startup { get { return KSPAddon.Startup.Flight; } }
		protected override bool InFlight { get { return true; } }
	}


#if DEBUG
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
	public class _PSystemSpawn : AmneBehaviour
	{
		protected override string StartupName { get { return "PSystemSpawn"; } }
		protected override KSPAddon.Startup Startup { get { return KSPAddon.Startup.PSystemSpawn; } }
		protected override bool InFlight { get { return false; } }
	}
#endif
}
