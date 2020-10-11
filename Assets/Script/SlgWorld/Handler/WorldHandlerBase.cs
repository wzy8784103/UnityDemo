using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class WorldHandlerBase : AutoReleaseBase
{
	public abstract void OnInit();
	
	public void Release()
	{
		ReleaseAll();
		OnRelease();
	}
	public virtual void OnRelease()
	{

	}
}
