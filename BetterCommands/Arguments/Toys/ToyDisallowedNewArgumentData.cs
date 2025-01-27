using System;
using AdminToys;

namespace BetterCommands.Arguments.Toys;

public struct ToyDisallowedNewArgumentData
{
	public AdminToyBase Toy { get; }

	public ToyDisallowedNewArgumentData(AdminToyBase toy)
	{
		Toy = toy;
	}

	public bool IfIs<TToy>(Action<TToy> execute) where TToy : AdminToyBase
	{
		if (!(Toy is TToy obj))
		{
			return false;
		}
		execute?.Invoke(obj);
		return true;
	}
}
