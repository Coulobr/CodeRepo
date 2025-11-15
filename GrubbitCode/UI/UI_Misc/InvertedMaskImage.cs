using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace Grubbit
{
	public class InvertedMaskImage : Image
	{
		public override Material materialForRendering
		{
			get
			{
				Material result = new Material(base.materialForRendering);
				result.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
				return result;
			}
		}
	}
}
